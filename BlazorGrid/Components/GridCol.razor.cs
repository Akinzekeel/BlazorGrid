using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using System;
using BlazorGrid.Interfaces;
using BlazorGrid.Abstractions.Helpers;

namespace BlazorGrid.Components
{
    public partial class GridCol : IGridCol
    {
        [CascadingParameter] internal IBlazorGrid Parent { get; set; }

        [Parameter] public string Caption { get; set; }
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public bool FitToContent { get; set; }
        [Parameter] public bool AlignRight { get; set; }
        [Parameter] public string OrderBy { get; set; }

        private bool IsRegistered;
        public string CssClass
        {
            get
            {
                var cls = new string[]{
                    AlignRight ? "text-right" : "",
                    IsSortable ? "sortable" : "",
                    IsSorted ? "sorted" : ""
                };

                return string.Join(' ', cls).Trim();
            }
        }

        private bool IsSortable => !string.IsNullOrEmpty(OrderBy);
        private bool IsSorted => IsSortable && Parent?.OrderByPropertyName == OrderBy;

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