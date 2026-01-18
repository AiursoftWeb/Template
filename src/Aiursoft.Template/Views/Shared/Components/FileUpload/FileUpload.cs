using Aiursoft.Template.Services.FileStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Aiursoft.Template.Views.Shared.Components.FileUpload;

public class FileUpload(StorageService storage) : ViewComponent
{
    public IViewComponentResult Invoke(
        ModelExpression aspFor,
        string? subfolder = null,
        string? uploadEndpoint = null,
        int maxSizeInMb = 2000,
        string? allowedExtensions = null,
        bool isVault = false)
    {
        if (string.IsNullOrWhiteSpace(uploadEndpoint) && !string.IsNullOrWhiteSpace(subfolder))
        {
            uploadEndpoint = storage.GetUploadUrl(subfolder, isVault);
        }

        if (string.IsNullOrWhiteSpace(uploadEndpoint))
        {
            throw new ArgumentException("Either uploadEndpoint or subfolder must be provided.");
        }

        return View(new FileUploadViewModel
        {
            AspFor = aspFor,
            UploadEndpoint = uploadEndpoint,
            MaxSizeInMb = maxSizeInMb,
            AllowedExtensions = allowedExtensions,
            IsVault = isVault
        });
    }
}
