using System;
using System.Collections.Generic;
using System.Text;

namespace FileUtils
{
    public static partial class FileUtils
    {

        /// <summary>
        /// Ensures a directory exists at the specified path, optionally clearing its contents.
        /// If the path points to a file, the method targets the file's parent directory instead.
        /// </summary>
        /// <param name="path">The full path of the directory (or file) to create.</param>
        /// <param name="clearDirectory">
        /// If true, deletes and recreates the directory if it already exists, effectively emptying it.
        /// Defaults to false.
        /// </param>
        /// <param name="safeDeleteLimit">
        /// A safety threshold in bytes. If the directory's total size exceeds this value,
        /// the operation is aborted to prevent accidental deletion of large directories.
        /// Set to 0 (default) to disable the safety check.
        /// </param>
        public static void CreateFolderIfDoesntExist(string path, bool clearDirectory = false, int safeDeleteLimit = 0)
        {
            // Just in case - dont delete large directories
            if (safeDeleteLimit > 0)
                if (IsDirectorySizeBiggerThan(path, safeDeleteLimit))
                {
                    Console.WriteLine("Folder size is too large to delete for safety reasons");
                    return;
                }

            try
            {
                //if the path is to a file it will call the method again but with the file folder as path
                if (File.Exists(path))
                {
                    string? directoryPath = Path.GetDirectoryName(path);
                    if(string.IsNullOrEmpty(directoryPath))
                        return; // Invalid path, cannot determine directory

                    CreateFolderIfDoesntExist(directoryPath, clearDirectory);
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
        /// Quickly calculates folder size with early exit if it exceeds maxSize.
        /// </summary>
        /// <param name="path">The full path of the directory to evaluate.</param>
        /// <param name="maxSize">The size threshold in bytes. Returns true as soon as the cumulative file size exceeds this value.</param>
        /// <returns>True if the total size of all files in the directory exceeds <paramref name="maxSize"/> bytes; otherwise, false.</returns>
        public static bool IsDirectorySizeBiggerThan(string path, long maxSize)
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
    }
}
