using Amazon.S3;
using Amazon.S3.Model;
using ChatBackend.Models.DTOs.User;

namespace ChatBackend.Services
{
    public class FileService : IFileService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileService> _logger;
        
        public FileService(IAmazonS3 s3Client, IConfiguration configuration, ILogger<FileService> logger)
        {
            _s3Client = s3Client;
            _configuration = configuration;
            _logger = logger;
        }
        
        public async Task<FileUploadDto> UploadFileAsync(IFormFile file, string bucketName = "chat-files")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty", nameof(file));
                
            // Generate unique file key
            var fileExtension = Path.GetExtension(file.FileName);
            var fileKey = $"{Guid.NewGuid()}{fileExtension}";
            
            try
            {
                using var stream = file.OpenReadStream();
                
                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileKey,
                    InputStream = stream,
                    ContentType = file.ContentType,
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                };
                
                await _s3Client.PutObjectAsync(request);
                
                var fileUrl = $"https://{bucketName}.s3.amazonaws.com/{fileKey}";
                
                _logger.LogInformation("File uploaded successfully: {FileKey}", fileKey);
                
                return new FileUploadDto
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    FileUrl = fileUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
                throw;
            }
        }
        
        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                // Extract bucket and key from URL
                var uri = new Uri(fileUrl);
                var bucketName = uri.Host.Split('.')[0];
                var fileKey = uri.AbsolutePath.TrimStart('/');
                
                var request = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileKey
                };
                
                await _s3Client.DeleteObjectAsync(request);
                
                _logger.LogInformation("File deleted successfully: {FileUrl}", fileUrl);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileUrl}", fileUrl);
                return false;
            }
        }
        
        public async Task<string> GetPresignedUrlAsync(string fileKey, TimeSpan expiration)
        {
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _configuration["AWS:S3:BucketName"],
                    Key = fileKey,
                    Verb = HttpVerb.GET,
                    Expires = DateTime.UtcNow.Add(expiration)
                };
                
                return await _s3Client.GetPreSignedURLAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating presigned URL for file: {FileKey}", fileKey);
                throw;
            }
        }
    }
}
