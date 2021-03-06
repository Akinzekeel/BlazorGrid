using BlazorGrid.Abstractions;
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
using System.Threading;
using System.Threading.Tasks;

namespace BlazorGrid.Components
{
    public partial class BlazorGrid<TRow> : ComponentBase, IDisposable, IBlazorGrid where TRow : class
    {
        private bool SkipNextRender;
        private bool DetectColumns = true;
        private readonly Type typeInfo = typeof(BlazorGrid<TRow>);

        // Setting these parameters will not immediately cause a re-render, but a reload
        private static readonly string[] ReloadTriggerParameterNames = new string[]
        {
            nameof(Provider),
            nameof(Query)
        };

        [Inject] private IBlazorGridConfig Config { get; set; }
        [Inject] private NavigationManager Nav { get; set; }

        [Parameter] public ProviderDelegate<TRow> Provider { get; set; }
        [Parameter] public int VirtualItemSize { get; set; } = 50;
        [Parameter] public RenderFragment<TRow> ChildContent { get; set; }
        [Parameter] public TRow EmptyRow { get; set; }
        [Parameter] public string Query { get; set; }
        [Parameter] public string QueryUserInput { get => QueryDebounceValue; set => SetQueryDebounced(value); }
        [Parameter] public EventCallback<TRow> OnClick { get; set; }
        [Parameter] public Func<TRow, string> Href { get; set; }
        [Parameter] public Expression<Func<TRow, object>> DefaultOrderBy { get; set; }
        [Parameter] public bool DefaultOrderByDescending { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> Attributes { get; set; }

        private bool? ShowLoadingOverlay;
        private string OrderByPropertyName;
        private bool OrderByDescending;

        public int? TotalRowCount { get; private set; }
        public FilterDescriptor Filter { get; private set; } = new FilterDescriptor();

        internal ICollection<IGridCol> Columns = new List<IGridCol>();
        private Exception LoadingError { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Modifizierer \"readonly\" hinzufügen", Justification = "<Ausstehend>")]
        private Virtualize<TRow> VirtualizeRef;

        private async ValueTask<ItemsProviderResult<TRow>> GetItemsVirtualized(ItemsProviderRequest request)
        {
            if (ShowLoadingOverlay is null)
            {
                ShowLoadingOverlay = true;
                StateHasChanged();
            }

            try
            {
                if (Provider != null && !request.CancellationToken.IsCancellationRequested)
                {
                    var providerRequest = new BlazorGridRequest
                    {
                        Query = Query,
                        OrderBy = OrderByPropertyName,
                        OrderByDescending = OrderByDescending,
                        Offset = request.StartIndex,
                        Length = request.Count,
                        Filter = Filter
                    };

                    var result = await Provider(providerRequest, request.CancellationToken);

                    TotalRowCount = result?.TotalCount ?? 0;

                    if (TotalRowCount != 0)
                    {
                        return new ItemsProviderResult<TRow>(result.Data, result.TotalCount);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                throw; // Let the Virtualize component handle this 
            }
            catch (OperationCanceledException)
            {
                // This can happen when the user scrolls the grid rapidly
                return default;
            }
            catch (ObjectDisposedException)
            {
                // This can happen when the user scrolls the grid rapidly
                return default;
            }
            catch (Exception x)
            {
                LoadingError = x;
            }
            finally
            {
                if (ShowLoadingOverlay == true)
                {
                    ShowLoadingOverlay = false;
                    StateHasChanged();
                }
            }

            // Return empty result set (fallback)
            return default;
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

            await Task.Delay(Config.SearchQueryInputDebounceMs);

            if (QueryDebounceValue == userInput)
            {
                var parameters = NextSetParametersAsyncMerge;
                NextSetParametersAsyncMerge = null;
                parameters[nameof(Query)] = userInput;
                await InvokeAsync(() => SetParametersAsync(ParameterView.FromDictionary(parameters)));
            }
        }

        private string CssClass
        {
            get
            {
                var cls = new List<string> { "blazor-grid-wrapper" };

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
            }
        }

        private async void OnFilterCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => await ReloadAsync();
        private async void OnFilterChanged(object sender, PropertyChangedEventArgs e) => await ReloadAsync();

        public async Task ReloadAsync()
        {
            // Clear error
            LoadingError = default;

            if (VirtualizeRef != null)
            {
                ShowLoadingOverlay = null;
                TotalRowCount = null;
                StateHasChanged();

                await VirtualizeRef.RefreshDataAsync();
                StateHasChanged();
            }
        }

        private async Task TryApplySortingAsync(IGridCol column)
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

            await ReloadAsync();
        }

        private string GridTemplateColumns()
        {
            var widths = Columns.Select(col => col.FitToContent ? "max-content" : "minmax(auto, 1fr)");
            return string.Join(' ', widths);
        }

        private async Task OnRowClicked(TRow row)
        {
            var onClickUrl = Href?.Invoke(row);

            if (onClickUrl != null)
            {
                SkipNextRender = true;
                Nav.NavigateTo(onClickUrl);
            }
            else if (OnClick.HasDelegate)
            {
                SkipNextRender = true;
                await OnClick.InvokeAsync(row);
            }
        }

        private bool IsFilteredBy(IGridCol column)
        {
            if (column.PropertyName == null)
            {
                return false;
            }

            return Filter?.Filters.Any(x => x.Property == column.PropertyName) == true;
        }

        private bool IsSortedBy(IGridCol column)
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
            var p = parameters.ToDictionary().ToDictionary(x => x.Key, x => x.Value);

            if (p.ContainsKey(nameof(QueryUserInput)) && (string)p[nameof(QueryUserInput)] != QueryUserInput)
            {
                // This will cause a debounce after which SetParametersAsync is called again
                QueryUserInput = (string)p[nameof(QueryUserInput)];
                p.Remove(nameof(QueryUserInput));
                NextSetParametersAsyncMerge = p;
                return;
            }

            if (p.ContainsKey(nameof(ChildContent)))
            {
                DetectColumns = true;
            }

            var mustReload = false;

            foreach (var k in ReloadTriggerParameterNames)
            {
                if (p.ContainsKey(k))
                {
                    var newVal = p[k]?.GetHashCode();
                    var oldVal = typeInfo.GetProperty(k).GetValue(this)?.GetHashCode();

                    mustReload = newVal != oldVal;
                }

                if (mustReload)
                {
                    break;
                }
            }

            await base.SetParametersAsync(parameters);

            if (mustReload)
            {
                await ReloadAsync();
            }
        }

        private void OnColumnsChanged(ICollection<IGridCol> cols)
        {
            DetectColumns = false;
            Columns = cols;
        }

        protected override bool ShouldRender()
        {
            var ret = !SkipNextRender;
            SkipNextRender = false;
            return ret;
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

            GC.SuppressFinalize(this);
        }

        private string ColHeaderSortIconCssClass(IGridCol col)
        {
            var cls = "blazor-grid-sort-icon";

            if (IsSortedBy(col))
            {
                cls += " active";

                if (OrderByDescending)
                {
                    cls += " sorted-desc";
                }
                else
                {
                    cls += " sorted-asc";
                }
            }

            return cls;
        }
    }
}