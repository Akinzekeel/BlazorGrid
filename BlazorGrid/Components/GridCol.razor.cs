using BlazorGrid.Helpers;
using BlazorGrid.Interfaces;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BlazorGrid.Components
{
    public partial class GridCol<T> : IGridCol<T>, IDisposable
    {
        [CascadingParameter] internal IColumnRegister Register { get; set; }
        [Parameter] public string Caption { get; set; }
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public bool FitToContent { get; set; }
        [Parameter] public bool AlignRight { get; set; }

        private IColumnRegister RegisterCache;
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
                        PropertyNameCached = RegisterCache.GetPropertyName(For);
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

        public bool IsFilterable => true;

        protected override void OnParametersSet()
        {
            if (Register != null)
            {
                Register.Register(this);
                RegisterCache = Register;
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

        public void Dispose()
        {
            RegisterCache = null;
        }
    }
}