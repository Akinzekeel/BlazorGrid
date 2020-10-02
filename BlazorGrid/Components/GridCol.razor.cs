using BlazorGrid.Helpers;
using BlazorGrid.Interfaces;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BlazorGrid.Components
{
    public partial class GridCol<T> : IGridCol<T>
    {
        [CascadingParameter] internal IBlazorGrid Parent { get; set; }
        [Parameter] public string Caption { get; set; }
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public bool FitToContent { get; set; }
        [Parameter] public bool AlignRight { get; set; }

        private bool IsPropertyNameQueried;
        private string PropertyNameCached;
        public string PropertyName
        {
            get
            {
                if (!IsPropertyNameQueried)
                {
                    IsPropertyNameQueried = true;

                    if (For != null)
                    {
                        PropertyNameCached = Parent.GetPropertyName(For);
                    }
                }

                return PropertyNameCached;
            }
        }

        private Expression<Func<T>> _For;
        private Func<T> _ForCompiled;

        [Parameter]
        public Expression<Func<T>> For
        {
            get => _For; set
            {
                _For = value;
                _ForCompiled = _For?.Compile();

                IsPropertyNameQueried = false;
                PropertyNameCached = null;
            }
        }

        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> Attributes { get; set; }

        Expression IGridCol.For => For;
        private bool IsRegistered;

        private T GetAutoValue()
        {
            return _For == null ? default : (_ForCompiled ??= _For.Compile()).Invoke();
        }

        private IDictionary<string, object> FinalAttributes
        {
            get
            {
                var attr = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(CssClass))
                {
                    attr.Add("class", CssClass);
                }

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
                    AlignRight ? "text-right" : ""
                };

                if (Parent != null)
                {
                    cls.Add("sortable");

                    if (IsSorted)
                    {
                        cls.Add("sorted");
                    }
                }

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

        private bool IsSorted => Parent?.IsSortedBy(this) == true;
        public bool IsFilterable => true;

        protected override void OnParametersSet()
        {
            if (!IsRegistered && Parent != null)
            {
                IsRegistered = true;
                Parent.Add(this);
            }
        }

        public string GetCaptionOrDefault()
        {
            if (string.IsNullOrEmpty(Caption))
            {
                // Attempt to get the DisplayName from the property
                return DisplayNameHelper.GetDisplayName(For);
            }

            return Caption;
        }

        public string SortIconCssClass()
        {
            var cls = "blazor-grid-sort-icon";

            if (IsSorted)
            {
                cls += " active";

                if (Parent.OrderByDescending)
                {
                    cls += " sorted-desc";
                }
                else
                {
                    cls += " sorted-asc";
                }
            }

            return cls;
        }

        protected override bool ShouldRender()
        {
            return !IsRegistered;
        }

        public void Unlink()
        {
            IsRegistered = false;
        }
    }
}