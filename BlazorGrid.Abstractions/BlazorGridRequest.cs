using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorGrid.Abstractions
{
    public class BlazorGridRequest
    {
        public int Offset { get; set; }
        public int Length { get; set; } = 25;
        public string OrderBy { get; set; }
        public bool OrderByDescending { get; set; }
        public string Query { get; set; }
        public IEnumerable<object> ExcludedKeys { get; set; }

        public virtual IDictionary<string, object> ToDictionary()
            => new Dictionary<string, object> {
                { nameof(Query), Query },
                { nameof(Offset), Offset },
                { nameof(Length), Length },
                { nameof(OrderBy), OrderBy },
                { nameof(OrderByDescending), OrderByDescending },
                { nameof(ExcludedKeys), ExcludedKeys?.Any() == true ? string.Join(",", ExcludedKeys) : null }
            };

        /// <summary>
        /// Returns the properties & values in the form of a=1&b=2&c=3
        ///
        /// If you added any custom properties, override the ToDictionary
        /// method to have them included in the query string as well
        /// </summary>
        /// <returns></returns>
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