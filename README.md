![NuGet package](https://img.shields.io/nuget/vpre/Akinzekeel.BlazorGrid)

# BlazorGrid
I've been a long time user of jQuery.DataTables and I think it's a really great plugin. However, even with no extra features and without jQuery, we're talking about some 15,000 lines of JavaScript. I needed a data grid in my Blazor project lately and I felt that wrapping a JavaScript library of this size wasn't something I really wanted to do, so instead I started writing my own component called BlazorGrid.

BlazorGrid does not match all of the features of jQuery.DataTables, however it is a lot more light-weight and does not depend on JavaScript at all.

## [Click here to see a live demo](https://blazorgrid.z6.web.core.windows.net/)

**WARNING: This package is currently in preview. See the end of the file for information on what's missing.**

## Features
- Fetching remote data page-wise
- Sorting, filtering & paging (server-side only)
- Tableless / based on pure CSS grids
- Limited support for client-side data

### BlazorGrid is probably not the right choice if...
- You need client-side sorting, filtering or paging
- You need advanced features such as virtual scrolling, grouping or complex filtering with operators

## Setup
### Step 1: Install
In your client-side Blazor project, install the NuGet package with the following command:
```powershell
dotnet add package Akinzekeel.BlazorGrid
```
In the project which will provide your remote data (Web Api or other) you may optionally install the following NuGet package:
```powershell
dotnet add package Akinzekeel.BlazorGrid.Abstractions
```
This optional package contains extension methods and interfaces without depending on Mvc or other packages.

### Step 2: Set up a provider
BlazorGrid will usually request data in batches by using **offset** and **length**. These can easily be translated to Linq on the server side with the `Take()` and `Skip()` methods.

However, there are scenarios where we do not have any control over the server-side. For these cases, you can implement your own `IGridProvider`. It is the responsibility of this provider to place requests in the correct format and then return the data as a `BlazorGridResult<T>`.

If you have control over the data-source, api or server, then I recommend you to use the DefaultHttpProvider and adjust your action methods to take a parameter of type `BlazorGridRequest` (see step 3).

Whether you write a custom provider or use the builtin one, you will need to set it up in your dependency injection configuration.

To set up a provider in dependency injection, go to **Program.cs** in your client-side project and register the service like so:

```c#
using BlazorGrid.Providers;
using BlazorGrid.Abstractions;
// ...
builder.Services.AddTransient<IGridProvider, DefaultHttpProvider>();
```

### Step 3: Set up your Web Api
If you are pulling data from your own Web Api project, then your action method should look something like this (assuming you use the `DefaultHttpProvider`):
```c#
using System.Threading;
using BlazorGrid.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Sample
{
    public class MySampleController : ControllerBase
    {
        protected readonly ISampleRepository Repository;

        protected MySampleController(ISampleRepository r)
        {
            Repository = r;
        }

        [HttpGet]
        [Route("")]
        public Task<IActionResult> Index([FromQuery]GridPageRequest Args)
        {
            if (Args.Offset < 0)
                return BadRequest();

            if (Args.Length < 1)
                return BadRequest("Invalid length (must be 1 or more)");

            else if (Args.Length > 100)
                return BadRequest("Invalid length (must be 100 or less)");

            IQueryable<MyDto> q = Repository.Read();

            if (Query != null)
                q = Repository.ApplySearchFilter(q, Query);

            var totalCount = await q.CountAsync();

            if (totalCount == 0)
            {
                return Ok(new GridPageResult<MyDto>
                {
                    Data = new List<MyDto>(),
                    TotalCount = 0
                });
            }

            // Apply ordering
            if (OrderBy != null)
            {
                if (OrderByDescending)
                    q = q.OrderByDescending(OrderBy);

                else
                    q = q.OrderBy(OrderBy);
            }

            // Apply paging
            if (Offset > 0)
                q = q.Skip(Offset);

            if (totalCount > Length)
                q = q.Take(Length);

            var rows = await q.ToListAsync();

            return Ok(new GridPageResult<MyDto>
            {
                Data = rows,
                TotalCount = totalCount
            });
        }
    }
}
```

## Usage
Now that the setup is complete, it's time to see the grid in action! Create a razor page in your client project and add the following code:
```razor
<BlazorGrid TRow="MyDto" SourceUrl="Api/MySample">
  <GridCol>@context.Name</Column>
  <GridCol>@context.Timestamp</Column>
</BlazorGrid>
```

## Custom styling
The component comes with a default css file which you can use out of the box for a default style. This default style is available at `_content/Akinzekeel.BlazorGrid/dist/blazor-grid.min.css`. However, for best customization I recommend you to use the SCSS files and integrate them into your own stylesheet. The SCSS files are located in this repository under `BlazorGrid/Styles`.

Some of the styles & variables are based on the amazing [Spectre CSS framework](https://picturepan2.github.io/spectre/). If you already use that then customization will be even easier.

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

If you want to contribute to this project you're always welcome to do so. In case of questions or comments you can also contact me on Twitter at @Akinzekeel.
