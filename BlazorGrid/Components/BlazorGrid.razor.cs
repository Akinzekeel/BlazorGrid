using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using BlazorGrid.Interfaces;
using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Helpers;

namespace BlazorGrid.Components
{
    public partial class BlazorGrid<TRow> : IBlazorGrid where TRow : class
    {
        [Inject] public IGridProvider Provider { get; set; }
        [Inject] public NavigationManager Nav { get; set; }
        private bool IsLoadingMore { get; set; }
        public const int DefaultPageSize = 25;

        [Parameter] public string SourceUrl { get; set; }
        [Parameter] public RenderFragment<TRow> ChildContent { get; set; }
        [Parameter] public int PageSize { get; set; } = DefaultPageSize;

        private string _Query;
        [Parameter]
        public string Query
        {
            get => _Query;
            set
            {

                if (_Query == value)
                    return;

                _Query = value;

                InvokeAsync(async () =>
                {
                    await Task.Delay(300);

                    if (_Query == value)
                    {
                        QueryDebounced = value;
                        await LoadAsync(true);
                    }
                });

            }
        }

        [Parameter] public EventCallback<TRow> OnClick { get; set; }
        [Parameter] public Func<TRow, string> Href { get; set; }
        [Parameter] public Expression<Func<TRow, object>> DefaultOrderBy { get; set; }
        [Parameter] public bool DefaultOrderByDescending { get; set; }
        [Parameter] public List<TRow> Rows { get; set; }

        private string QueryDebounced { get; set; }
        public string OrderByPropertyName { get; private set; }
        public bool OrderByDescending { get; private set; }
        private int TotalCount { get; set; }
        private IList<IGridCol> Columns { get; set; } = new List<IGridCol>();
        private Exception LoadingError { get; set; }

        public event EventHandler<int> OnAfterRowClicked;

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                if (DefaultOrderBy != null)
                {
                    OrderByPropertyName = ExpressionHelper.GetPropertyName(DefaultOrderBy);
                    OrderByDescending = DefaultOrderByDescending;
                }

                if (Rows == null)
                {
                    InvokeAsync(() => LoadAsync(true));
                }
            }
        }

        private async Task LoadAsync(bool Initialize)
        {
            if (IsLoadingMore)
            {
                return;
            }

            if (Initialize)
            {
                Rows = null;
            }

            LoadingError = null;
            IsLoadingMore = true;
            StateHasChanged();

            try
            {
                var result = await Provider.GetAsync<TRow>(
                    SourceUrl,
                    Rows?.Count ?? 0,
                    PageSize,
                    OrderByPropertyName,
                    OrderByDescending,
                    QueryDebounced
                );

                TotalCount = result.TotalCount;

                if (Initialize)
                {
                    Rows = result.Data.ToList();
                }
                else
                {
                    Rows.AddRange(result.Data);
                }
            }
            catch (Exception x)
            {
                LoadingError = x;
            }
            finally
            {
                IsLoadingMore = false;
                StateHasChanged();
            }
        }

        public Task TryApplySorting(string PropertyName)
        {
            if (string.IsNullOrEmpty(PropertyName))
                return Task.CompletedTask;

            if (OrderByPropertyName == PropertyName)
            {
                OrderByDescending = !OrderByDescending;
            }
            else
            {
                OrderByPropertyName = PropertyName;
                OrderByDescending = false;
            }

            return LoadAsync(true);
        }

        private string GridColumns
        {
            get
            {
                var sizes = Columns.Select(col => col.FitToContent ? "max-content" : "auto");
                return string.Join(' ', sizes);
            }
        }

        public Task LoadMoreAsync()
        {
            if (IsLoadingMore)
            {
                return Task.CompletedTask;
            }

            return InvokeAsync(() => LoadAsync(false));
        }

        public int LastClickedRowIndex { get; private set; } = -1;
        private void OnRowClicked(TRow r, int index)
        {
            if (r == null)
            {
                return;
            }

            LastClickedRowIndex = index;
            var onClickUrl = Href?.Invoke(r);

            if (onClickUrl != null)
            {
                Nav.NavigateTo(onClickUrl);
            }
            else if (OnClick.HasDelegate)
            {
                OnClick.InvokeAsync(r);
            }

            OnAfterRowClicked?.Invoke(this, index);
        }

        public void Add(IGridCol col)
        {
            Columns.Add(col);
            StateHasChanged();
        }
    }
}