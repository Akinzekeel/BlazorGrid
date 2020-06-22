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

        private Expression<Func<T>> _For;
        private Func<T> _ForCompiled;

        [Parameter]
        public Expression<Func<T>> For
        {
            get => _For; set
            {
                _For = value;
                _ForCompiled = _For?.Compile();
            }
        }
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> Attributes { get; set; }

        Expression IGridCol.For => For;
        private bool IsRegistered;

        private T GetAutoValue()
        {
            return _ForCompiled == null ? default : _ForCompiled.Invoke();
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
                    "sortable",
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

        private bool IsSorted => false; // Parent?.OrderByPropertyName == ForExpression.ToString();

        public bool IsFilterable => true;
        private bool IsFiltered => false;// Parent?.IsFilteredBy(ForExpression) == true;

        protected override void OnParametersSet()
        {
            if (!IsRegistered && Parent != null)
            {
                IsRegistered = true;
                Parent.Add(this);
            }
        }

        private string GetCaptionOrDefault()
        {
            if (string.IsNullOrEmpty(Caption))
            {
                // Attempt to get the DisplayName from the property
                return DisplayNameHelper.GetDisplayName(For);
            }

            return Caption;
        }
    }
}