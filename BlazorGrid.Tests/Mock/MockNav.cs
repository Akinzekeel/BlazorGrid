using Microsoft.AspNetCore.Components;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using System;

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