using BlazorGrid.Abstractions.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BlazorGrid.Abstractions
{
    public class BlazorGridRequest
    {
        public int Offset { get; set; }
        public int Length { get; set; } = 25;
        public string OrderBy { get; set; }
        public bool OrderByDescending { get; set; }
        public string Query { get; set; }

        public FilterDescriptor Filter { get; set; }

        public string FilterJson
        {
            get
            {
                return JsonSerializer.Serialize(Filter);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Filter = default;
                }
                else
                {
                    Filter = JsonSerializer.Deserialize<FilterDescriptor>(value);
                }
            }
        }

        public virtual IDictionary<string, object> ToDictionary()
            => new Dictionary<string, object> {
                { nameof(Query), Query },
                { nameof(Offset), Offset },
                { nameof(Length), Length },
                { nameof(OrderBy), OrderBy },
                { nameof(OrderByDescending), OrderByDescending },
                { nameof(FilterJson), FilterJson }
            };

        /// <summary>
        /// If you added any custom properties, override the ToDictionary
        /// method to have them included in the query string as well.
        /// 
        /// All values will be Uri-escaped.
        /// </summary>
        /// <returns>string with the properties in the form of a=1&amp;b=2&amp;c=3</returns>
        public string ToQueryString()
        {
            var dic = ToDictionary();
            var values = dic.Select(x =>
            {
                var v = x.Value?.ToString();

                if (!string.IsNullOrEmpty(v))
                    v = Uri.EscapeDataString(v);

                return x.Key + '=' + v;
            });

            return string.Join("&", values);
        }
    }
}