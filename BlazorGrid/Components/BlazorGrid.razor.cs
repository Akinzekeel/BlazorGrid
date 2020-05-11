using System.Data;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using BlazorGrid.Interfaces;
using BlazorGrid.Abstractions;
using BlazorGrid.Helpers;

namespace BlazorGrid.Components
{
    public partial class BlazorGrid<TRow> where TRow : IGridRow
    {
        [Inject] public IGridProvider Provider { get; set; }
        [Inject] public NavigationManager Nav { get; set; }
        private bool IsLoadingMore { get; set; }
        public const int DefaultPageSize = 25;

        [Parameter] public string SourceUrl { get; set; }
        [Parameter] public RenderFragment ChildContent { get; set; }
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

        private string QueryDebounced { get; set; }
        private string OrderByPropertyName { get; set; }
        private bool OrderByDescending { get; set; }
        private int TotalCount { get; set; }
        private IList<IGridCol<TRow>> Columns { get; set; } = new List<IGridCol<TRow>>();
        private Exception LoadingError { get; set; }

        private bool ParametersSetCalled;
        protected override async Task OnParametersSetAsync()
        {
            if (!ParametersSetCalled)
            {
                if (DefaultOrderBy != null)
                {
                    OrderByPropertyName = Helpers.ExpressionHelper.GetPropertyName(DefaultOrderBy);
                    OrderByDescending = DefaultOrderByDescending;
                }

                ParametersSetCalled = true;
                await LoadAsync(true);
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

        internal void Add(IGridCol<TRow> Column)
        {
            Columns.Add(Column);
        }

        protected Task TryApplySorting(string PropertyName)
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

        private bool IsRefreshing;
        public async Task RefreshAsync()
        {
            if (IsRefreshing) return;

            // Silently refresh the page that contains the row which
            // had been clicked
            IsRefreshing = true;

            try
            {
                if (LastClickedRowIndex > -1)
                {
                    var row = Rows[LastClickedRowIndex];
                    var result = await Provider.GetAsync<TRow>(SourceUrl, row.RowId);

                    if (result == null)
                    {
                        Rows.Remove(row);
                        TotalCount--;
                    }
                    else
                    {
                        Rows[LastClickedRowIndex] = result;
                    }

                    LastClickedRowIndex = -1;
                }
                else
                {
                    var result = await Provider.GetAsync<TRow>(
                        SourceUrl,
                        0,
                        PageSize,
                        OrderByPropertyName,
                        OrderByDescending,
                        QueryDebounced
                    );

                    var newRow = result.Data.FirstOrDefault();

                    if (newRow != null)
                    {
                        // A row has been added
                        if (!Rows.Any(x => x.RowId == newRow.RowId))
                        {
                            Rows.Insert(0, newRow);
                        }
                        else
                        {
                            // The total count is unchanged but the row might have been
                            // updated. Find & update it in our cache as well
                            var i = Rows.FindIndex(0, Rows.Count, x => x.RowId == newRow.RowId);
                            Rows[i] = newRow;
                        }
                    }
                    else
                    {
                        // Looks like our row has been deleted
                        Rows.RemoveAt(0);
                    }
                }

                await InvokeAsync(StateHasChanged);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private string GridColumns
        {
            get
            {
                var sizes = Columns.Select(col => col.FitToContent ? "max-content" : "auto");
                return string.Join(' ', sizes);
            }
        }

        private List<TRow> Rows { get; set; }

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
        }
    }
}