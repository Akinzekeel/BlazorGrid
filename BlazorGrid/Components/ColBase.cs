using BlazorGrid.Interfaces;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorGrid.Components
{
    public abstract class ColBase : ComponentBase
    {
        [Parameter] public string? Caption { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public bool FitToContent { get; set; }
        [Parameter] public bool AlignRight { get; set; }
        [CascadingParameter] internal IColumnRegister? Register { get; set; }
        [CascadingParameter(Name = "RowClass")] internal string? RowClass { get; set; }
        [CascadingParameter(Name = "RowClickCallback")] internal Func<Task>? RowClickCallback { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? Attributes { get; set; }

        public abstract bool IsFilterable { get; }
        public abstract string? PropertyName { get; }
        public abstract string GetCaptionOrDefault();

        public string CssClass
        {
            get
            {
                var cls = new List<string?>
                {
                    "grid-cell",
                    RowClass,
                    AlignRight ? "text-right" : ""
                };

                // Merge custom CSS classes if necessary
                if (Attributes != null)
                {
                    var customClasses = Attributes?
                        .Where(x => x.Key == "class")
                        .Select(x => x.Value?.ToString())
                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(customClasses))
                    {
                        cls.AddRange(customClasses.Split(' '));
                    }
                }

                return string.Join(' ', cls).Trim();
            }
        }

        protected IDictionary<string, object> FinalAttributes
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

        protected Task OnClickBinder()
        {
            return RowClickCallback?.Invoke() ?? Task.CompletedTask;
        }
    }
}
