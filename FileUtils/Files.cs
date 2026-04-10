namespace FileUtils
{
    public static partial class FileUtils
    {
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

        public static void RenameFile(string filePath, string newName)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");
            
            string? directory = Path.GetDirectoryName(filePath);

            // Should never be null since filePath is valid. Just to supress warnnings. 
            if (directory == null)
                throw new DirectoryNotFoundException($"Directory not found for file: {filePath}");

            string newFilePath = Path.Combine(directory, newName);

            if (File.Exists(newFilePath))
                throw new IOException($"A file with the name '{newName}' already exists in the directory '{directory}'.");

            File.Move(filePath, newFilePath);
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
