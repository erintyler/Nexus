namespace Nexus.Application.Common.Services;

public interface IStorageService
{
    /// <summary>
    /// Gets a stream for downloading an object from S3
    /// </summary>
    /// <param name="bucketName">The name of the S3 bucket</param>
    /// <param name="key">The object key/path in the bucket</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A stream containing the object data</returns>
    Task<Stream?> GetObjectStreamAsync(string bucketName, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a presigned URL for uploading an object to S3
    /// </summary>
    /// <param name="bucketName">The name of the S3 bucket</param>
    /// <param name="key">The object key/path in the bucket</param>
    /// <param name="contentType">The content type of the object to be uploaded</param>
    /// <param name="expirationMinutes">How long the URL should remain valid (in minutes)</param>
    /// <returns>A presigned URL that can be used to upload an object</returns>
    string GeneratePresignedUploadUrl(string bucketName, string key, string contentType, int expirationMinutes = 60);

    /// <summary>
    /// Saves an object to the specified S3 bucket
    /// </summary>
    /// <param name="bucketName">The name of the S3 bucket</param>
    /// <param name="key">The object key/path in the bucket</param>
    /// <param name="data">The stream containing the object data to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SaveObjectAsync(string bucketName, string key, byte[] data, CancellationToken cancellationToken = default);
}

