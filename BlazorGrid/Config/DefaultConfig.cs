using BlazorGrid.Interfaces;

namespace BlazorGrid.Config
{
    public class DefaultConfig : IBlazorGridConfig
    {
        public IBlazorGridConfigStyles Styles { get; set; }
    }
}
