using BlazorGrid.Interfaces;

namespace BlazorGrid.Config.Styles
{
    public class SpectreStyles : IBlazorGridConfigStyles
    {
        public string PlaceholderWrapperClass => "empty";
        public string ErrorHeadingClass => "empty-title text-error";
        public string ErrorSubHeadingClass => "empty-subtitle text-error";
        public string ErrorTextClass => "d-block text-error";
        public string NoDataHeadingClass => "empty-title";
        public string NoDataTextClass => "empty-subtitle";
        public string FooterWrapperClass => "empty pt-3";
        public string FooterTextClass => "empty-subtitle";
        public string RowClickableClass => "clickable";
    }
}
