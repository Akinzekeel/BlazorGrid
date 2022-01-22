using BlazorGrid.Interfaces;
using System.Linq.Expressions;

namespace BlazorGrid.Components
{
    public partial class StaticGridCol : ColBase, IGridCol
    {
        Expression IGridCol.For => null;
        public override string PropertyName => null;

        public override bool IsFilterable => false;

        protected override void OnParametersSet()
        {
            if (Register != null)
            {
                Register.Register(this);
            }
        }

        public override string GetCaptionOrDefault() => Caption;
    }
}