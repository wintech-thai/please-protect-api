using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Its.PleaseProtect.Api.Services
{
    public class MinioObjectStorageService : IObjectStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _defaultBucket;
        private readonly bool _withTLS;

        public MinioObjectStorageService()
        {
            var endpoint = Environment.GetEnvironmentVariable("MINIO_ENDPOINT")
                ?? throw new InvalidOperationException("MINIO_ENDPOINT is not set");

            var user = Environment.GetEnvironmentVariable("MINIO_USER")
                ?? throw new InvalidOperationException("MINIO_USER is not set");

            var password = Environment.GetEnvironmentVariable("MINIO_PASSWORD")
                ?? throw new InvalidOperationException("MINIO_PASSWORD is not set");

            _defaultBucket = Environment.GetEnvironmentVariable("MINIO_BUCKET")
                ?? throw new InvalidOperationException("MINIO_BUCKET is not set");

            _withTLS = Environment.GetEnvironmentVariable("MINIO_TLS") == "true";


            _minioClient = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(user, password)
                .WithSSL(_withTLS)
                .Build();
        }

        public async Task<PresignedPostResult> GetPresignedUrlPost(string bucket, string path, int secDurationExpire)
        {
            var targetBucket = string.IsNullOrEmpty(bucket) ? _defaultBucket : bucket;

            var expireAt = DateTime.UtcNow.AddSeconds(secDurationExpire);

            var policy = new PostPolicy();
            policy.SetBucket(targetBucket);
            policy.SetKey(path);
            policy.SetExpires(expireAt);

            var (url, formData) = await _minioClient.PresignedPostPolicyAsync(policy);

            var result = new PresignedPostResult
                {
                    Url = url.ToString(),
                    Fields = formData,
                    ObjectKey = path,
                    ExpiresAtUtc = expireAt,
                    Provider = "minio"
                };

            return result;
        }

        public async Task<string> GetPresignedUrlGet(string bucket, string path, int secDurationExpire)
        {
            var targetBucket = string.IsNullOrEmpty(bucket) ? _defaultBucket : bucket;

            var result = await _minioClient.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(targetBucket)
                    .WithObject(path)
                    .WithExpiry(secDurationExpire)
            );

            return result;
        }

        public async Task<bool> IsObjectExist(string bucket, string path)
        {
            try
            {
                await _minioClient.StatObjectAsync(
                    new StatObjectArgs()
                        .WithBucket(bucket)
                        .WithObject(path)
                );

                return true; // มีไฟล์อยู่จริง
            }
            catch (MinioException)
            {
                // error อื่น ๆ เช่น permission, network
                return false;
            }
        }
    }
}
