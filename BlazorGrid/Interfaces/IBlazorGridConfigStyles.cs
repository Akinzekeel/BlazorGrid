namespace BlazorGrid.Interfaces
{
    public interface IBlazorGridConfigStyles
    {
        public string PlaceholderWrapperClass { get; }

        public string LoadingSpinnerOuterClass { get; }
        public string LoadingSpinnerInnerClass { get; }
        public string LoadingTextClass { get; }

        public string ErrorHeadingClass { get; }
        public string ErrorSubHeadingClass { get; }
        public string ErrorTextClass { get; }

        public string NoDataHeadingClass { get; }
        public string NoDataTextClass { get; }

        public string FooterWrapperClass { get; }
        public string FooterTextClass { get; }
        public string FooterButtonClass { get; }
        public string FooterButtonLoadingClass { get; }

        public string RowClickableClass { get; }
    }
}
