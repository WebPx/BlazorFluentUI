﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorFluentUI.Lists
{
    public partial class DetailsHeader<TItem> : FluentUIComponentBase, IAsyncDisposable
    {
        //[CascadingParameter]
        //private SelectionZone<TItem> SelectionZone { get; set; }

        [Parameter]
        public string? AriaLabelForSelectAllCheckbox { get; set; }

        [Parameter]
        public string? AriaLabelForSelectionColumn { get; set; }

        [Parameter]
        public string? AriaLabelForToggleAllGroup { get; set; }

        [Parameter]
        public CheckboxVisibility CheckboxVisibility { get; set; }

        [Parameter]
        public CollapseAllVisibility CollapseAllVisibility { get; set; }

        [Parameter]
        public RenderFragment<object>? ColumnHeaderTooltipTemplate { get; set; }

        [Parameter]
        public object? ColumnReorderOptions { get; set; }

        [Parameter]
        public object? ColumnReorderProps { get; set; }

        [Parameter]
        public IEnumerable<IDetailsRowColumn<TItem>>? Columns { get; set; }

        [Parameter]
        public RenderFragment? DetailsCheckboxTemplate { get; set; }

        [Parameter]
        public int GroupNestingDepth { get; set; }

        [Parameter]
        public bool IsAllCollapsed { get; set; }

        [Parameter]
        public double IndentWidth { get; set; }

        [Parameter]
        public bool IsAllSelected { get; set; }

        [Parameter]
        public DetailsListLayoutMode LayoutMode { get; set; }

        [Parameter]
        public int MinimumPixelsForDrag { get; set; }


        [Parameter]
        public EventCallback<ItemContainer<IDetailsRowColumn<TItem>>> OnColumnAutoResized { get; set; }

        [Parameter]
        public EventCallback<IDetailsRowColumn<TItem>> OnColumnClick { get; set; }

        [Parameter]
        public EventCallback<IDetailsRowColumn<TItem>> OnColumnContextMenu { get; set; }

        [Parameter]
        public EventCallback<object> OnColumnIsSizingChanged { get; set; }

        [Parameter]
        public EventCallback<ColumnResizedArgs<TItem>> OnColumnResized { get; set; }

        [Parameter]
        public EventCallback<bool> OnToggleCollapsedAll { get; set; }

        [Parameter]
        public SelectAllVisibility SelectAllVisibility { get; set; } = SelectAllVisibility.Visible;

        [Parameter]
        public Selection<TItem>? Selection { get; set; }

        [Parameter]
        public SelectionMode SelectionMode { get; set; }

        [Parameter]
        public string? TooltipHostClassName { get; set; }

        [Parameter]
        public bool UseFastIcons { get; set; } = true;

        private string? _scriptPath;

        private string ScriptPath => _scriptPath ??= BuildScriptPath(ref _scriptPath, "detailsList.js");

        private IJSObjectReference? scriptModule;

        private bool showCheckbox;
        private bool isCheckboxHidden;
        private bool isCheckboxAlwaysVisible;
        private int frozenColumnCountFromStart;
        //private int frozenColumnCountFromEnd;

        private string? id;
        //private object? dragDropHelper;
        private (int SourceIndex, int TargetIndex) onDropIndexInfo;
        //private int currentDropHintIndex;
        //private int draggedColumnIndex = -1;

        private bool isResizingColumn;

        const double MIN_COLUMN_WIDTH = 100;

        //state
        //private bool isAllSelected;
        //private bool isAllCollapsed;
        private bool isSizing;
        private int resizeColumnIndex;
        private double resizeColumnMinWidth;
        private double resizeColumnOriginX;

        private DotNetObjectReference<DetailsHeader<TItem>>? selfReference;
        //private ElementReference cellSizer;

        protected override Task OnInitializedAsync()
        {
            id = $"id_{Guid.NewGuid().ToString().Replace("-", "")}";
            onDropIndexInfo = (-1, -1);
            //currentDropHintIndex = -1;

            return base.OnInitializedAsync();
        }

        protected override Task OnParametersSetAsync()
        {
            showCheckbox = SelectAllVisibility != SelectAllVisibility.None;
            isCheckboxHidden = SelectAllVisibility == SelectAllVisibility.Hidden;
            isCheckboxAlwaysVisible = CheckboxVisibility == CheckboxVisibility.Always;

            isResizingColumn = isSizing;

            // TBD
            if (ColumnReorderProps != null && ColumnReorderProps.ToString() == "something")
            {
                frozenColumnCountFromStart = 1234;
            }
            else
            {
                frozenColumnCountFromStart = 0;
            }

            return base.OnParametersSetAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (scriptModule == null)
                scriptModule = await JSRuntime!.InvokeAsync<IJSObjectReference>("import", ScriptPath);

            if (firstRender)
            {
                selfReference = DotNetObjectReference.Create(this);
                await scriptModule!.InvokeVoidAsync("registerDetailsHeader", selfReference, RootElementReference);
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        [JSInvokable]
        public void OnSizerMouseDown(int columnIndex, double originX)
        {
            isSizing = true;
            resizeColumnIndex = columnIndex; //columnIndex - (showCheckbox ? 2 : 1);
            resizeColumnOriginX = originX;
            resizeColumnMinWidth = Columns!.ElementAt(resizeColumnIndex).CalculatedWidth;
            InvokeAsync(StateHasChanged);
        }

        [JSInvokable]
        public void OnDoubleClick(int columnIndex)
        {
            //System.Diagnostics.Debug.WriteLine("DoubleClick happened.");
            OnColumnAutoResized.InvokeAsync(new ItemContainer<IDetailsRowColumn<TItem>> { Item = Columns!.ElementAt(columnIndex), Index = columnIndex });
        }

        private void OnSelectAllClicked(MouseEventArgs mouseEventArgs)
        {
            if (!isCheckboxHidden)
            {
                Selection?.ToggleAllSelected();
            }
        }

        private static void OnToggleCollapseAll(MouseEventArgs mouseEventArgs)
        {

        }

        //private void OnSizerMouseDown(MouseEventArgs args, int colIndex)
        //{
        //    isSizing = true;
        //    resizeColumnIndex = colIndex - (showCheckbox ? 2 : 1);
        //    resizeColumnOriginX = args.ClientX;
        //    resizeColumnMinWidth = Columns.ElementAt(resizeColumnIndex).CalculatedWidth;
        //}

        private void OnSizerMouseMove(MouseEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.ClientX != resizeColumnOriginX)
            {
                //OnColumnIsSizingChanged.InvokeAsync();
            }
            if (OnColumnResized.HasDelegate)
            {
                double movement = mouseEventArgs.ClientX - resizeColumnOriginX;
                //skipping RTL check
                double calculatedWidth = resizeColumnMinWidth + movement;
                double currentColumnMinWidth = Columns!.ElementAt(resizeColumnIndex).MinWidth;
                double constrictedCalculatedWidth = Math.Max((currentColumnMinWidth < 0 || double.IsNaN(currentColumnMinWidth) ? MIN_COLUMN_WIDTH : currentColumnMinWidth), calculatedWidth);
                OnColumnResized.InvokeAsync(new ColumnResizedArgs<TItem>(Columns!.ElementAt(resizeColumnIndex), resizeColumnIndex, constrictedCalculatedWidth));

            }

        }
        private void OnSizerMouseUp(MouseEventArgs mouseEventArgs)
        {
            isSizing = false;
        }

        private static void UpdateDragInfo(int itemIndex)
        {

        }

        public override async ValueTask DisposeAsync()
        {
            try
            {
                if (selfReference != null)
                {
                    await scriptModule!.InvokeVoidAsync("unregisterDetailsHeader", selfReference);
                    selfReference?.Dispose();
                }
                if (scriptModule != null)
                    await scriptModule.DisposeAsync();

                await base.DisposeAsync();
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}
