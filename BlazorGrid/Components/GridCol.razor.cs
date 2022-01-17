using BlazorGrid.Helpers;
using BlazorGrid.Interfaces;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq.Expressions;

namespace BlazorGrid.Components
{
    public partial class GridCol<T> : ColBase, IGridCol<T>, IDisposable
    {
        private IColumnRegister? RegisterCache;
        private bool IsPropertyNameQueried;
        private string? PropertyNameCached;

        public override string? PropertyName
        {
            get
            {
                if (!IsPropertyNameQueried)
                {
                    IsPropertyNameQueried = true;

                    if (For != null)
                    {
                        PropertyNameCached = RegisterCache?.GetPropertyName(For);
                    }
                }

                return PropertyNameCached;
            }
        }

        private Expression<Func<T>>? _For;
        private Func<T>? _ForCompiled;

        [Parameter]
        public Expression<Func<T>>? For
        {
            get => _For; set
            {
                _For = value;
                _ForCompiled = _For?.Compile();

                IsPropertyNameQueried = false;
                PropertyNameCached = null;
            }
        }

        Expression? IGridCol.For => For;

        private T? GetAutoValue()
        {
            return _For == null ? default : (_ForCompiled ??= _For.Compile()).Invoke();
        }

        public override bool IsFilterable => true;

        protected override void OnParametersSet()
        {
            if (Register != null)
            {
                Register.Register(this);
                RegisterCache = Register;
            }
        }

        public override string GetCaptionOrDefault()
        {
            if (string.IsNullOrEmpty(Caption) && For != null)
            {
                // Attempt to get the DisplayName from the property
                return DisplayNameHelper.GetDisplayName(For);
            }

            return Caption ?? string.Empty;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            RegisterCache = null;
        }
    }
}