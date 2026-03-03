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
        [HttpPost]
        [Route("org/{id}/action/GetIndices")]
        public async Task<IActionResult> GetIndices(string id, [FromBody] VMIndexInfo request)
        {
            var pattern = "censor-events-*";

            // 1️⃣ เรียก _cat/indices
            var catResponse = await _esClient.GetAsync(
                $"/_cat/indices/{pattern}?format=json&bytes=b");

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

                var ilmPhase = ilmIndices.TryGetProperty(indexName, out var ilmInfo)
                    ? ilmInfo.GetProperty("phase").GetString()
                    : "N/A";

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
                    Replicas = int.Parse(item.GetProperty("rep").GetString()!)
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
