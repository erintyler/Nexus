using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Nexus.Application.Common.Services;

namespace Nexus.Infrastructure.Services;

public class S3StorageService(IAmazonS3 s3Client, ILogger<S3StorageService> logger) : IStorageService
{
    public async Task<Stream?> GetObjectStreamAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = key
        };

        try
        {
            var response = await s3Client.GetObjectAsync(request, cancellationToken);

            return response.HttpStatusCode is not HttpStatusCode.OK ? null : response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogWarning("Object not found in S3 bucket. Bucket: {BucketName}, Key: {Key}", bucketName, key);
            return null;
        }
    }

    public string GeneratePresignedUploadUrl(string bucketName, string key, string contentType, int expirationMinutes = 60)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            ContentType = contentType
        };

        return s3Client.GetPreSignedURL(request);
    }

    public Task SaveObjectAsync(string bucketName, string key, byte[] data, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            InputStream = new MemoryStream(data)
        };

        return s3Client.PutObjectAsync(request, cancellationToken);
    }
}

