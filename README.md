![NuGet package](https://img.shields.io/nuget/vpre/Akinzekeel.BlazorGrid)

# BlazorGrid
I've been a long time user of jQuery.DataTables and I think it's a really great plugin. However, even with no extra features and without jQuery, we're talking about some 15,000 lines of JavaScript. I needed a data grid in my Blazor project lately and I felt that wrapping a JavaScript library of this size wasn't something I really wanted to do, so instead I started writing my own component called BlazorGrid.

BlazorGrid does not match all of the features of jQuery.DataTables, however it is a lot more light-weight and does not depend on JavaScript at all.

## [See a demo & documentation here](https://blazorgrid.z6.web.core.windows.net/)

# Roadmap
This package is currently in preview.

I would like to get the following things done before removing the preview status:
1. Localization
2. Support different data providers per grid
3. ~~Provide helper methods and classes for quick server-side setup~~
4. ~~Find a better way to perform smart-refresh which doesn't require IGridRow~~
5. Improve documentation
6. ~~Finish the demo project & host it~~
7. Responsive support