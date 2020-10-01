using BlazorGrid.Config.Styles;
using BlazorGrid.Interfaces;

namespace BlazorGrid.Config
{
    public class DefaultConfig : IBlazorGridConfig
    {
        public DefaultConfig()
        {
            Styles = new BootstrapStyles();
        }

        public IBlazorGridConfigStyles Styles { get; set; }
    }
}
