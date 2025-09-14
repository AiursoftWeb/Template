using Aiursoft.CSTools.Attributes;
using Aiursoft.Template.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Template.Controllers;

public class FilesController(
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
    public IActionResult Download([FromRoute] string folderNames)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        if (folderNames.Contains(".."))
        {
            return BadRequest("Invalid path!");
        }

        var physicalPath = storage.GetFilePhysicalPath(folderNames);
        var workspaceFullPath = Path.GetFullPath(storage.WorkspaceFolder);
        if (!physicalPath.StartsWith(workspaceFullPath))
        {
            return BadRequest("Attempted to access a restricted path.");
        }
        if (!System.IO.File.Exists(physicalPath))
        {
            return NotFound();
        }

        return this.WebFile(physicalPath);
    }
}
