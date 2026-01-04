using System.Net.Http.Headers;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace Its.Otep.Api.Utils
{
    public interface IStorageUtils
    {
        string GenerateUploadUrl(string bucketName, string objectName, TimeSpan validFor, string? contentType = null);
        bool IsObjectExist(string objectName);
        public string GenerateDownloadUrl(string objectName, TimeSpan validFor, string? contentType = null);
        public void UpdateMetaData(string bucketName, string objectName, string metaName, string metaValue);
        public void DeleteObject(string bucketName, string objectName);
        public Task<byte[]> PartialDownloadToStream(string bucketName, string objectName, long start, long end);
        public Google.Apis.Storage.v1.Data.Object? GetStorageObject(string bucketName, string objectName);
    }

    public class StorageUtils : IStorageUtils
    {
        private readonly UrlSigner _urlSigner;
        private readonly StorageClient _storageClient;

        public StorageUtils(GoogleCredential credential, StorageClient storageClient)
        {
            // สร้าง UrlSigner จาก service account credential
            _urlSigner = UrlSigner.FromCredential(
                credential.UnderlyingCredential as ServiceAccountCredential
                ?? throw new InvalidOperationException("Expected service account credential.")
            );

            _storageClient = storageClient;
        }

        public string GenerateUploadUrl(string bucketName, string objectName, TimeSpan validFor, string? contentType = null)
        {
            var options = UrlSigner.Options.FromDuration(validFor);

            var template = UrlSigner.RequestTemplate
                .FromBucket(bucketName)
                .WithObjectName(objectName)
                .WithHttpMethod(HttpMethod.Put);

            if (!string.IsNullOrEmpty(contentType))
            {
                template = template.WithContentHeaders(
                [
                    new KeyValuePair<string, IEnumerable<string>>("Content-Type", [contentType])
                ]);
            }

            template = template.WithRequestHeaders(
            [
                new KeyValuePair<string, IEnumerable<string>>("x-goog-meta-onix-is-temp-file", ["true"])
            ]);

            return _urlSigner.Sign(template, options);
        }

        public void UpdateMetaData(string bucketName, string objectName, string metaName, string metaValue)
        {
            var obj = _storageClient.GetObject(bucketName, objectName);
            obj.Metadata = obj.Metadata ?? new Dictionary<string, string>();
            obj.Metadata[metaName] = metaValue;

            _storageClient.UpdateObject(obj);
        }

        public async Task<byte[]> PartialDownloadToStream(string bucketName, string objectName, long start, long end)
        {
            using var ms = new MemoryStream();
            await _storageClient.DownloadObjectAsync(
                bucket: bucketName,
                objectName: objectName,
                destination: ms,
                options: new DownloadObjectOptions { Range = new RangeHeaderValue(start, end) }
                );

            return ms.ToArray();
        }

        public void DeleteObject(string bucketName, string objectName)
        {
            _storageClient.DeleteObject(bucketName, objectName);
        }

        public Google.Apis.Storage.v1.Data.Object? GetStorageObject(string bucketName, string objectName)
        {
            try
            {
                var obj = _storageClient.GetObject(bucketName, objectName);
                return obj;
            }
            catch (Exception)
            {
                return null; // ไม่พบ object
            }
        }

        public bool IsObjectExist(string objectName)
        {
            var bucketName = Environment.GetEnvironmentVariable("STORAGE_BUCKET")!;

            try
            {
                var obj = _storageClient.GetObject(bucketName, objectName);
                return obj != null;  // ถ้าเจอจะไม่เป็น null
            }
            catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false; // ไม่พบ object
            }
        }

        public string GenerateDownloadUrl(string objectName, TimeSpan validFor, string? contentType = null)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return "";
            }
 
            var bucketName = Environment.GetEnvironmentVariable("STORAGE_BUCKET")!;
            return _urlSigner.Sign(bucketName, objectName, validFor, HttpMethod.Get);
        }
    }
}
