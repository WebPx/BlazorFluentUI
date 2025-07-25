﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace BlazorFluentUI
{
    public partial class SelectionZone<TItem> : FluentUIComponentBase, IAsyncDisposable
    {
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter]
        public bool DisableAutoSelectOnInputElements { get; set; }

        [Parameter]
        public bool DisableRenderOnSelectionChanged { get; set; } = false;

        [Parameter]
        public bool EnableTouchInvocationTarget { get; set; } = false;

        [Parameter]
        public bool EnterModalOnTouch { get; set; }

        [Parameter]
        public bool IsSelectedOnFocus { get; set; } = true;

        [Parameter]
        public Action<TItem, int>? OnItemContextMenu { get; set; }

        [Parameter]
        public Action<TItem, int>? OnItemInvoked { get; set; }

        private Selection<TItem>? _selection;
        [Parameter]
        public Selection<TItem> Selection
        {
            get => _selection!;
            set
            {
                if (_selection != value)
                {
                    if (_selection != null && _selectionSubscription != null)
                    {
                        _selectionSubscription.Dispose();
                    }
                    _selection = value;
                    if (_selection != null)
                    {
                        _selectionSubscription = _selection.SelectionChanged.Subscribe(_ => { });//InvokeAsync(StateHasChanged));
                    }
                }
            }
        }

        IDisposable? _selectionSubscription;
        //[Parameter]
        //public EventCallback<Selection<TItem>> SelectionChanged { get; set; }

        [Parameter]
        public SelectionMode SelectionMode { get; set; }

        [Parameter]
        public bool SelectionPreservedOnEmptyClick { get; set; }

        private IJSObjectReference? scriptModule;
        private string? _scriptPath;

        private string ScriptPath => _scriptPath ??= BuildScriptPath(ref _scriptPath, "selectionZone.js");


        private bool isModal = false;

        private bool doNotRenderOnce = false;

        private DotNetObjectReference<SelectionZone<TItem>>? selfReference;
        private SelectionZoneProps? props;

        protected override bool ShouldRender()
        {
            if (doNotRenderOnce && DisableRenderOnSelectionChanged)
            {
                doNotRenderOnce = false;
                return false;
            }
            else
                doNotRenderOnce = false;

            return true;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (props == null)
            {
                props = GenerateProps();
            }

            if (selfReference != null && scriptModule != null)
            {
                if (isModal != props!.IsModal
                    || SelectionMode != props.SelectionMode
                    || DisableAutoSelectOnInputElements != props.DisableAutoSelectOnInputElements
                    || EnterModalOnTouch != props.EnterModalOnTouch
                    || EnableTouchInvocationTarget != props.EnableTouchInvocationTarget
                    || (OnItemInvoked!= null) != props.OnItemInvokeSet)
                {
                    props = GenerateProps();
                    
                    await scriptModule.InvokeVoidAsync("updateProps", selfReference, props);
                }
            }

            await base.OnParametersSetAsync();
        }

        private SelectionZoneProps GenerateProps()
        {
            return new SelectionZoneProps
            {
                IsModal = isModal,
                SelectionMode = SelectionMode,
                DisableAutoSelectOnInputElements = DisableAutoSelectOnInputElements,
                EnterModalOnTouch = EnterModalOnTouch,
                EnableTouchInvocationTarget=EnableTouchInvocationTarget,
                OnItemInvokeSet = (OnItemInvoked != null)
            };
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            selfReference = DotNetObjectReference.Create(this);
            if (scriptModule == null)
                scriptModule = await JSRuntime!.InvokeAsync<IJSObjectReference>("import", ScriptPath);

            if (firstRender)
            {
                await scriptModule!.InvokeVoidAsync("registerSelectionZone", selfReference, RootElementReference, new SelectionZoneProps { IsModal = isModal, SelectionMode = SelectionMode });
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        public override async ValueTask DisposeAsync()
        {
            try
            {
                if (scriptModule != null)
                {
                    await scriptModule!.InvokeVoidAsync("unregisterSelectionZone", selfReference);
                    await scriptModule.DisposeAsync();
                }
                selfReference?.Dispose();

                await base.DisposeAsync();
            }
            catch (TaskCanceledException)
            {
            }
        }

        [JSInvokable]
        public int GetItemsLength()
        {
            return Selection.GetItems().Count;
        }

        [JSInvokable]
        public bool IsIndexSelected(int index)
        {
            return Selection.IsIndexSelected(index);
        }

        [JSInvokable]
        public int GetSelectedCount()
        {
            int? count = Selection.GetSelectedCount();
            if (count.HasValue)
                return count.Value;
            else
                return 0;
        }

        [JSInvokable]
        public void SetModal(bool isModal)
        {
            Selection.SetModal(isModal);
        }

        [JSInvokable]
        public void SetChangeEvents(bool change)
        {
            Selection.SetChangeEvents(change);
        }

        [JSInvokable]
        public void ToggleAllSelected()
        {
            Selection.ToggleAllSelected();
        }

        [JSInvokable]
        public void ToggleIndexSelected(int index)
        {
            Selection.ToggleIndexSelected(index);
        }

        [JSInvokable]
        public void SetAllSelected(bool isAllSelected)
        {
            Selection.SetAllSelected(isAllSelected);
        }

        [JSInvokable]
        public void SetIndexSelected(int index, bool isSelected, bool shouldAnchor)
        {
            Selection.SetIndexSelected(index, isSelected, shouldAnchor);
        }

        [JSInvokable]
        public void SelectToIndex(int index, bool clearSelection)
        {
            Selection.SelectToIndex(index,clearSelection);
        }

        [JSInvokable]
        public void InvokeItem(int index)
        {
            OnItemInvoked?.Invoke(Selection.GetItems()[index], index);
        }
        
    }
}
