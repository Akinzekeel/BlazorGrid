using BlazorGrid.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;

namespace BlazorGrid.Components
{
    public abstract class ColBase : ComponentBase
    {
        [Parameter] public string Caption { get; set; }
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public bool FitToContent { get; set; }
        [Parameter] public bool AlignRight { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> Attributes { get; set; }
        [CascadingParameter] internal IColumnRegister Register { get; set; }

        public string CssClass
        {
            get
            {
                var cls = new List<string>
                {
                    AlignRight ? "text-right" : ""
                };

                // Merge custom CSS classes if necessary
                if (Attributes != null)
                {
                    string customClasses = Attributes
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

        public abstract bool IsFilterable { get; }
        public abstract string PropertyName { get; }
        public abstract string GetCaptionOrDefault();
    }
}
