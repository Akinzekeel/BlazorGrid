![NuGet package](https://github.com/Akinzekeel/BlazorGrid/workflows/Publish%20main%20project%20to%20NuGet/badge.svg)

# BlazorGrid
I've been a long time user of jQuery.DataTables and I think it's a really great plugin. However, even with no extra features and without jQuery, we're talking about some 15,000 lines of JavaScript. I needed a data grid in my Blazor project lately and I felt that wrapping a JavaScript library of this size wasn't something I really wanted to do, so instead I started writing my own component called BlazorGrid.

BlazorGrid does not match all of the features of jQuery.DataTables, however it is also a lot more light-weight and does not depend on JavaScript at all.

**WARNING: This package is currently in preview (version 0.2.0-beta at the point of writing this warning). See the end of the file for information on what's missing.**

## Features
- Fetching remote data page-wise
- Sorting, filtering & paging (server-side only)
- Smart refresh
- Based on CSS grids instead of table elements

### BlazorGrid is probably not the right choice if...
- You need to handle client-side data
- You need client-side sorting, filtering or paging
- You need advanced features such as virtual scrolling, grouping or complex filtering with operators

## Setup
### Step 1: Install
In your client-side Blazor project, install the NuGet package with the following command: 
```powershell
dotnet add package Akinzekeel.BlazorGrid
```

On your server-side Blazor or Web Api project and also in your shared project (or wherever your models or dto's are located), install the following NuGet package:
```powershell
dotnet add package Akinzekeel.BlazorGrid.Abstractions
```

### Step 2: IGridProvider
BlazorGrid will usually request data in batches by using **offset** and **length**. These can easily be translated to Linq on the server side with the `Take()` and `Skip()` methods (see step 4 on how to do that).

However, there are scenarios where we do not have any control over the server-side. For these cases, you can implement your own IGridProvider. It is the responsibility of this provider to place requests in the correct format and then return the data as a GridPageResult<T>.
  
If you have control over the data-source/api/server, then I recommend you to use the DefaultHttpProvider.

To set up a provider in dependency injection, go to **Program.cs** in your client-side project and register the service like so:

```c#
using BlazorGrid.Providers;
using BlazorGrid.Abstractions;
// ...
builder.Services.AddTransient<IGridProvider, DefaultHttpProvider>();
``` 

### Step 3: Set up your models
Models or dto's which you want to display in the grid need to have a unique row id. Therefore your models must implement `IGridRow` which has a string getter called `RowId`. The `RowId` is required for smart refresh to work.

### Step 4 (optional): Set up your Web Api
If you are pulling data from your own Web Api project, then your action method should look something like this (assuming you use the DefaultHttpProvider):
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
  <GridCol TRow="MyDto">@context.Name</Column>
  <GridCol TRow="MyDto">@context.Timestamp</Column>
</BlazorGrid>
```

## Custom styling
The component comes with a default css file which you can use out of the box for a default style. This default style is available at `_content/Akinzekeel.BlazorGrid/dist/blazor-grid.min.css`. However, for best customization I recommend you to use the SCSS files and integrate them into your own stylesheet. The SCSS files are located in this repository under `BlazorGrid/Styles`.

Some of the styles & variables are based on the amazing [Spectre CSS framework](https://picturepan2.github.io/spectre/). If you already use that then customization will be even easier.

# Roadmap
This package is currently in preview lest because it is both my very first GitHub repository and my first NuGet package so I've got plenty to learn and discover. 

I would like to get the following things done before removing the preview status:
1. Localization
1. Support different data providers per grid
1. Provide helper methods and classes for quick server-side setup
1. Find a better way to perform smart-refresh which doesn't require IGridRow
1. Improve documentation
1. Finish the demo project & host it
1. Responsive support

If you want to contribute to this project you're always welcome to do so. In case of questions or comments you can also contact me on Twitter at @Akinzekeel.
