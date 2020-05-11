using BlazorGrid.Abstractions;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using System;
using BlazorGrid.Interfaces;

namespace BlazorGrid.Components
{
    public partial class GridCol : IGridCol
    {
        [CascadingParameter] internal IBlazorGrid Parent { get; set; }

        [Parameter] public string Caption { get; set; }
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public bool FitToContent { get; set; }
        [Parameter] public bool AlignRight { get; set; }

        [Parameter]
        public Expression<Func<object>> Sortable
        {
            get => _Sortable; set
            {
                _Sortable = value;
                _SortablePropertyName = _Sortable == null
                    ? null
                    : Helpers.ExpressionHelper.GetPropertyName(_Sortable);
            }
        }

        private string _SortablePropertyName;
        private Expression<Func<object>> _Sortable;

        public string SortablePropertyName => _SortablePropertyName;

        private bool IsRegistered;

        public string CssClass => AlignRight ? "text-right" : "";

        protected override void OnParametersSet()
        {
            if (!IsRegistered && Parent != null)
            {
                Parent.Add(this);
                IsRegistered = true;
            }
        }
    }
}