using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace ThemableDemo.Models.Services
{
    public interface IThemer
    {
        string CurrentThemeName { get; }
        void ChangeTheme(string themeName);
        IReadOnlyCollection<string> GetThemeNames();
    }
}