using Microsoft.AspNetCore.Components;

namespace BlazorGrid.Tests.Mock
{
    public class MockNav : NavigationManager
    {
        public MockNav()
        {
            Initialize("https://unit-test.example/", "https://unit-test.example/");
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
        }
    }
}