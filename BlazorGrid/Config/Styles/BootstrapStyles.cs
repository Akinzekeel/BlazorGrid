using BlazorGrid.Interfaces;

namespace BlazorGrid.Config.Styles
{
    public class BootstrapStyles : IBlazorGridConfigStyles
    {
        public string PlaceholderWrapperClass => "my-5 text-center";
        public string LoadingSpinnerOuterClass => "text-muted my-2";
        public string LoadingSpinnerInnerClass => "spinner-grow spinner-grow-sm";
        public string LoadingTextClass => "text-muted small";
        public string ErrorHeadingClass => "h6 text-danger";
        public string ErrorSubHeadingClass => "text-danger";
        public string ErrorTextClass => "d-block text-danger small";
        public string NoDataHeadingClass => "h6 text-muted";
        public string NoDataTextClass => "text-muted small";
        public string FooterWrapperClass => "p-3 text-center";
        public string FooterTextClass => "text-muted my-3";
        public string FooterButtonClass => "btn btn-outline-secondary";
        public string FooterButtonLoadingClass => "";
        public string RowClickableClass => "clickable";
    }
}
