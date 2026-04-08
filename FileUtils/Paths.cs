using System;
using System.Collections.Generic;
using System.Text;

namespace FileUtils
{
    public static partial class FileUtils
    {
        /// <summary>
        /// This method checks if the path that the ai provided is valid. Or if he is just adding a suggestion.
        /// </summary>
        /// <returns></returns>
        public static bool IsValidPath(string path)
        {
            char[] invalidChars = new char[] { '\n', '\r', '*', '<', '>', ':', '"', '|', '?' };
            foreach (char c in invalidChars)
                if (path.Contains(c))
                    return false;

            return true;
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
