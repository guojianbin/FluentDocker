﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using Ductus.FluentDocker.Model.Common;

namespace Ductus.FluentDocker.Extensions
{
  public static class FileExtensions
  {
    public static void ToFile(this string contents, TemplateString fqPath)
    {
      var folder = Path.GetDirectoryName(fqPath);
      if (null != folder && !Directory.Exists(folder))
      {
        Directory.CreateDirectory(folder);
      }

      File.WriteAllText(fqPath, contents);
    }

    public static string FromFile(this TemplateString fqPath, Encoding encoding = null)
    {
      if (null == encoding)
      {
        encoding = Encoding.UTF8;
      }

      return File.ReadAllText(fqPath, encoding);
    }

    /// <summary>
    ///   Copies file or directories (recursively) to the <paramref name="workdir" /> and returns a relative
    ///   linux compatible¨path string to be used in e.g. a Dockerfile.
    /// </summary>
    /// <param name="fileOrDirectory">The file or directory to copy to <paramref name="workdir" />.</param>
    /// <param name="workdir">The working directory to copy the file or directory to.</param>
    /// <returns>A relative path to <paramref name="workdir" /> in linux format. If fails it will return null.</returns>
    /// <remarks>
    ///   If the <paramref name="fileOrDirectory" /> is on format emb://namespace/file format it will use
    ///   <see
    ///     cref="ResourceExtensions.ToFile(System.Collections.Generic.IEnumerable{Ductus.FluentDocker.Resources.ResourceInfo},TemplateString)" />
    ///   to perform the copy. Only one file is permitted and thus the file or directory parameter is always a single file.
    /// </remarks>
    public static string Copy(this TemplateString fileOrDirectory, TemplateString workdir)
    {
      if (fileOrDirectory.Rendered.StartsWith($"{EmbeddedUri.Prefix}:"))
      {
        return new EmbeddedUri(fileOrDirectory.Rendered).ToFile(workdir);
      }

      if (File.Exists(fileOrDirectory))
      {
        var file = Path.GetFileName(fileOrDirectory);
        File.Copy(fileOrDirectory, Path.Combine(workdir, file));
        return file;
      }

      if (!Directory.Exists(fileOrDirectory))
      {
        return null;
      }

      CopyTo(fileOrDirectory, workdir);

      // Return the relative path of workdir
      return Path.GetFileName(Path.GetFullPath(fileOrDirectory).TrimEnd(Path.DirectorySeparatorChar));
    }

    public static void CopyTo(this TemplateString sourceDirectory, TemplateString targetDirectory)
    {
      CopyAll(new DirectoryInfo(sourceDirectory), new DirectoryInfo(targetDirectory));
    }

    private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
    {
      Directory.CreateDirectory(target.FullName);

      foreach (var fi in source.GetFiles())
      {
        fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
      }

      foreach (var diSourceSubDir in source.GetDirectories())
      {
        var nextTargetSubDir =
          target.CreateSubdirectory(diSourceSubDir.Name);
        CopyAll(diSourceSubDir, nextTargetSubDir);
      }
    }
  }
}