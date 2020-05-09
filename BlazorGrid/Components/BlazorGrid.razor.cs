using System.Data;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using BlazorGrid.Interfaces;
using BlazorGrid.Abstractions.Interfaces;
using BlazorGrid.Helpers;
using BlazorGrid.Abstractions.Models;

namespace BlazorGrid
{
    public partial class BlazorGrid<TRow> : IBlazorGrid<TRow> where TRow : IGridRow
    {
        [Inject] public IGridProvider Provider { get; set; }
        [Inject] public NavigationManager Nav { get; set; }
        private IBlazorGrid<TRow> Instance => this;
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
        [Parameter] public Func<TRow, string> href { get; set; }
        [Parameter] public Expression<Func<TRow, object>> DefaultOrderBy { get; set; }
        [Parameter] public bool DefaultOrderByDescending { get; set; }

        private string QueryDebounced { get; set; }
        private string PagedSourceUrl() => GetSourceFor(Rows?.Count ?? 0, SourceUrl);

        private string OrderByPropertyName { get; set; }
        private bool OrderByDescending { get; set; }
        private int TotalCount { get; set; }
        private IList<IGridCol<TRow>> Columns { get; set; } = new List<IGridCol<TRow>>();
        private Exception loadingError { get; set; }

        private bool ParametersSetCalled;
        protected override async Task OnParametersSetAsync()
        {
            if (!ParametersSetCalled)
            {
                if (DefaultOrderBy != null)
                {
                    OrderByPropertyName = ExpressionHelper.GetPropertyName(DefaultOrderBy);
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

            IsLoadingMore = true;
            StateHasChanged();

            try
            {
                var result = await Provider.GetAsync<TRow>(PagedSourceUrl());

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
                loadingError = x;
            }
            finally
            {
                IsLoadingMore = false;
                StateHasChanged();
            }
        }

        public void Add(IGridCol<TRow> Column)
        {
            Columns.Add(Column);
        }

        private string GetSourceFor(int Offset, string Source)
        {
            return Provider.GetRequestUrl(Source, Offset, PageSize, OrderByPropertyName, OrderByDescending, QueryDebounced);
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
                    var url = SourceUrl.TrimEnd('/') + '/' + row.RowId + "?More=false";
                    var result = await Provider.GetAsync<TRow>(url);
                    var updatedRow = result.Data.FirstOrDefault();

                    if (updatedRow == null)
                    {
                        Rows.Remove(row);
                    }
                    else
                    {
                        Rows[LastClickedRowIndex] = updatedRow;
                    }

                    LastClickedRowIndex = -1;
                    TotalCount = result.TotalCount;
                }
                else
                {
                    var url = GetSourceFor(0, SourceUrl);
                    var result = await Provider.GetAsync<TRow>(url);
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
            var onClickUrl = href?.Invoke(r);

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