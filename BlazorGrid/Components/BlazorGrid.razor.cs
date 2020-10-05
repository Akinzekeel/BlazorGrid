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
        internal int RenderCount { get; private set; }
        internal bool IgnoreRender { get; private set; }

        private bool AreColumnsProcessed;
        private bool IgnoreSetParameters;
        private bool IsLoadingMore;
        private readonly Type typeInfo = typeof(BlazorGrid<TRow>);

        public const int DefaultPageSize = 25;
        public const int SearchQueryInputDebounceMs = 400;

        // Setting these parameters will cause a grid re-render
        private static readonly string[] RerenderParameterNames = new string[]
        {
            nameof(OnClick),
            nameof(Href),
            nameof(Attributes),
            nameof(Rows)
        };

        // Setting these parameters will not immediately cause a re-render, but a reload
        private static readonly string[] ReloadTriggerParameterNames = new string[]
        {
            nameof(SourceUrl),
            nameof(Query)
        };

        // These parameters will be set without causing a render
        private static readonly string[] PassThroughParameterNames = new string[]
        {
            nameof(QueryUserInput)
        };

        [Inject] public IGridProvider Provider { get; set; }
        [Inject] public IBlazorGridConfig Config { get; set; }
        [Inject] public NavigationManager Nav { get; set; }

        [Parameter] public string SourceUrl { get; set; }
        [Parameter] public RenderFragment<TRow> ChildContent { get; set; }
        [Parameter] public int PageSize { get; set; } = DefaultPageSize;
        [Parameter] public TRow EmptyRow { get; set; }
        [Parameter] public string Query { get; set; }
        [Parameter] public string QueryUserInput { get => QueryDebounceValue; set => SetQueryDebounced(value); }
        [Parameter] public EventCallback<TRow> OnClick { get; set; }
        [Parameter] public Func<TRow, string> Href { get; set; }
        [Parameter] public Expression<Func<TRow, object>> DefaultOrderBy { get; set; }
        [Parameter] public bool DefaultOrderByDescending { get; set; }
        [Parameter] public List<TRow> Rows { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> Attributes { get; set; }

        public FilterDescriptor Filter { get; private set; } = new FilterDescriptor();

        public string OrderByPropertyName { get; private set; }
        public bool OrderByDescending { get; private set; }
        private int TotalCount { get; set; }
        private IList<IGridCol> RegisteredColumns = new List<IGridCol>();
        private IList<IGridCol> ColumnAddBuffer = new List<IGridCol>();
        public IEnumerable<IGridCol> Columns => RegisteredColumns;
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

        private string QueryDebounceValue;
        private async void SetQueryDebounced(string userInput)
        {
            if (Query == userInput)
            {
                return;
            }

            QueryDebounceValue = userInput;

            await Task.Delay(SearchQueryInputDebounceMs);

            if (QueryDebounceValue == userInput)
            {
                await SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object>
                {
                    { nameof(Query), userInput }
                }));
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

            if (AreColumnsProcessed)
            {
                IgnoreRender = true;
            }
            else if (ColumnAddBuffer.Any())
            {
                // Now that the columns are processed, trigger another render
                RegisteredColumns = ColumnAddBuffer;
                ColumnAddBuffer = new List<IGridCol>();
                AreColumnsProcessed = true;
                IgnoreRender = false;
                StateHasChanged();
            }

            RenderCount++;
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
            IgnoreRender = false;
            StateHasChanged();

            try
            {
                var result = await Provider.GetAsync<TRow>(
                    SourceUrl,
                    Rows?.Count ?? 0,
                    PageSize,
                    OrderByPropertyName,
                    OrderByDescending,
                    Query,
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

        private string GridTemplateColumns()
        {
            var widths = RegisteredColumns.Select(col => col.FitToContent ? "max-content" : "auto");
            return string.Join(' ', widths);
        }

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
                IgnoreSetParameters = true;
                Nav.NavigateTo(onClickUrl);
            }
            else if (OnClick.HasDelegate)
            {
                IgnoreSetParameters = true;
                await OnClick.InvokeAsync(r);
            }

            if (OnAfterRowClicked != null)
            {
                IgnoreSetParameters = true;
                OnAfterRowClicked.Invoke(this, index);
            }
        }

        public bool Register(IGridCol col)
        {
            if (!col.IsRegistered)
            {
                ColumnAddBuffer.Add(col);
                return true;
            }

            return false;
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

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            if (IgnoreSetParameters)
            {
                IgnoreSetParameters = false;
                return;
            }

            var p = parameters.ToDictionary();

            IgnoreRender = true;

            if (p.Keys.Intersect(PassThroughParameterNames).Count() == p.Count)
            {
                await base.SetParametersAsync(parameters);
                return;
            }

            if (p.Keys.Intersect(ReloadTriggerParameterNames).Count() == p.Count)
            {
                await base.SetParametersAsync(parameters);
                await InvokeAsync(() => LoadAsync(true));
                return;
            }

            if (
                RegisteredColumns.Any()
                && p.ContainsKey(nameof(ChildContent))
            )
            {
                foreach (var c in RegisteredColumns)
                {
                    c.Unlink();
                }

                RegisteredColumns.Clear();
                ColumnAddBuffer.Clear();

                IgnoreRender = false;
                AreColumnsProcessed = false;
            }

            if (IgnoreRender)
            {
                foreach (var parameter in RerenderParameterNames)
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
            }

            await base.SetParametersAsync(parameters);
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