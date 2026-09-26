using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using LocalStack.Models.Dto;
using LocalStack.Services.Interfaces;
using LocalStack.Services.Options;

namespace LocalStack.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class FileController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly LocalStackOptions _options;
        private readonly ILogger<FileController> _logger;

        public FileController(
            IStorageService storageService,
            IOptions<LocalStackOptions> options,
            ILogger<FileController> logger)
        {
            _storageService = storageService;
            _options = options.Value;
            _logger = logger;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [ProducesResponseType(typeof(FileUploadResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string? key = null, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file or empty file.");

            if (file.Length > _options.MaxUploadBytes)
                return BadRequest($"File exceeds maximum size of {_options.MaxUploadBytes / (1024 * 1024)} MB.");

            var objectKey = key ?? $"{Guid.NewGuid():N}_{file.FileName}";

            try
            {
                await using var stream = file.OpenReadStream();
                var resultKey = await _storageService.UploadAsync(stream, objectKey, file.ContentType, cancellationToken);
                _logger.LogInformation("File uploaded: {Key}", resultKey);

                return Ok(new FileUploadResultDto
                {
                    Key = resultKey,
                    FileName = file.FileName,
                    Size = file.Length,
                    ContentType = file.ContentType
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("download/{*key}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Download(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var stored = await _storageService.GetAsync(key, cancellationToken);
                if (stored == null)
                    return NotFound($"Object with key '{key}' not found.");

                var fileName = key.Contains('/') ? Path.GetFileName(key) : key;
                return File(stored.Content, stored.ContentType, fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(FileListResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> List(CancellationToken cancellationToken = default)
        {
            var result = await _storageService.ListKeysAsync(cancellationToken);
            return Ok(result);
        }

        [HttpHead("{*key}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Exists(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                FileMetadataDto? metadata = await _storageService.GetMetadataAsync(key, cancellationToken);
                if (metadata == null)
                    return NotFound();

                Response.Headers.ContentLength = metadata.ContentLength;
                Response.Headers.ContentType = metadata.ContentType;
                if (!string.IsNullOrEmpty(metadata.ETag))
                    Response.Headers.ETag = metadata.ETag;

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("metadata/{*key}")]
        [ProducesResponseType(typeof(FileMetadataDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Metadata(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                FileMetadataDto? metadata = await _storageService.GetMetadataAsync(key, cancellationToken);
                if (metadata == null)
                    return NotFound($"Object with key '{key}' not found.");

                return Ok(metadata);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("presign/{*key}")]
        [ProducesResponseType(typeof(PresignedUrlDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Presign(
            string key,
            [FromQuery] string verb = "GET",
            [FromQuery] int expiresInMinutes = 15,
            CancellationToken cancellationToken = default)
        {
            try
            {
                PresignedUrlDto? result = await _storageService.GetPresignedUrlAsync(key, verb, expiresInMinutes, cancellationToken);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{*key}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _storageService.DeleteAsync(key, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Object with key '{key}' not found.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
