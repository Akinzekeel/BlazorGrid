![NuGet package](https://img.shields.io/nuget/vpre/Akinzekeel.BlazorGrid)

# BlazorGrid
BlazorGrid is yet another grid component for Blazor. I was inspired to write this because I've been using jQuery.DataTables for a long time and felt that incorporating more than 15,000 lines of JavaScript into my Blazor application probably wasn't the best approach. 

The primary goals of this component are:
- Displaying remote data by utilizing virtualization
- Sorting and searching of remote data (backend support required)
- Require as little markup as possible to define the grid
- Use custom RenderFragments for the cells
- Highly customizable UI by providing SCSS
- Using a delegate to provide the data (which can pull the data via JSON, gRPC or anything else)
- High performance by intelligently preventing unnecessary render events

Check out the website for more information & demos:
## [blazorgrid.majidcodes.net](https://blazorgrid.majidcodes.net/)
