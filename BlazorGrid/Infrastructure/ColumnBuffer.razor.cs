using BlazorGrid.Interfaces;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorGrid.Infrastructure
{
    public partial class ColumnBuffer<TRow> : ComponentBase
    {
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public EventCallback<ICollection<IGridCol>> OnColumnsChanged { get; set; }

        private ColumnRegister<TRow> CurrentRegister = new ColumnRegister<TRow>();
        private ColumnRegister<TRow> Buffer;

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            return DetectChangesAsync(Buffer);
        }

        private async Task DetectChangesAsync(ColumnRegister<TRow> otherRegister)
        {
            if (otherRegister is null)
            {
                throw new ArgumentNullException();
            }

            if (CurrentRegister is null || otherRegister.GetHashCode() != CurrentRegister.GetHashCode() || otherRegister.Columns.Count != CurrentRegister.Columns.Count)
            {
                CurrentRegister = otherRegister;
                await OnColumnsChanged.InvokeAsync(CurrentRegister.Columns);
            }
        }
    }
}
