using System.Text.Json.Serialization;

namespace BlazorGrid.Abstractions.Filters
{
    public class PropertyFilter
    {
        [JsonPropertyName("p")]
        public string Property { get; set; }

        [JsonPropertyName("v")]
        public string Value { get; set; }

        [JsonPropertyName("o")]
        public FilterOperator Operator { get; set; }
    }
}
