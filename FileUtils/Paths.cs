using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FileUtils
{
    public static partial class FileUtils
    {
        /// <summary>
        /// Determines whether the specified path string is valid.
        /// </summary>
        /// <remarks>
        /// A path is considered invalid if it is:
        /// <list type="bullet">
        ///   <item><description>Null, empty, or whitespace-only</description></item>
        ///   <item><description>Contains null bytes (<c>\0</c>)</description></item>
        ///   <item><description>Contains characters invalid for a path, as defined by <see cref="Path.GetInvalidPathChars"/></description></item>
        /// </list>
        /// Note: This method only validates the format of the path string itself.
        /// It does not check whether the path exists or is accessible on the filesystem.
        /// </remarks>
        /// <param name="path">The path string to validate.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="path"/> is a well-formed path string; otherwise, <c>false</c>.
        /// </returns>
        /// <example>
        /// <code>
        /// IsValidPath("C:\\Users\\file.txt") // true
        /// IsValidPath("")                    // false - empty
        /// IsValidPath("C:\\file\0.txt")      // false - contains null byte
        /// IsValidPath("C:\\file|?.txt")      // false - contains invalid characters
        /// </code>
        /// </example>
        public static bool IsValidPath(string path)
        {
            // Reject null, empty, or whitespace-only paths
            if (string.IsNullOrEmpty(path)) return false;
            if (string.IsNullOrWhiteSpace(path)) return false;

            // Reject null bytes
            if (path.Contains('\0')) return false;

            char[] invalidChars = Path.GetInvalidPathChars();
            foreach (char c in invalidChars)
                if (path.Contains(c))
                    return false;

            return true;
        }


        /// <summary>
        /// Determines whether the specified path is located within the given directory.
        /// </summary>
        /// <param name="path">The file or directory path to check.</param>
        /// <param name="directory">The directory to check against.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="path"/> is located within <paramref name="directory"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Both paths are resolved to their full absolute forms before comparison.
        /// A directory separator is always appended to the resolved directory path to prevent
        /// false positives where one directory name is a prefix of another (e.g. /foo/bar
        /// incorrectly matching /foo/barbaz). String comparison case-sensitivity is determined
        /// by the OS: case-insensitive on Windows and macOS, case-sensitive on Linux.
        /// </remarks>
        public static bool IsPathInDirectory(string path, string directory)
        {
            // Resolve both paths to their canonical absolute forms.
            string fullPath = Path.GetFullPath(path);
            string fullDirectory = Path.GetFullPath(directory);

            // Add directory separator to the end of the directory path if it's not already present
            // Because we are using StartsWith for the check, we need to make sure that we are comparing directories and not just prefixes of directory names
            if (!fullDirectory.EndsWith(Path.DirectorySeparatorChar))
                fullDirectory += Path.DirectorySeparatorChar;

            // Use the string comparison that matches the host file system's own rules:
            //   Windows / macOS  → case-insensitive  (OrdinalIgnoreCase)
            //   Linux            → case-sensitive    (Ordinal)
            StringComparison comparison = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            return fullPath.StartsWith(fullDirectory, comparison);
        }

        /// <summary>
        /// Cross Platform method to check if two paths are equal, accounting for differences in path separators and case sensitivity across operating systems.
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        public static bool ArePathsEqual(string path1, string path2)
        {
            string p1 = Path.GetFullPath(path1)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            string p2 = Path.GetFullPath(path2)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Windows → case-insensitive
            // Linux/macOS → case-sensitive by default
            var comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            return string.Equals(p1, p2, comparison);
        }

    }
}
