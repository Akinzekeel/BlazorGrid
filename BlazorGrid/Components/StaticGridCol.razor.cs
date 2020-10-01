using BlazorGrid.Interfaces;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BlazorGrid.Components
{
    public partial class StaticGridCol : IGridCol
    {
        [CascadingParameter] internal IBlazorGrid Parent { get; set; }
        [Parameter] public string Caption { get; set; }
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public bool FitToContent { get; set; }
        [Parameter] public bool AlignRight { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> Attributes { get; set; }

        Expression IGridCol.For => null;
        string IGridCol.PropertyName => null;

        private bool IsRegistered;

        protected override bool ShouldRender()
        {
            return !IsRegistered;
        }

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

        public bool IsFilterable => false;

        protected override void OnParametersSet()
        {
            if (!IsRegistered && Parent != null)
            {
                IsRegistered = true;
                Parent.Add(this);
            }
        }

        public string GetCaptionOrDefault() => Caption;

        public string SortIconCssClass() => null;

        public void Unlink()
        {
            IsRegistered = false;
        }
    }
}