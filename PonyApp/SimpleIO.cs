using System;
using System.IO;
using static System.IO.Path;

namespace SimpleMaid
{
  public static class SimpleIO
  {
    public static class Path
    {
      /// <summary>
      /// Doesn't complain if paths are null; platform-independent.
      /// </summary>
      public static bool Equals(string pathA, string pathB)
      {
        if (pathA == pathB)
          return true;
        else if (String.IsNullOrWhiteSpace(pathA) || String.IsNullOrWhiteSpace(pathB))
          return false;

        pathA = pathA.Replace("\"", "");
        pathB = pathB.Replace("\"", "");

        char[] dirSeparators = { DirectorySeparatorChar, AltDirectorySeparatorChar };

        return (SimplePlatform.Platform.Unix == SimplePlatform.runningPlatform()) ?
          String.Equals(GetFullPath(pathA).TrimEnd(dirSeparators), GetFullPath(pathB).TrimEnd(dirSeparators)) :
          String.Equals(GetFullPath(pathA).TrimEnd(dirSeparators), GetFullPath(pathB).TrimEnd(dirSeparators), StringComparison.OrdinalIgnoreCase);
      }
    }

    public static class Directory
    {
      /// <summary>
      /// Doesn't complain if source directory isn't present or is the same as target,
      /// overwrites the existing files (standart exception is thrown if they're in use).
      /// </summary>
      /// <param name="sourceDir">source directory.</param>
      /// <param name="destDir">target directory.</param>
      /// <param name="copyRootFiles">false to exclude files in source directory from being copied; otherwise, true.</param>
      public static void Copy(DirectoryInfo sourceDir, DirectoryInfo destDir, bool copyRootFiles = true)
      {
        if (!sourceDir.Exists || Path.Equals(sourceDir.FullName, destDir.FullName))
          return;
        destDir.Create();

        if (copyRootFiles)
        {
          var filesInSource = sourceDir.GetFiles();
          foreach (var file in filesInSource)
          {
            file.CopyTo(Combine(destDir.FullName, file.Name), true);
          }
        }

        var dirsInSource = sourceDir.GetDirectories();
        foreach (var dir in dirsInSource)
        {
          Copy(dir, new DirectoryInfo(Combine(destDir.FullName, dir.Name)));
        }
      }

      /// <summary>
      /// Doesn't complain if source directory isn't present or is the same as target,
      /// overwrites the existing files (standart exception is thrown if they're in use).
      /// </summary>
      /// <param name="sourceDirPath">source directory full name.</param>
      /// <param name="destDirPath">target directory full name.</param>
      /// <param name="copyRootFiles">false to exclude files in source directory from being copied; otherwise, true.</param>
      public static void Copy(string sourceDirPath, string destDirPath, bool copyRootFiles = true)
      {
        Copy(new DirectoryInfo(sourceDirPath), new DirectoryInfo(destDirPath), copyRootFiles);
      }
    }


    #region Extension methods

    /// <summary>
    /// Doesn't complain if paths are null; platform-independent.
    /// </summary>
    public static bool IsEqualTo(this DirectoryInfo dir, string pathToCompare)
    {
      if (null == dir)
        throw new ArgumentNullException(nameof(dir));

      return Path.Equals(dir.FullName, pathToCompare);
    }

    /// <summary>
    /// Doesn't complain if paths are null; platform-independent.
    /// </summary>
    public static bool IsEqualTo(this FileInfo file, string pathToCompare)
    {
      if (null == file)
        throw new ArgumentNullException(nameof(file));

      return Path.Equals(file.FullName, pathToCompare);
    }

    /// <summary>
    /// Doesn't complain if source directory isn't present or is the same as target,
    /// overwrites the existing files (standart exception is thrown if they're in use).
    /// </summary>
    /// <param name="dir">source directory.</param>
    /// <param name="destDir">target directory.</param>
    /// <param name="copyRootFiles">false to exclude files in source directory from being copied; otherwise, true.</param>
    public static void CopyTo(this DirectoryInfo dir, DirectoryInfo destDir, bool copyRootFiles = true)
    {
      if (null == dir)
        throw new ArgumentNullException(nameof(dir));

      Directory.Copy(dir, destDir, copyRootFiles);
    }

    /// <summary>
    /// Doesn't complain if source directory isn't present or is the same as target,
    /// overwrites the existing files (standart exception is thrown if they're in use).
    /// </summary>
    /// <param name="dir">source directory.</param>
    /// <param name="destDirPath">target directory full name.</param>
    /// <param name="copyRootFiles">false to exclude files in source directory from being copied; otherwise, true.</param>
    public static void CopyTo(this DirectoryInfo dir, string destDirPath, bool copyRootFiles = true)
    {
      if (null == dir)
        throw new ArgumentNullException(nameof(dir));

      Directory.Copy(dir, new DirectoryInfo(destDirPath), copyRootFiles);
    }

    #endregion
  }
}
