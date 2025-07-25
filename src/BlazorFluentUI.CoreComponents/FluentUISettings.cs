using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorFluentUI
{
    public sealed record FluentUISettings
    {
        public static FluentUISettings Default { get; } = new FluentUISettings();

        public bool UseFluentUISystemIcons { get; set; } = true;

        public string? BasePath { get; set; }

        public string? AssetsPath { get; set; }
    }
}