using BlazorGrid.Interfaces;

namespace BlazorGrid.Config.Styles
{
    public class SpectreStyles : IBlazorGridConfigStyles
    {
        public string LoadingSpinnerOuterClass => "empty-title";
        public string LoadingSpinnerInnerClass => "loading";
        public string LoadingTextClass => "empty-subtitle";
        public string PlaceholderWrapperClass => "empty";
        public string ErrorHeadingClass => "empty-title text-error";
        public string ErrorSubHeadingClass => "empty-subtitle text-error";
        public string ErrorTextClass => "d-block text-error";
        public string ErrorFooterClass => "text-center my-2";
        public string ErrorFooterBtnClass => "btn btn-sm btn-error";
        public string NoDataHeadingClass => "empty-title";
        public string NoDataTextClass => "empty-subtitle";
        public string FooterWrapperClass => "empty pt-3";
        public string FooterTextClass => "empty-subtitle";
        public string RowClickableClass => "clickable";
        public string RowHighlightedClass => "highlighted";
    }
}
