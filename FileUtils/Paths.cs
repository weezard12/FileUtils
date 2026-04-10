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
            StringComparison comparison = GetPathComparison();

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

        /// <summary>
        /// Returns the appropriate <see cref="StringComparison"/> strategy for comparing file system paths
        /// on the current operating system.
        /// </summary>
        /// <returns>
        /// <see cref="StringComparison.OrdinalIgnoreCase"/> on case-insensitive file systems
        /// (Windows, macOS, Mac Catalyst, iOS, tvOS, and watchOS);
        /// otherwise, <see cref="StringComparison.Ordinal"/> for case-sensitive file systems (e.g. Linux).
        /// </returns>
        public static StringComparison GetPathComparison()
        {
            StringComparison comparison = (OperatingSystem.IsWindows()
                                        || OperatingSystem.IsMacOS()
                                        || OperatingSystem.IsMacCatalyst()
                                        || OperatingSystem.IsIOS()
                                        || OperatingSystem.IsTvOS()
                                        || OperatingSystem.IsWatchOS())
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
            return comparison;
        }

        public static StringComparison GetPathComparison(string path)
        {
            if (!Directory.Exists(path))
                return GetPathComparison();

            try
            {
                // Resolve to a full path and walk up until we find an existing directory to probe
                string fullPath = Path.GetFullPath(path);

                if (!path.EndsWith(Path.DirectorySeparatorChar) && !path.EndsWith(Path.AltDirectorySeparatorChar))
                    path += Path.DirectorySeparatorChar;


                char[] pathChars = fullPath.ToCharArray();

                pathChars[0] = char.ToLower(pathChars[0]);
                fullPath = new string(pathChars);

                pathChars[0] = char.ToUpper(pathChars[0]);



                bool caseSensitive = Directory.Exists(fullPath);

                // A case-insensitive file system will resolve the toggled name to the same entry
                return caseSensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            }
            catch
            {
                // I/O errors, permission denied, etc. — degrade gracefully
                return GetPathComparison();
            }
        }

        public static bool IsPathCaseSensitive(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"The directory '{path}' does not exist.");

            // Resolve to a full path and walk up until we find an existing directory to probe
            string fullPath = Path.GetFullPath(path);

            string newPath = fullPath;

            char[] pathChars = fullPath.ToCharArray();
            for (int i = pathChars.Length - 1; i >= 0; i--)
            {
                if (char.IsLetter(pathChars[i]))
                {
                    // Toggle the case of the first letter we find from the end
                    pathChars[i] = char.IsUpper(pathChars[i]) ? char.ToLower(pathChars[i]) : char.ToUpper(pathChars[i]);
                    newPath = new string(pathChars);

                    // If swapping a letter to upper/lower case results in a different path that does not exists, then the file system is case-insensitive
                    if (!Directory.Exists(newPath))
                        return true;

                    // If the two paths have different creation times, then they are different entries and the file system is case-sensitive
                    if (!Directory.GetCreationTime(newPath).Equals(Directory.GetCreationTime(fullPath)))
                        return true;
                    
                    return false;
                }
            }

            return false;
        }

        public static void TrimDirectorySeparator(ref string path)
        {
            path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        public static string TrimDirectorySeparator(string path)
        {
            return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
