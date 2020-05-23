using Microsoft.AspNetCore.Components;
using BlazorGrid.Interfaces;
using System.Collections.Generic;
using System.Linq;

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
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> Attributes { get; set; }

        private bool IsRegistered;

        private IDictionary<string, object> FinalAttributes
        {
            get
            {
                var attr = new Dictionary<string, object>
                {
                    { "class", CssClass }
                };

                if (Attributes != null)
                {
                    foreach (var a in Attributes)
                    {
                        if (a.Key != "class")
                        {
                            attr.Add(a.Key, a.Value);
                        }
                    }
                }

                return attr;
            }
        }

        public string CssClass
        {
            get
            {
                var cls = new List<string>{
                    AlignRight ? "text-right" : "",
                    IsSortable ? "sortable" : "",
                    IsSorted ? "sorted" : ""
                };

                if (Attributes != null)
                {
                    string customClasses = Attributes
                        .Where(x => x.Key == "class")
                        .Select(x => x.Value?.ToString())
                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(customClasses))
                    {
                        // Merge custom classes
                        cls.AddRange(customClasses.Split(' '));
                    }
                }

                return string.Join(' ', cls).Trim();
            }
        }

        private bool IsSortable => !string.IsNullOrEmpty(OrderBy);
        private bool IsSorted => IsSortable && Parent?.OrderByPropertyName == OrderBy;

        protected override void OnParametersSet()
        {
            if (!IsRegistered && Parent != null)
            {
                IsRegistered = true;
                Parent.Add(this);
            }
        }
    }
}