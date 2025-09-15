using Aiursoft.CSTools.Attributes;
using Aiursoft.CSTools.Tools;
using Aiursoft.Template.Services;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;

namespace Aiursoft.Template.Controllers;

/// <summary>
/// This controller is used to handle file operations like upload and download.
/// </summary>
public class FilesController(
    ImageProcessingService imageCompressor,
    ILogger<FilesController> logger,
    StorageService storage) : ControllerBase
{
    [Route("upload/{subfolder}")]
    public async Task<IActionResult> Index([FromRoute] [ValidDomainName] string subfolder)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        // Executing here will let the browser upload the file.
        try
        {
            _ = HttpContext.Request.Form.Files.FirstOrDefault()?.ContentType;
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }

        if (HttpContext.Request.Form.Files.Count < 1)
        {
            return BadRequest("No file uploaded!");
        }

        var file = HttpContext.Request.Form.Files.First();
        if (!new ValidFolderName().IsValid(file.FileName))
        {
            return BadRequest("Invalid file name!");
        }

        var storePath = Path.Combine(
            subfolder,
            DateTime.UtcNow.Year.ToString("D4"),
            DateTime.UtcNow.Month.ToString("D2"),
            DateTime.UtcNow.Day.ToString("D2"),
            file.FileName);
        var relativePath = await storage.Save(storePath, file);
        return Ok(new
        {
            Path = relativePath,
            InternetPath = storage.RelativePathToInternetUrl(relativePath, HttpContext)
        });
    }

    [Route("download/{**folderNames}")]
    public async Task<IActionResult> Download([FromRoute] string folderNames)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var physicalPath = storage.GetFilePhysicalPath(folderNames);
        var workspaceFullPath = Path.GetFullPath(storage.StorageRootFolder);
        if (!physicalPath.StartsWith(workspaceFullPath))
        {
            return BadRequest("Attempted to access a restricted path.");
        }
        if (!System.IO.File.Exists(physicalPath))
        {
            return NotFound();
        }
        if (physicalPath.IsStaticImage() && await IsValidImageAsync(physicalPath))
        {
            return await FileWithImageCompressor(physicalPath);
        }

        return this.WebFile(physicalPath);
    }

    private async Task<bool> IsValidImageAsync(string imagePath)
    {
        try
        {
            _ = await Image.DetectFormatAsync(imagePath);
            logger.LogTrace("File with path {ImagePath} is a valid image", imagePath);
            return true;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "File with path {ImagePath} is not a valid image", imagePath);
            return false;
        }
    }

    private async Task<IActionResult> FileWithImageCompressor(string path)
    {
        var passedWidth = int.TryParse(Request.Query["w"], out var width);
        var passedSquare = bool.TryParse(Request.Query["square"], out var square);
        if (width > 0 && passedWidth)
        {
            width = SizeCalculator.Ceiling(width);
            if (square && passedSquare)
            {
                var compressedPath = await imageCompressor.CompressAsync(path, width, width);
                return this.WebFile(compressedPath);
            }
            else
            {
                var compressedPath = await imageCompressor.CompressAsync(path, width, 0);
                return this.WebFile(compressedPath);
            }
        }

        // If no width or invalid, just clear EXIF
        var clearedPath = await imageCompressor.ClearExifAsync(path);
        return this.WebFile(clearedPath);
    }
}
