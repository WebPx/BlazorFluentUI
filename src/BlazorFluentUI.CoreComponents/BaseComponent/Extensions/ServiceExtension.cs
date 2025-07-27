using System;
using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorFluentUI
{
    public static class ServiceExtension
    {
        public static void AddBlazorFluentUI(this IServiceCollection services, Action<FluentUISettings>? configure = default)
        {
            services.AddScoped<ObjectIDGenerator>();
            services.AddScoped<IComponentStyle, ComponentStyle>();
            services.AddScoped<ThemeProvider>();
            services.AddScoped<ScopedStatics>();
            services.AddScoped<LayerHostService>();
            var setup = services.AddOptions<FluentUISettings>();
            setup.Configure(options =>
            {
                options.UseFluentUISystemIcons = null;
            });
            if (configure != null)
                setup.Configure(configure);
        }
    }
}