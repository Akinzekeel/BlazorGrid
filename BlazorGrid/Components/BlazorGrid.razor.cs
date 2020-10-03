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
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorGrid.Components
{
    public partial class BlazorGrid<TRow> : IDisposable, IBlazorGrid where TRow : class
    {
#if DEBUG
        internal int RenderCount;
#endif
        public const int DefaultPageSize = 25;
        private readonly Type typeInfo = typeof(BlazorGrid<TRow>);
        private static readonly string[] ObservableParameterNames = new string[]
        {
            nameof(SourceUrl),
            nameof(Query),
            nameof(OnClick),
            nameof(Href),
            nameof(Attributes),
            nameof(Rows)
        };

        [Inject] public IGridProvider Provider { get; set; }
        [Inject] public IBlazorGridConfig Config { get; set; }
        [Inject] public NavigationManager Nav { get; set; }

        [Parameter] public string SourceUrl { get; set; }
        [Parameter] public RenderFragment<TRow> ChildContent { get; set; }
        [Parameter] public int PageSize { get; set; } = DefaultPageSize;
        [Parameter] public TRow EmptyRow { get; set; }

        private bool AreColumnsAdded;
        private bool IgnoreRender;
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
        private readonly IList<IGridCol> ColumnsList = new List<IGridCol>();
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

                if (Rows is null)
                {
                    InvokeAsync(() => LoadAsync(true));
                }

                // Subscribe to Filter object changes
                Filter.PropertyChanged += OnFilterChanged;
                Filter.Filters.CollectionChanged += OnFilterCollectionChanged;
            }

            if (AreColumnsAdded)
            {
                IgnoreRender = true;
            }
            else
            {
                // Now that the columns are processed, trigger another render
                AreColumnsAdded = true;
                StateHasChanged();
            }

#if DEBUG
            RenderCount++;
#endif
        }

        private void OnFilterCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => Reload();
        private void OnFilterChanged(object sender, PropertyChangedEventArgs e) => Reload();

        public Task Reload() => InvokeAsync(() => LoadAsync(true));

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

            try
            {
                var providerTask = Provider.GetAsync<TRow>(
                    SourceUrl,
                    Rows?.Count ?? 0,
                    PageSize,
                    OrderByPropertyName,
                    OrderByDescending,
                    QueryDebounced,
                    Filter
                );

                var delay = Task.Delay(100);

                await Task.WhenAny(providerTask, delay);

                if ((delay.IsCompleted && !providerTask.IsCompleted) || Initialize)
                {
                    IgnoreRender = false;
                    StateHasChanged();
                }

                var result = await providerTask;

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
                else
                {
                    Rows = null;
                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (HttpRequestException x)
            {
                LoadingError = x;
            }
            finally
            {
                IsLoadingMore = false;
                IgnoreRender = false;
                StateHasChanged();
            }
        }

        public Task TryApplySorting(IGridCol column)
        {
            if (column.PropertyName == null)
            {
                return Task.CompletedTask;
            }

            // Change direction if it's already sorted
            if (OrderByPropertyName == column.PropertyName)
            {
                OrderByDescending = !OrderByDescending;
            }
            else
            {
                OrderByPropertyName = column.PropertyName;
                OrderByDescending = false;
            }

            IgnoreRender = false;

            return InvokeAsync(() => LoadAsync(true));
        }

        public string GetPropertyName<T>(Expression<Func<T>> property)
        {
            return ExpressionHelper.GetPropertyName<TRow, T>(property);
        }

        private string GridColumns => ColumnsList
            .Select(col => col.FitToContent ? "max-content" : "auto")
            .Aggregate((a, b) => a + " " + b);

        public Task LoadMoreAsync()
        {
            if (IsLoadingMore)
            {
                return Task.CompletedTask;
            }

            IgnoreRender = false;

            return InvokeAsync(() => LoadAsync(false));
        }

        public int LastClickedRowIndex { get; private set; } = -1;

        private async Task OnRowClicked(int index)
        {
            LastClickedRowIndex = index;

            var r = Rows.ElementAt(index);
            var onClickUrl = Href?.Invoke(r);

            if (onClickUrl != null)
            {
                Nav.NavigateTo(onClickUrl);
            }
            else if (OnClick.HasDelegate)
            {
                await OnClick.InvokeAsync(r);
            }

            OnAfterRowClicked?.Invoke(this, index);
        }

        public void Add(IGridCol col)
        {
            ColumnsList.Add(col);
        }

        public bool IsFilteredBy(IGridCol column)
        {
            if (column.PropertyName == null)
            {
                return false;
            }

            return Filter?.Filters.Any(x => x.Property == column.PropertyName) == true;
        }

        public bool IsSortedBy(IGridCol column)
        {
            if (column.PropertyName == null)
            {
                return false;
            }

            return OrderByPropertyName == column.PropertyName;
        }

        /// <summary>
        /// Generate an empty row object. This is used when writing
        /// the grid header.
        /// </summary>
        /// <returns>A new row object</returns>
        private TRow GetEmptyRow()
        {
            return EmptyRow ?? Activator.CreateInstance<TRow>();
        }

        public override Task SetParametersAsync(ParameterView parameters)
        {
            IgnoreRender = true;

            var p = parameters.ToDictionary();

            foreach (var parameter in ObservableParameterNames)
            {
                if (!p.ContainsKey(parameter))
                {
                    continue;
                }

                if (!Equals(typeInfo.GetProperty(parameter), p[parameter]))
                {
                    IgnoreRender = false;
                    break;
                }
            }

            if (p.ContainsKey(nameof(ChildContent)) && !Equals(ChildContent, p[nameof(ChildContent)]))
            {
                foreach (var c in ColumnsList)
                {
                    c.Unlink();
                }

                ColumnsList.Clear();

                IgnoreRender = false;
                AreColumnsAdded = false;
            }

            return base.SetParametersAsync(parameters);
        }

        protected override bool ShouldRender()
        {
            return !IgnoreRender;
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
    }
}