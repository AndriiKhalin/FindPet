using Microsoft.Extensions.Configuration;

namespace FindPet_API.Helpers.UrlHelper;

public static class PathNormalizer
{
    /// <summary>
    /// Normalizes file paths by converting backslashes to forward slashes
    /// and removing duplicate slashes
    /// </summary>
    public static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        // Replace backslashes with forward slashes
        string normalizedPath = path.Replace('\\', '/');

        // Remove duplicate slashes
        while (normalizedPath.Contains("//"))
        {
            normalizedPath = normalizedPath.Replace("//", "/");
        }

        return normalizedPath;
    }
}