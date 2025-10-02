using Microsoft.AspNetCore.Components;

namespace BlazorFluentUI
{
    public partial class Icon : FluentUIComponentBase
    {

        [Parameter] public bool Filled { get; set; }
        [Parameter] public string? IconName { get; set; }
        [Parameter] public int IconSize { get; set; } = 20;
        [Parameter] public string? IconSrc { get; set; }
        [Parameter] public IconType IconType { get; set; }
        [Parameter] public bool? UseFluentUISystemIcons { get; set; }

        private bool IsSystemIcons => UseFluentUISystemIcons ?? Settings?.UseFluentUISystemIcons ?? false;

        public string IconClassName
        {
            get
            {
                if (string.IsNullOrEmpty(IconName))
                {
                    return "ms-Icon-placeHolder";
                }
                else
                {
                    if (!IsSystemIcons)
                    {
                        return $"ms-Icon--{IconName}";
                    }
                    else
                    {
                        return $"icon-ic_fluent_{IconName}_{IconSize}_{(Filled ? "filled" : "regular")}";
                    }
                }
            }
        }

        public string FontStyle
        {
            get
            {
                return IsSystemIcons ? $@"font-family: FluentSystemIcons-{(Filled ? "Filled" : "Regular")} !important; {this.Style}" : this.Style!;
            }
        }

        public string SpecificIconName
        {
            get
            {
                if (IconName == null)
                {
                    return "ms-Icon-placeHolder";
                }
                else
                {
                    if (!IsSystemIcons)
                    {
                        return IconName;
                    }
                    else
                    {
                        return $"ic_fluent_{IconName}_{IconSize}_{(Filled ? "filled" : "regular")}";
                    }
                }
            }
        }
    }
}
