using BlazorGrid.Abstractions.Filters;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BlazorGrid.Interfaces
{
    public interface IBlazorGrid
    {
        FilterDescriptor Filter { get; }
    }
}