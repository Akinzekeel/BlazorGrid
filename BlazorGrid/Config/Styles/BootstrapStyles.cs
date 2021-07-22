using BlazorGrid.Interfaces;

namespace BlazorGrid.Config.Styles
{
    public class BootstrapStyles : IBlazorGridConfigStyles
    {
        public string LoadingSpinnerOuterClass => "text-muted my-2";
        public string LoadingSpinnerInnerClass => "spinner-grow spinner-grow-sm";
        public string LoadingTextClass => "text-muted small";
        public string PlaceholderWrapperClass => "my-5 text-center";
        public string ErrorHeadingClass => "h6 text-danger";
        public string ErrorSubHeadingClass => "text-danger";
        public string ErrorTextClass => "d-block text-danger small";
        public string ErrorFooterClass => "text-center my-3";
        public string ErrorFooterBtnClass => "btn btn-sm btn-danger";
        public string NoDataHeadingClass => "h6 text-muted";
        public string NoDataTextClass => "text-muted small";
        public string FooterWrapperClass => "m-5 text-center";
        public string FooterTextClass => "text-muted mb-3";
        public string RowClickableClass => "clickable";
        public string RowHighlightedClass => "highlighted";
    }
}
