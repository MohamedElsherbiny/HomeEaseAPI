//using Azure.Storage.Blobs;
//using HomeEase.Domain.Entities;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.IO;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

//namespace HomeEase.API.Controllers;

//[ApiController]
//[Route("api/provider-images")]
//[Authorize]
//public class ProviderImageController : ControllerBase
//{
//    private readonly DbContext _context;
//    private readonly string _connectionString;
//    private readonly string _containerName;

//    public ProviderImageController(DbContext context, IConfiguration configuration)
//    {
//        _context = context ?? throw new ArgumentNullException(nameof(context));
//        _connectionString = configuration["BlobStorage:ConnectionString"]
//            ?? throw new InvalidOperationException("Blob Storage connection string is missing or empty.");
//        _containerName = "provider-images";
//    }

//    [HttpPost("upload")]
//    public async Task<IActionResult> UploadProviderImage([FromForm] IFormFile file, [FromForm] Guid providerId, [FromForm] int sortOrder)
//    {
//        if (file == null || file.Length == 0)
//            return BadRequest("No file provided.");

//        if (file.Length > 10 * 1024 * 1024)
//            return BadRequest("File size exceeds the limit (10MB).");

//        var provider = await _context.Set<Provider>().FindAsync(providerId);
//        if (provider == null)
//            return NotFound("Provider not found.");

//        try
//        {
//            var blobServiceClient = new BlobServiceClient(_connectionString);
//            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
//            await containerClient.CreateIfNotExistsAsync();

//            var safeFileName = SanitizeFileName(file.FileName);
//            var blobPath = $"provider/{providerId}/{Guid.NewGuid()}/{safeFileName}";
//            var blobClient = containerClient.GetBlobClient(blobPath);

//            await using var fileStream = file.OpenReadStream();
//            fileStream.Position = 0;
//            await blobClient.UploadAsync(fileStream, overwrite: true);

//            var providerImage = new ProviderImage
//            {
//                Id = Guid.NewGuid(),
//                ProviderId = providerId,
//                ImageUrl = blobClient.Uri.ToString(),
//                SortOrder = sortOrder,
//                CreatedAt = DateTime.UtcNow
//            };

//            _context.Set<ProviderImage>().Add(providerImage);
//            await _context.SaveChangesAsync();

//            return Ok(new { providerImage.Id, providerImage.ImageUrl, providerImage.SortOrder });
//        }
//        catch (Azure.RequestFailedException ex)
//        {
//            return StatusCode(500, $"Blob Storage error: {ex.Message}");
//        }
//        catch (Exception ex)
//        {
//            return StatusCode(500, $"An error occurred: {ex.Message}");
//        }
//    }

//    [HttpGet("provider/{providerId}")]
//    public async Task<IActionResult> GetProviderImages(Guid providerId)
//    {
//        var provider = await _context.Set<Provider>().AnyAsync(p => p.Id == providerId);
//        if (!provider)
//            return NotFound("Provider not found.");

//        var images = await _context.Set<ProviderImage>()
//            .Where(pi => pi.ProviderId == providerId)
//            .OrderBy(pi => pi.SortOrder)
//            .Select(pi => new { pi.Id, pi.ImageUrl, pi.SortOrder, pi.CreatedAt, pi.UpdatedAt })
//            .ToListAsync();

//        return Ok(images);
//    }

//    [HttpGet("{id}")]
//    public async Task<IActionResult> GetProviderImage(Guid id)
//    {
//        var image = await _context.Set<ProviderImage>()
//            .Select(pi => new { pi.Id, pi.ProviderId, pi.ImageUrl, pi.SortOrder, pi.CreatedAt, pi.UpdatedAt })
//            .FirstOrDefaultAsync(pi => pi.Id == id);

//        if (image == null)
//            return NotFound("Image not found.");

//        return Ok(image);
//    }

//    [HttpPut("{id}")]
//    public async Task<IActionResult> UpdateProviderImage(Guid id, [FromForm] IFormFile? file, [FromForm] int sortOrder)
//    {
//        var providerImage = await _context.Set<ProviderImage>().FindAsync(id);
//        if (providerImage == null)
//            return NotFound("Image not found.");

//        try
//        {
//            if (file != null && file.Length > 0)
//            {
//                if (file.Length > 10 * 1024 * 1024)
//                    return BadRequest("File size exceeds the limit (10MB).");

//                var blobServiceClient = new BlobServiceClient(_connectionString);
//                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
//                await containerClient.CreateIfNotExistsAsync();

//                var safeFileName = SanitizeFileName(file.FileName);
//                var blobPath = $"provider/{providerImage.ProviderId}/{Guid.NewGuid()}/{safeFileName}";
//                var blobClient = containerClient.GetBlobClient(blobPath);

//                await using var fileStream = file.OpenReadStream();
//                fileStream.Position = 0;
//                await blobClient.UploadAsync(fileStream, overwrite: true);

//                providerImage.ImageUrl = blobClient.Uri.ToString();
//            }

//            providerImage.SortOrder = sortOrder;
//            providerImage.UpdatedAt = DateTime.UtcNow;

//            _context.Set<ProviderImage>().Update(providerImage);
//            await _context.SaveChangesAsync();

//            return Ok(new { providerImage.Id, providerImage.ImageUrl, providerImage.SortOrder });
//        }
//        catch (Azure.RequestFailedException ex)
//        {
//            return StatusCode(500, $"Blob Storage error: {ex.Message}");
//        }
//        catch (Exception ex)
//        {
//            return StatusCode(500, $"An error occurred: {ex.Message}");
//        }
//    }

//    [HttpDelete("{id}")]
//    public async Task<IActionResult> DeleteProviderImage(Guid id)
//    {
//        var providerImage = await _context.Set<ProviderImage>().FindAsync(id);
//        if (providerImage == null)
//            return NotFound("Image not found.");

//        try
//        {
//            var blobServiceClient = new BlobServiceClient(_connectionString);
//            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
//            var blobClient = containerClient.GetBlobClient(GetBlobPathFromUrl(providerImage.ImageUrl));
//            await blobClient.DeleteIfExistsAsync();

//            _context.Set<ProviderImage>().Remove(providerImage);
//            await _context.SaveChangesAsync();

//            return NoContent();
//        }
//        catch (Azure.RequestFailedException ex)
//        {
//            return StatusCode(500, $"Blob Storage error: {ex.Message}");
//        }
//        catch (Exception ex)
//        {
//            return StatusCode(500, $"An error occurred: {ex.Message}");
//        }
//    }

//    private string SanitizeFileName(string fileName)
//    {
//        var extension = Path.GetExtension(fileName)?.ToLower() ?? "";
//        var baseName = Path.GetFileNameWithoutExtension(fileName);
//        baseName = Regex.Replace(baseName, @"[^a-zA-Z0-9_\-]", "");
//        if (string.IsNullOrWhiteSpace(baseName))
//            baseName = "image";

//        return $"{baseName}_{Guid.NewGuid()}{extension}";
//    }

//    private string GetBlobPathFromUrl(string imageUrl)
//    {
//        var uri = new Uri(imageUrl);
//        var path = uri.AbsolutePath.TrimStart('/');
//        return path.StartsWith(_containerName) ? path.Substring(_containerName.Length + 1) : path;
//    }
//}