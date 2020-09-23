using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Abstractions.Helpers;
using BlazorGrid.Interfaces;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BlazorGrid.Components
{
    public partial class BlazorGrid<TRow> : IDisposable, IBlazorGrid where TRow : class
    {
        public const int DefaultPageSize = 25;

        [Inject] public IGridProvider Provider { get; set; }
        [Inject] public IBlazorGridConfig Config { get; set; }
        [Inject] public NavigationManager Nav { get; set; }
        [Parameter] public string SourceUrl { get; set; }
        [Parameter] public RenderFragment<TRow> ChildContent { get; set; }
        [Parameter] public int PageSize { get; set; } = DefaultPageSize;
        [Parameter] public TRow EmptyRow { get; set; }

        private bool IsLoadingMore { get; set; }

        private string _Query;

        [Parameter]
        public string Query
        {
            get => _Query;
            set
            {
                if (_Query == value)
                {
                    return;
                }

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
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> Attributes { get; set; }

        public FilterDescriptor Filter { get; private set; } = new FilterDescriptor();

        private string QueryDebounced { get; set; }
        public string OrderByPropertyName { get; private set; }
        public bool OrderByDescending { get; private set; }
        private int TotalCount { get; set; }
        private IList<IGridCol> ColumnsList { get; set; } = new List<IGridCol>();
        public IEnumerable<IGridCol> Columns => ColumnsList;
        private Exception LoadingError { get; set; }

        public event EventHandler<int> OnAfterRowClicked;

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
                var cls = new List<string> { "blazor-grid" };

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

                // Subscribe to Filter object changes
                Filter.PropertyChanged += OnFilterChanged;
                Filter.Filters.CollectionChanged += OnFilterCollectionChanged;
            }
        }

        private void OnFilterCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => Reload();
        private void OnFilterChanged(object sender, PropertyChangedEventArgs e) => Reload();

        public Task Reload()
        {
            return LoadAsync(true);
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
                    QueryDebounced,
                    Filter
                );

                if (result != null)
                {
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

        public Task TryApplySorting<T>(Expression<Func<T>> property)
        {
            if (property == null)
            {
                return Task.CompletedTask;
            }

            var prop = GetPropertyName(property);

            if (OrderByPropertyName == prop)
            {
                OrderByDescending = !OrderByDescending;
            }
            else
            {
                OrderByPropertyName = prop;
                OrderByDescending = false;
            }

            return LoadAsync(true);
        }

        public string GetPropertyName<T>(Expression<Func<T>> property)
        {
            return ExpressionHelper.GetPropertyName<TRow, T>(property);
        }

        private string GridColumns
        {
            get
            {
                var sizes = ColumnsList.Select(col => col.FitToContent ? "max-content" : "auto");
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
            ColumnsList.Add(col);
            StateHasChanged();
        }

        public bool IsFilteredBy<T>(Expression<Func<T>> property)
        {
            if (property == null)
            {
                return false;
            }

            var prop = GetPropertyName(property);
            return Filter?.Filters.Any(x => x.Property == prop) == true;
        }

        public bool IsSortedBy<T>(Expression<Func<T>> property)
        {
            if (property == null)
            {
                return false;
            }

            var prop = GetPropertyName(property);
            return OrderByPropertyName == prop;
        }

        public void Dispose()
        {
            if (Filter != null)
            {
                Filter.PropertyChanged -= OnFilterChanged;

                if (Filter.Filters != null)
                {
                    Filter.Filters.CollectionChanged -= OnFilterCollectionChanged;
                }
            }
        }

        private TRow GetEmptyRow() => EmptyRow ?? Activator.CreateInstance<TRow>();
    }
}