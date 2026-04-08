namespace FileUtils
{
    public static class FileUtils
    {
        public static void CreateFolderIfDoesntExist(string path, bool clearDirectory = false, int safeDeleteLimit = 0)
        {
            // Just in case - dont delete large directories
            if (safeDeleteLimit > 0)
                if (IsDirectorySizeBiggerThen(path, safeDeleteLimit))
                {
                    Console.WriteLine("Folder size is too large to delete for safety reasons");
                    return;
                }


            try
            {
                //if the path is to a file it will call the method again but with the file folder as path
                if (File.Exists(path))
                {
                    CreateFolderIfDoesntExist(Path.GetDirectoryName(path), clearDirectory);
                    return;
                }

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    return;
                }
                if (clearDirectory)
                {
                    Directory.Delete(path, true);
                    Directory.CreateDirectory(path);
                }
            }
            catch
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch
                {
                    File.Delete(path);
                    CreateFolderIfDoesntExist(path, clearDirectory);
                }
            }
        }

        /// <summary>
        /// Its like the File.WriteAllText but it will create missing directories.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public static void CreateFileAndWriteAllText(string path, string content = "")
        {
            try
            {
                // If the path contains a directory structure, ensure it exists
                string directoryPath = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    CreateFolderIfDoesntExist(directoryPath);
                }

                // Create the file and close the stream immediately
                File.WriteAllText(path, content);

            }
            catch (Exception)
            {
                throw; // Optionally rethrow the exception if needed
            }
        }


        /// <summary>
        /// Quickly calculates folder size with early exit if it exceeds maxSize.
        /// </summary>
        public static bool IsDirectorySizeBiggerThen(string path, long maxSize)
        {
            long size = 0;
            try
            {
                // Check if directory exists before enumerating
                if (!Directory.Exists(path))
                    return false;

                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        size += new FileInfo(file).Length;
                        if (size > maxSize)
                            return true; // ✅ Early return for speed
                    }
                    catch { /* Skip inaccessible files */ }
                }
            }
            catch (DirectoryNotFoundException)
            {
                // Directory was deleted after existence check or invalid path
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                // No permission to access directory
                return false;
            }
            catch { /* Handle other unexpected errors */ }

            return false;
        }

        /// <summary>
        /// takes a file path, line number (1-based), start index (0-based), length, and a new value, and modifies the specified line in the file by replacing the substring at the given index and length with the new value.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="line"></param>
        /// <param name="startIdx"></param>
        /// <param name="length"></param>
        /// <param name="value"></param>
        public static void ModifyFileLine(string path, int line, int startIdx, int length, string value)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException($"File not found: {path}");

            if (line < 1)
                throw new ArgumentOutOfRangeException(nameof(line), "Line number must be 1-based (greater than 0).");

            if (startIdx < 0)
                throw new ArgumentOutOfRangeException(nameof(startIdx), "Start index must be 0-based (greater than or equal to 0).");

            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than or equal to 0.");

            // Read all lines from the file
            string[] lines = File.ReadAllLines(path);

            // Check if the specified line exists
            if (line > lines.Length)
                throw new ArgumentOutOfRangeException(nameof(line), $"Line {line} does not exist. File has {lines.Length} lines.");

            // Get the target line (convert from 1-based to 0-based indexing)
            string targetLine = lines[line - 1];

            // Validate that the start index and length are within bounds of the line
            if (startIdx > targetLine.Length)
                throw new ArgumentOutOfRangeException(nameof(startIdx), $"Start index {startIdx} is beyond the line length {targetLine.Length}.");

            if (startIdx + length > targetLine.Length)
                throw new ArgumentOutOfRangeException(nameof(length), $"Start index {startIdx} + length {length} exceeds the line length {targetLine.Length}.");

            // Modify the line by replacing the substring
            string modifiedLine = targetLine.Substring(0, startIdx) +
                                    value +
                                    targetLine.Substring(startIdx + length);

            // Update the line in the array
            lines[line - 1] = modifiedLine;

            // Write all lines back to the file
            File.WriteAllLines(path, lines);
        }

        public static void RenameFile(string filePath, string expectedFileName)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File does not exist: " + filePath);
                return;
            }

            string directory = Path.GetDirectoryName(filePath);
            string newFilePath = Path.Combine(directory, expectedFileName);

            if (File.Exists(newFilePath))
            {
                Console.WriteLine("A file with the new name already exists: " + newFilePath);
                return;
            }

            try
            {
                File.Move(filePath, newFilePath);
                Console.WriteLine("File renamed successfully to: " + expectedFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error renaming file: " + ex.Message);
            }
        }

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

        /// <summary>
        /// Copies all files and subdirectories from the source folder to the destination folder.
        /// Creates the destination folder if it doesn't exist.
        /// </summary>
        /// <param name="sourceFolder">The source folder path</param>
        /// <param name="destinationFolder">The destination folder path</param>
        public static void CopyFolderToFolder(string sourceFolder, string destinationFolder)
        {
            if (string.IsNullOrEmpty(sourceFolder))
                throw new ArgumentException("Source folder cannot be null or empty.", nameof(sourceFolder));

            if (string.IsNullOrEmpty(destinationFolder))
                throw new ArgumentException("Destination folder cannot be null or empty.", nameof(destinationFolder));

            if (!Directory.Exists(sourceFolder))
                throw new DirectoryNotFoundException($"Source folder not found: {sourceFolder}");

            // Create destination folder if it doesn't exist
            CreateFolderIfDoesntExist(destinationFolder);

            // Copy all files
            foreach (string file in Directory.GetFiles(sourceFolder))
            {
                try
                {
                    string fileName = Path.GetFileName(file);
                    string destFile = Path.Combine(destinationFolder, fileName);
                    File.Copy(file, destFile, overwrite: true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying file {file}: {ex.Message}");
                }
            }

            // Copy all subdirectories recursively
            foreach (string directory in Directory.GetDirectories(sourceFolder))
            {
                try
                {
                    string dirName = Path.GetFileName(directory);
                    string destDir = Path.Combine(destinationFolder, dirName);
                    CopyFolderToFolder(directory, destDir);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying directory {directory}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Copies a file to the destination path, creating any missing directories in the path.
        /// Unlike File.Copy, this method will not throw DirectoryNotFoundException if intermediate directories don't exist.
        /// </summary>
        /// <param name="sourceFile">The source file path to copy from</param>
        /// <param name="destinationFile">The destination file path to copy to</param>
        /// <param name="overwrite">If true, overwrites the destination file if it exists</param>
        /// <exception cref="ArgumentException">Thrown when source or destination path is null or empty</exception>
        /// <exception cref="FileNotFoundException">Thrown when source file does not exist</exception>
        public static void StrongCopy(string sourceFile, string destinationFile, bool overwrite = false)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(sourceFile))
                throw new ArgumentException("Source file path cannot be null or empty.", nameof(sourceFile));

            if (string.IsNullOrEmpty(destinationFile))
                throw new ArgumentException("Destination file path cannot be null or empty.", nameof(destinationFile));

            if (!File.Exists(sourceFile))
                throw new FileNotFoundException($"Source file not found: {sourceFile}");

            // Create the destination directory structure if it doesn't exist
            string destinationDirectory = Path.GetDirectoryName(destinationFile);
            if (!string.IsNullOrEmpty(destinationDirectory))
            {
                CreateFolderIfDoesntExist(destinationDirectory);
            }

            // Copy the file
            File.Copy(sourceFile, destinationFile, overwrite);
        }
    }
}
