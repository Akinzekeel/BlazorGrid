using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Extensions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Abstractions.Helpers;
using BlazorGrid.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
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
    public partial class BlazorGrid<TRow> : ComponentBase, IDisposable, IBlazorGrid where TRow : class
    {
        private bool AreColumnsProcessed;
        private bool IgnoreSetParameters;
        private bool IsInitialRenderDone;
        private readonly Type typeInfo = typeof(BlazorGrid<TRow>);

        public const int DefaultPageSize = 25;
        public const int SearchQueryInputDebounceMs = 400;

        // Setting these parameters will cause a grid re-render
        private static readonly string[] RerenderParameterNames = new string[]
        {
            nameof(OnClick),
            nameof(Href),
            nameof(Attributes)
        };

        // Setting these parameters will not immediately cause a re-render, but a reload
        private static readonly string[] ReloadTriggerParameterNames = new string[]
        {
            nameof(SourceUrl),
            nameof(Query)
        };

        [Inject] private IGridProvider Provider { get; set; }
        [Inject] private IBlazorGridConfig Config { get; set; }
        [Inject] private NavigationManager Nav { get; set; }

        [Parameter] public List<TRow> Rows { get; set; }
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
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> Attributes { get; set; }

        private int? TotalRowCount;
        public FilterDescriptor Filter { get; private set; } = new FilterDescriptor();
        public string OrderByPropertyName { get; private set; }
        public bool OrderByDescending { get; private set; }

        private IList<IGridCol> RegisteredColumns = new List<IGridCol>();
        private IList<IGridCol> ColumnAddBuffer = new List<IGridCol>();
        public IEnumerable<IGridCol> Columns => RegisteredColumns;
        private Exception LoadingError { get; set; }
        private Virtualize<TRow> VirtualizeRef;
        private bool IgnoreRender;

        private async ValueTask<ItemsProviderResult<TRow>> GetItemsVirtualized(ItemsProviderRequest request)
        {
            var len = Math.Max(request.Count, PageSize);

            if (Rows != null)
            {
                var rows = Rows.AsQueryable();

                if (OrderByPropertyName != null)
                {
                    // Apply client-side sorting
                    if (OrderByDescending)
                    {
                        rows = rows.OrderByDescending(OrderByPropertyName);
                    }
                    else
                    {
                        rows = rows.OrderBy(OrderByPropertyName);
                    }
                }

                rows = rows
                    .Skip(request.StartIndex)
                    .Take(len);

                return new ItemsProviderResult<TRow>(rows, Rows.Count);
            }

            var result = await Provider.GetAsync<TRow>
            (
                SourceUrl,
                request.StartIndex,
                len,
                OrderByPropertyName,
                OrderByDescending,
                Query,
                Filter,
                request.CancellationToken
            );

            if (request.CancellationToken.IsCancellationRequested)
            {
                return await ValueTask.FromCanceled<ItemsProviderResult<TRow>>(request.CancellationToken);
            }

            TotalRowCount = result.TotalCount;
            return new ItemsProviderResult<TRow>(result.Data, result.TotalCount);
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
                var parameters = NextSetParametersAsyncMerge;
                parameters[nameof(Query)] = userInput;

                await SetParametersAsync(ParameterView.FromDictionary(parameters));
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

                // Subscribe to Filter object changes
                Filter.PropertyChanged += OnFilterChanged;
                Filter.Filters.CollectionChanged += OnFilterCollectionChanged;

                IsInitialRenderDone = true;
            }

            if (AreColumnsProcessed)
            {
                //IgnoreRender = true;
            }
            else if (ColumnAddBuffer.Any())
            {
                // Now that the columns are processed, trigger another render
                RegisteredColumns = ColumnAddBuffer;
                ColumnAddBuffer = new List<IGridCol>();
                AreColumnsProcessed = true;
                //IgnoreRender = false;
                StateHasChanged();
            }
        }

        private async void OnFilterCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => await ReloadAsync();
        private async void OnFilterChanged(object sender, PropertyChangedEventArgs e) => await ReloadAsync();

        public async Task ReloadAsync()
        {
            if (VirtualizeRef != null)
            {
                await VirtualizeRef.RefreshDataAsync();
            }

            StateHasChanged();
        }

        public async Task TryApplySortingAsync(IGridCol column)
        {
            if (column.PropertyName == null)
            {
                return;
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

            //IgnoreRender = false;
            await ReloadAsync();
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

        private async Task OnRowClicked(TRow row)
        {
            var onClickUrl = Href?.Invoke(row);

            if (onClickUrl != null)
            {
                IgnoreSetParameters = true;
                Nav.NavigateTo(onClickUrl);
            }
            else if (OnClick.HasDelegate)
            {
                IgnoreSetParameters = true;
                await OnClick.InvokeAsync(row);
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

        private Dictionary<string, object> NextSetParametersAsyncMerge;
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            if (IgnoreSetParameters)
            {
                IgnoreSetParameters = false;
                return;
            }

            //IgnoreRender = true;

            var mustReload = false;
            var p = parameters.ToDictionary().ToDictionary(x => x.Key, x => x.Value);

            if (p.ContainsKey(nameof(QueryUserInput)) && (string)p[nameof(QueryUserInput)] != QueryUserInput)
            {
                // This will cause a debounce after which SetParametersAsync is called again
                QueryUserInput = (string)p[nameof(QueryUserInput)];
                p.Remove(nameof(QueryUserInput));
                NextSetParametersAsyncMerge = p;

                // Cancel further processing
                return;
            }

            if (
                IsInitialRenderDone
                && p.Keys.Intersect(ReloadTriggerParameterNames)
                    .Any(x => (string)typeInfo.GetProperty(x).GetValue(this) != (string)p[x])
            )
            {
                await base.SetParametersAsync(parameters);
                mustReload = true;
            }

            if (
                IsInitialRenderDone
                && RegisteredColumns.Any()
                && p.ContainsKey(nameof(ChildContent))
            )
            {
                foreach (var c in RegisteredColumns)
                {
                    c.Unlink();
                }

                RegisteredColumns.Clear();
                ColumnAddBuffer.Clear();

                //IgnoreRender = false;
                AreColumnsProcessed = false;
            }

            if (mustReload)
            {
                await base.SetParametersAsync(parameters);
                await ReloadAsync();
                return;
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