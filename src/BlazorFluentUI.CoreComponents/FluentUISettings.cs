namespace BlazorFluentUI
{
    public sealed record FluentUISettings
    {
        public static FluentUISettings Default { get; } = new FluentUISettings();

        public bool? UseFluentUISystemIcons { get; set; }

        public string? BasePath { get; set; }

        public string? AssetsPath { get; set; }
    }
}