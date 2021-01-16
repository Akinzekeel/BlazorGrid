namespace BlazorGrid.Interfaces
{
    public interface IBlazorGridConfig
    {
        public int SearchQueryInputDebounceMs { get; set; }
        public IBlazorGridConfigStyles Styles { get; set; }
    }
}
