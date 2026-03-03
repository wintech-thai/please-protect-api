using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Its.PleaseProtect.Api.ViewsModels;
using Its.PleaseProtect.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Its.PleaseProtect.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class EsController : ControllerBase
    {
        private readonly HttpClient _esClient;

        [ExcludeFromCodeCoverage]
        public EsController(IHttpClientFactory factory)
        {
            _esClient = factory.CreateClient("es-proxy");
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetIndexSetting/{indexName}")]
        public async Task<IActionResult> GetIndexSetting(string id, string indexName)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                return BadRequest("indexName is required");

            // optional: validate pattern ป้องกัน injection
            if (indexName.Contains("..") || indexName.Contains("/"))
                return BadRequest("invalid indexName");

            var requestUrl = $"/{indexName}/_settings";

            using var response = await _esClient.GetAsync(requestUrl);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, content);
            }

            // ส่ง JSON ตรง ๆ กลับไปเลย
            return Content(content, "application/json");
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetIndices")]
        public async Task<IActionResult> GetIndices(string id, [FromBody] VMIndexInfo request)
        {
            var pattern = "censor-events-*";

            // 3️⃣ ดึง settings (codec)
            var settingsResponse = await _esClient.GetAsync(
                $"/{pattern}/_settings?filter_path=*.settings.index.codec");

            settingsResponse.EnsureSuccessStatusCode();
            var settingsJson = JsonSerializer.Deserialize<JsonElement>(
                await settingsResponse.Content.ReadAsStringAsync());

            // 4️⃣ ดึง stats (store size)
            var statsResponse = await _esClient.GetAsync(
                $"/{pattern}/_stats/store?filter_path=indices.*.primaries.store.size_in_bytes");

            statsResponse.EnsureSuccessStatusCode();
            var statsJson = JsonSerializer.Deserialize<JsonElement>(
                await statsResponse.Content.ReadAsStringAsync());

            // 1️⃣ เรียก _cat/indices
            var catResponse = await _esClient.GetAsync(
                $"/_cat/indices/{pattern}?format=json&bytes=b" +
                $"&h=index,health,status,docs.count,store.size,pri,rep,creation.date" +
                $"&s=creation.date:desc");

            catResponse.EnsureSuccessStatusCode();

            var catContent = await catResponse.Content.ReadAsStringAsync();
            var catIndices = JsonSerializer.Deserialize<List<JsonElement>>(catContent);

            // 2️⃣ เรียก ILM explain
            var ilmResponse = await _esClient.GetAsync(
                $"/{pattern}/_ilm/explain");

            ilmResponse.EnsureSuccessStatusCode();

            var ilmContent = await ilmResponse.Content.ReadAsStringAsync();
            var ilmJson = JsonSerializer.Deserialize<JsonElement>(ilmContent);

            var ilmIndices = ilmJson.GetProperty("indices");

            // 3️⃣ รวมข้อมูล
            var allIndices = new List<MIndexInfo>();

            foreach (var item in catIndices!)
            {
                var indexName = item.GetProperty("index").GetString()!;

                // ===== Compression Info =====

                string codec = "default";
                string compressionAlgorithm = "LZ4";
                double? estimatedRatio = null;

                // codec
                if (settingsJson.TryGetProperty(indexName, out var settingRoot) &&
                    settingRoot.TryGetProperty("settings", out var settingsNode) &&
                    settingsNode.TryGetProperty("index", out var indexNode) &&
                    indexNode.TryGetProperty("codec", out var codecProp) &&
                    codecProp.ValueKind == JsonValueKind.String)
                {
                    codec = codecProp.GetString()!;
                }

                compressionAlgorithm = codec == "best_compression"
                    ? "DEFLATE"
                    : "LZ4";

                // store size
                long? storeSizeFromStats = null;

                if (statsJson.TryGetProperty("indices", out var indicesNode) &&
                    indicesNode.TryGetProperty(indexName, out var indexStats) &&
                    indexStats.TryGetProperty("primaries", out var primariesNode) &&
                    primariesNode.TryGetProperty("store", out var storeNode) &&
                    storeNode.TryGetProperty("size_in_bytes", out var sizeProp) &&
                    sizeProp.ValueKind == JsonValueKind.Number)
                {
                    storeSizeFromStats = sizeProp.GetInt64();
                }

                if (storeSizeFromStats.HasValue && item.TryGetProperty("docs.count", out var docCountProp) &&
                    docCountProp.ValueKind == JsonValueKind.String &&
                    long.TryParse(docCountProp.GetString(), out var docCount) &&
                    docCount > 0)
                {
                    estimatedRatio = (double) storeSizeFromStats.Value / docCount;
                }

                string ilmPhase = "N/A";
                if (ilmIndices.TryGetProperty(indexName, out var ilmInfo))
                {
                    if (ilmInfo.TryGetProperty("phase", out var phaseProp) &&
                        phaseProp.ValueKind == JsonValueKind.String)
                    {
                        ilmPhase = phaseProp.GetString()!;
                    }
                }

                DateTime? creationDate = null;

                if (item.TryGetProperty("creation.date", out var creationProp))
                {
                    if (creationProp.ValueKind == JsonValueKind.String &&
                        long.TryParse(creationProp.GetString(), out var creationDateMs))
                    {
                        creationDate = DateTimeOffset
                            .FromUnixTimeMilliseconds(creationDateMs)
                            .UtcDateTime;
                    }
                }

                allIndices.Add(new MIndexInfo
                {
                    IndexName = indexName,
                    Health = item.GetProperty("health").GetString()!,
                    Status = item.GetProperty("status").GetString()!,
                    DocCount = long.Parse(item.GetProperty("docs.count").GetString()!),
                    StoreSizeBytes = long.Parse(item.GetProperty("store.size").GetString()!),
                    StoreSizeHuman = item.GetProperty("store.size").GetString()!,
                    IlmPhase = ilmPhase!,
                    CreationDate = creationDate,
                    PrimaryShards = int.Parse(item.GetProperty("pri").GetString()!),
                    Replicas = int.Parse(item.GetProperty("rep").GetString()!),

                    Codec = codec,
                    CompressionAlgorithm = compressionAlgorithm,
                    EstimatedAvgDocSizeBytes = estimatedRatio
                });
            }

            // 4️⃣ paging (in-memory เพราะมี ~1000 index เท่านั้น)
            var total = allIndices.Count;

            var paged = allIndices
                .OrderByDescending(x => x.CreationDate)
                .Skip((request.Offset - 1) * request.Limit)
                .Take(request.Limit)
                .ToList();

            return Ok(new
            {
                offset = request.Offset,
                limit = request.Limit,
                total,
                data = paged
            });
        }
    }
}
