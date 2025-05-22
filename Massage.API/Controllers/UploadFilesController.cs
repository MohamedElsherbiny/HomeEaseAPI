using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Massage.API.Controllers;

[ApiController]
[Route("api/files")]
[Authorize]
public class FileController : ControllerBase
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public FileController(IConfiguration configuration)
    {
        _connectionString = configuration["BlobStorage:ConnectionString"];
        _containerName = "general-files"; // Hardcoded, or use configuration if needed

        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Blob Storage connection string is missing or empty.");
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file provided.");
        }

        // Optional: Check file size (e.g., max 10MB)
        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest("File size exceeds the limit (10MB).");
        }

        try
        {
            // Initialize BlobContainerClient
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            // Sanitize and generate unique file name
            var safeFileName = SanitizeFileName(file.FileName);
            var blobPath = $"files/{Guid.NewGuid()}/{safeFileName}";

            // Upload file
            var blobClient = containerClient.GetBlobClient(blobPath);
            await using var fileStream = file.OpenReadStream();
            fileStream.Position = 0;
            await blobClient.UploadAsync(fileStream, overwrite: true);

            // Return the file URL
            var fileUrl = blobClient.Uri.ToString();
            return Ok(new { FileUrl = fileUrl });
        }
        catch (Azure.RequestFailedException ex)
        {
            return StatusCode(500, $"Blob Storage error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    private string SanitizeFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLower() ?? "";
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        baseName = Regex.Replace(baseName, @"[^a-zA-Z0-9_\-]", "");
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "file";
        }

        return $"{baseName}_{Guid.NewGuid()}{extension}";
    }
}