using Microsoft.AspNetCore.Components;

namespace BlazorGrid.Tests.Mock
{
    internal class MockNav : NavigationManager
    {
        public MockNav()
        {
            Initialize("https://unit-test.example/", "https://unit-test.example/");
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            NotifyLocationChanged(false);
        }
    }
}