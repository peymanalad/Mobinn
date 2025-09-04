using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Abp.Dependency;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace Chamran.Deed.Storage
{
    public class MinioBinaryObjectManager : IBinaryObjectManager, ITransientDependency
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly Lazy<Task> _ensureBucket;
        private readonly ILogger<MinioBinaryObjectManager>? _logger;

        public MinioBinaryObjectManager(IConfiguration configuration, ILogger<MinioBinaryObjectManager>? logger = null)
        {
            _logger = logger;
            var endpoint = configuration["Minio:Endpoint"] ?? "localhost:9000";
            var accessKey = configuration["Minio:AccessKey"] ?? "minioadmin";
            var secretKey = configuration["Minio:SecretKey"] ?? "minioadmin";
            var useSsl = configuration["Minio:UseSSL"] == "true";
            _bucketName = configuration["Minio:BucketName"] ?? "binaryobjects";

            var config = new AmazonS3Config
            {
                ServiceURL = (useSsl ? "https://" : "http://") + endpoint,
                ForcePathStyle = true,
                UseHttp = !useSsl
            };

            System.Net.ServicePointManager.Expect100Continue = false;
            _s3Client = new AmazonS3Client(accessKey, secretKey, config);
            _ensureBucket = new Lazy<Task>(EnsureBucketAsync);
        }

        private async Task EnsureBucketAsync()
        {
            var exists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName).ConfigureAwait(false);
            if (!exists)
            {
                await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = _bucketName }).ConfigureAwait(false);
            }
        }

        public async Task<BinaryObject> GetOrNullAsync(Guid id)
        {
            await _ensureBucket.Value.ConfigureAwait(false);
            var baseKey = id.ToString("N");
            var candidates = new[]
            {
                baseKey,
                baseKey + ".jpg",
                baseKey + ".png",
                baseKey + ".jpeg",
                baseKey + ".gif",
                baseKey + ".pdf",
                baseKey + ".mp4"
            };

            foreach (var key in candidates)
            {
                try
                {
                    using var response = await _s3Client.GetObjectAsync(_bucketName, key).ConfigureAwait(false);
                    using var ms = new MemoryStream();
                    await response.ResponseStream.CopyToAsync(ms).ConfigureAwait(false);
                    return new BinaryObject { Id = id, Bytes = ms.ToArray(), Description = Path.GetFileName(key) };
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    continue;
                }
            }

            return null;
        }

        public async Task SaveAsync(BinaryObject file)
        {
            await _ensureBucket.Value.ConfigureAwait(false);

            var ext = Path.GetExtension(file.Description ?? string.Empty) ?? string.Empty;
            var key = file.Id.ToString("N") + ext.ToLowerInvariant();

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(file.Description ?? string.Empty, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            using var ms = new MemoryStream(file.Bytes);
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = ms,
                AutoCloseStream = false,
                AutoResetStreamPosition = false,
                ContentType = contentType
            };

            await _s3Client.PutObjectAsync(request).ConfigureAwait(false);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _ensureBucket.Value.ConfigureAwait(false);

            var prefix = id.ToString("N");
            var list = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = prefix
            }).ConfigureAwait(false);

            if (!list.S3Objects.Any())
            {
                return;
            }

            var request = new DeleteObjectsRequest
            {
                BucketName = _bucketName,
                Objects = list.S3Objects.Select(o => new KeyVersion { Key = o.Key }).ToList()
            };

            try
            {
                await _s3Client.DeleteObjectsAsync(request).ConfigureAwait(false);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
            }
        }
    }
}