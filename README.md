![.NET Core](https://github.com/Akinzekeel/BlazorGrid/workflows/.NET%20Core/badge.svg)

# BlazorGrid
Coming from jQuery.DataTables to Blazor, I felt that wrapping the entire JavaScript library probably wasn't the best approach. The aforementioned library also included loads of features which I never used yet were always downloaded to the clients regardless.

So I started working on my own table/grid component and after reaching a point where it is stable enough to use, I want to share my work for free.

## Features
- Fetching remote data from an Api or HttpClient
- Sorting, filtering & paging (server-side only)
- Intelligent refresh

## BlazorGrid is not the right choice if...
- You need to handle client-side data
- You need client-side sorting, filtering or paging
- You need advanced features such as virtual scrolling, grouping or filtering with operators

## Dependencies
- Blazor
- A tiny CSS file

## Usage
<BlazorGrid>
  <Column>@context.Name</Column>
  <Column>@context.Timestamp</Column>
</BlazorGrid>

## Custom styling
The component comes with a css file which you can use out of the box for a default style. For customization purposes there is also an SCSS file which can be used to customize colors and other properties easily.

The following table shows how to target elements in the grid:

| CSS Rule | Element |
---------------------
| .grid-row > * | Corresponds to any cell, including header cells |
| .grid-header .grid-row > * | Corresponds to header cells only |
