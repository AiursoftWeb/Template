using System.Security.Cryptography;
using System.Text;
using Aiursoft.Scanner.Abstractions;

namespace Aiursoft.Template.Services.FileStorage;

/// <summary>
/// Represents a service for storing and managing files. (Level 3: Business Gateway)
/// </summary>
public class StorageService(
    FeatureFoldersProvider folders,
    FileLockProvider fileLockProvider,
    IConfiguration configuration) : ITransientDependency
{
    /// <summary>
    /// Saves a file to the storage.
    /// </summary>
    /// <param name="logicalPath">The logical path (relative to Workspace) where the file will be saved.</param>
    /// <param name="file">The file to be saved.</param>
    /// <param name="isVault">Whether to save to the private Vault.</param>
    /// <returns>The actual logical path where the file is saved (may differ if renamed).</returns>
    public async Task<string> Save(string logicalPath, IFormFile file, bool isVault = false)
    {
        // 1. Get Workspace root
        var root = isVault ? folders.GetVaultFolder() : folders.GetWorkspaceFolder();
        
        // 2. Resolve physical path
        var physicalPath = Path.GetFullPath(Path.Combine(root, logicalPath));

        // 3. Security check: Ensure path is within Workspace
        if (!physicalPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Path traversal attempt detected!");
        }

        // 4. Create directory if needed
        var directory = Path.GetDirectoryName(physicalPath);
        if (!Directory.Exists(directory))
        {
             Directory.CreateDirectory(directory!);
        }

        // 5. Handle collisions (Renaming)
        // Lock on the directory to prevent race conditions during renaming
        var lockObj = fileLockProvider.GetLock(directory!); 
        await lockObj.WaitAsync();
        try
        {
            var expectedFileName = Path.GetFileName(physicalPath);
            while (File.Exists(physicalPath))
            {
                expectedFileName = "_" + expectedFileName;
                physicalPath = Path.Combine(directory!, expectedFileName);
            }

            // Create placeholder to reserve name
            File.Create(physicalPath).Close();
        }
        finally
        {
            lockObj.Release();
        }

        // 6. Write file content
        await using var fileStream = new FileStream(physicalPath, FileMode.Create);
        await file.CopyToAsync(fileStream);
        
        // 7. Return logical path (relative to Workspace)
        return Path.GetRelativePath(root, physicalPath).Replace("\\", "/");
    }

    /// <summary>
    /// Retrieves the physical file path for a given logical path.
    /// Defaults to Workspace.
    /// </summary>
    public string GetFilePhysicalPath(string logicalPath, bool isVault = false)
    {
        var root = isVault ? folders.GetVaultFolder() : folders.GetWorkspaceFolder();
        var physicalPath = Path.GetFullPath(Path.Combine(root, logicalPath));

        if (!physicalPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Restricted path access!");
        }
        return physicalPath;
    }

    public string GetDownloadToken(string path)
    {
        var expiry = DateTime.UtcNow.AddMinutes(60); 
        var expiryTicks = expiry.Ticks;
        var key = configuration["Storage:Key"] ?? throw new InvalidOperationException("Storage:Key is not configured!");
        var signatureInput = $"{path}|{expiryTicks}";
        
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureInput));
        var signature = Convert.ToHexString(signatureBytes);
        
        var token = $"{path}|{expiryTicks}|{signature}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(token)); 
    }

    public bool ValidateDownloadToken(string requestPath, string tokenString)
    {
        try 
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(tokenString));
            var parts = decoded.Split('|');
            
            if (parts.Length < 3) return false;
            
            var signature = parts.Last();
            var expiryTicksStr = parts[parts.Length - 2];
            var authorizedPath = string.Join("|", parts.Take(parts.Length - 2));
            
            if (!long.TryParse(expiryTicksStr, out var expiryTicks)) return false;
            
            // Validate Expiry
            var expiry = new DateTime(expiryTicks, DateTimeKind.Utc);
            if (DateTime.UtcNow > expiry) return false;
            
            // Validate Signature
            var key = configuration["Storage:Key"];
             if (string.IsNullOrEmpty(key)) return false;

            var signatureInput = $"{authorizedPath}|{expiryTicks}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var expectedSignatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureInput));
            var expectedSignature = Convert.ToHexString(expectedSignatureBytes);
            
            if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(signature), 
                Encoding.UTF8.GetBytes(expectedSignature)))
            {
                return false;
            }
            
            return requestPath.StartsWith(authorizedPath, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Converts a logical path to a URI-compatible path.
    /// </summary>
    private string RelativePathToUriPath(string relativePath)
    {
        var urlPath = Uri.EscapeDataString(relativePath)
            .Replace("%5C", "/")
            .Replace("%5c", "/")
            .Replace("%2F", "/")
            .Replace("%2f", "/")
            .TrimStart('/');
        return urlPath;
    }

    public string RelativePathToInternetUrl(string relativePath, HttpContext context, bool isVault = false)
    {
        if (isVault)
        {
            var token = GetDownloadToken(relativePath);
            return $"{context.Request.Scheme}://{context.Request.Host}/download-private/{RelativePathToUriPath(relativePath)}?token={token}";
        }
        return $"{context.Request.Scheme}://{context.Request.Host}/download/{RelativePathToUriPath(relativePath)}";
    }

    public string RelativePathToInternetUrl(string relativePath, bool isVault = false)
    {
        if (isVault)
        {
            var token = GetDownloadToken(relativePath);
            return $"/download-private/{RelativePathToUriPath(relativePath)}?token={token}";
        }
        return $"/download/{RelativePathToUriPath(relativePath)}";
    }
}
