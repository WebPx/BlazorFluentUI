using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorFluentUI
{
    public class FluentUIComponentBase : ComponentBase, IAsyncDisposable
    {
        [CascadingParameter(Name = "Theme")]
        public ITheme? Theme { get; set; }

        //[Inject] private IComponentContext ComponentContext { get; set; }
        [Inject] protected IJSRuntime? JSRuntime { get; set; }
        [Inject] private ThemeProvider ThemeProvider { get; set; } = new ThemeProvider();
        [Inject] private IOptions<FluentUISettings>? _settingsOptions { get; set; }

        private FluentUISettings? _settings;

        protected FluentUISettings Settings => _settings ??= _settingsOptions?.Value ?? BlazorFluentUI.FluentUISettings.Default;

        [Parameter] public string? ClassName { get; set; }
        [Parameter] public string? Style { get; set; }

        //ARIA Properties
        [Parameter] public string? AriaAtomic { get; set; }
        [Parameter] public string? AriaBusy { get; set; }
        [Parameter] public string? AriaControls { get; set; }
        [Parameter] public string? AriaCurrent { get; set; }
        [Parameter] public string? AriaDescribedBy { get; set; }
        [Parameter] public string? AriaDetails { get; set; }
        [Parameter] public bool AriaDisabled { get; set; }
        [Parameter] public string? AriaDropEffect { get; set; }
        [Parameter] public string? AriaErrorMessage { get; set; }
        [Parameter] public string? AriaFlowTo { get; set; }
        [Parameter] public string? AriaGrabbed { get; set; }
        [Parameter] public string? AriaHasPopup { get; set; }
        [Parameter] public string? AriaHidden { get; set; }
        [Parameter] public string? AriaInvalid { get; set; }
        [Parameter] public string? AriaKeyShortcuts { get; set; }
        [Parameter] public string? AriaLabel { get; set; }
        [Parameter] public string? AriaLabelledBy { get; set; }
        [Parameter] public AriaLive AriaLive { get; set; } = AriaLive.Polite;
        [Parameter] public string? AriaOwns { get; set; }
        [Parameter] public bool AriaReadonly { get; set; }  //not universal
        [Parameter] public string? AriaRelevant { get; set; }
        [Parameter] public string? AriaRoleDescription { get; set; }

        public ElementReference RootElementReference;

        /// <summary>
        /// Gets or sets a collection of additional attributes that will be applied to the created element.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object>? AdditionalAttributes { get; set; }

        //private ITheme _theme;
        //private bool reloadStyle;

        [Inject] ScopedStatics? ScopedStatics { get; set; }

        internal const string BaseComponentJs = "baseComponent.js";
        internal const string BaseComponentPath = "_content/BlazorFluentUI.CoreComponents/";
        internal const string DefaultBasePath = "./";

        private static string? _appBasePath = null;
        private static string? _baseComponentPath = null;

        protected string AppBasePath => _appBasePath ??= (Settings.AssetsPath ?? ((Settings.BasePath ?? DefaultBasePath) + BaseComponentPath));

        protected string BuildScriptPath(ref string? scriptPath, string scriptName) => scriptPath ??= AppBasePath + scriptName;

        protected string BasePath => _baseComponentPath ??= BuildScriptPath(ref _baseComponentPath, BaseComponentJs);
        protected IJSObjectReference? baseModule;

        protected CancellationTokenSource cancellationTokenSource = new();

        protected override async Task OnInitializedAsync()
        {
            ThemeProvider.ThemeChanged += OnThemeChangedPrivate;
            ThemeProvider.ThemeChanged += OnThemeChangedProtected;
            //cancellationTokenSource = new CancellationTokenSource();
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {

            try
            {
                if (baseModule == null)
                    baseModule = await JSRuntime!.InvokeAsync<IJSObjectReference>("import", BasePath);

                if (cancellationTokenSource.Token.IsCancellationRequested)
                    throw new TaskCanceledException();

                if (!ScopedStatics!.FocusRectsInitialized)
                {
                    ScopedStatics.FocusRectsInitialized = true;
                    await baseModule!.InvokeVoidAsync("initializeFocusRects", cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException cancelled)
            {
                Debug.WriteLine($"Task cancelled: {cancelled.Message}");
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        public async Task<Rectangle> GetBoundsAsync()
        {
            CancellationToken token = cancellationTokenSource.Token;
            return await GetBoundsAsync(token);

        }

        public async Task<Rectangle> GetBoundsAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (baseModule == null)
                    baseModule = await JSRuntime!.InvokeAsync<IJSObjectReference>("import", cancellationToken, BasePath);

                Rectangle? rectangle = await baseModule!.InvokeAsync<Rectangle>("measureElementRect", cancellationToken, RootElementReference);
                return rectangle;
            }
            catch (JSException)
            {
                return new Rectangle();
            }
        }

        public async Task<Rectangle> GetBoundsAsync(ElementReference elementReference)
        {
            CancellationToken token = cancellationTokenSource.Token;
            return await GetBoundsAsync(elementReference, token);
        }

        public async Task<Rectangle> GetBoundsAsync(ElementReference elementReference, CancellationToken cancellationToken)
        {
            try
            {
                if (baseModule == null)
                    baseModule = await JSRuntime!.InvokeAsync<IJSObjectReference>("import", cancellationToken, BasePath);
                Rectangle? rectangle = await baseModule!.InvokeAsync<Rectangle>("measureElementRect", cancellationToken, elementReference);
                return rectangle;
            }
            catch (JSException)
            {
                return new Rectangle();
            }
        }

        private void OnThemeChangedProtected(object? sender, ThemeChangedArgs themeChangedArgs)
        {
            Theme = themeChangedArgs.Theme;
            OnThemeChanged();
        }

        protected virtual void OnThemeChanged() { }

        private void OnThemeChangedPrivate(object? sender, ThemeChangedArgs themeChangedArgs)
        {
            //reloadStyle = true;
        }

        public virtual async ValueTask DisposeAsync()
        {
            try
            {
                cancellationTokenSource.Cancel();
                if (baseModule != null && !cancellationTokenSource.IsCancellationRequested)
                {
                    await baseModule.DisposeAsync();
                    baseModule = null;
                }
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}
