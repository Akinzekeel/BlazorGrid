using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace BlazorGrid.Abstractions.Filters
{
    public class FilterDescriptor : INotifyPropertyChanged
    {
        private ConnectorType connector = ConnectorType.All;

        [JsonPropertyName("c")]
        public ConnectorType Connector
        {
            get => connector; set
            {
                connector = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Connector)));
            }
        }

        [JsonPropertyName("f")]
        public ObservableCollection<PropertyFilter> Filters { get; set; } = new ObservableCollection<PropertyFilter>();

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
