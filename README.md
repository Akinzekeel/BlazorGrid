# BlazorGrid
Coming from jQuery.DataTables to Blazor, I felt that wrapping the entire JavaScript library probably wasn't the best approach. The aforementioned library also included loads of features which I never used yet were always downloaded to the clients regardless.

So I started working on my own table/grid component and after reaching a point where it is stable enough to use, I want to share my work for free.

# Features
- Fetching remote data from an Api or HttpClient
- Sorting, filtering & paging (server-side only)
- Intelligent refresh

# BlazorGrid is not the right choice if...
- You need to handle client-side data
- You need client-side sorting, filtering or paging
- You need advanced features such as virtual scrolling, grouping or filtering with operators

# Dependencies
- Blazor
- A tiny CSS file
