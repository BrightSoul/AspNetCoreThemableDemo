using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace ThemableDemo.Models.Services
{
    public class ThemableViewFileProvider : IFileProvider, IThemer, IDisposable
    {
        private readonly PhysicalFileProvider fileProvider;
        private const string ViewRoot = "/Pages"; // Relative to the contentRoot
        private const string ThemeRoot = "/Themes"; // Relative to the contentRoot
        private readonly string contentRoot;
        private string currentThemeName = string.Empty;

        public ThemableViewFileProvider(string contentRoot)
        {
            this.contentRoot = contentRoot; // Physical absolute path to app directory (e.g. C:\app\)
            this.fileProvider = new PhysicalFileProvider(contentRoot);
        }

        #region IFileProvider implementation
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            subpath = GetThemePath(subpath, currentThemeName);
            return fileProvider.GetDirectoryContents(subpath);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            string themedSubpath = GetThemePath(subpath, currentThemeName);
            IFileInfo themedInfo = fileProvider.GetFileInfo(themedSubpath);
            if (themedInfo.Exists)
            {
                return themedInfo;
            }

            return fileProvider.GetFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            filter = GetThemePath(filter, currentThemeName);
            return fileProvider.Watch(filter);
        }
        #endregion

        #region IThemer implementation
        public string CurrentThemeName => currentThemeName;

        public void ChangeTheme(string themeName)
        {
            lock(fileProvider) { // Make this thread-safe since it's registered as a singleton
                if (string.IsNullOrEmpty(themeName))
                {
                    throw new ArgumentException("Theme name cannot be empty");
                }

                if (themeName == currentThemeName)
                {
                    // Theme is not changing, just return
                    return;
                }

                // More sanitization on the theme name (e.g. it must exist on disk)
                if (!GetThemeNames().Contains(themeName))
                {
                    throw new ArgumentException($"Theme {themeName} does not exist in the {ThemeRoot} directory");
                }

                currentThemeName = themeName;
            }
        }

        public IReadOnlyCollection<string> GetThemeNames()
        {
            string themePhysicalRoot = Path.Combine(contentRoot, ThemeRoot.TrimStart('/'));
            return Directory.EnumerateDirectories(themePhysicalRoot).Select(path => Path.GetFileName(path)).ToArray();
        }
        #endregion

        #region IDisposable implementation
        public void Dispose()
        {
            fileProvider.Dispose();
        }
        #endregion

        private static string GetThemePath(string path, string themeName)
        {
            if (string.IsNullOrEmpty(themeName))
            {
                return path;
            }

            if (!path.StartsWith(ViewRoot))
            {
                return path;
            }

            string themeViewRoot = $"{ThemeRoot}/{themeName}";
            return themeViewRoot + path.Substring(ViewRoot.Length);
        }
    }
}