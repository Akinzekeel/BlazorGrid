using System;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using BlazorGrid.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using BlazorGrid.Components;
using BlazorGrid.Tests.Mock;

namespace EOS.Client.Tests.ComponentTests
{
    [TestClass]
    public class DataTableTests : ComponentTestFixture
    {
        class MyDto : IGridRow
        {
            public MyDto() : this(Guid.NewGuid()) { }
            public MyDto(Guid id) { Id = id; }

            public Guid Id { get; private set; }
            public string Name { get; set; } = Guid.NewGuid().ToString();
            public string RowId => Id.ToString();
        }

        // [DataTestMethod]
        // [DataRow(0)]
        // [DataRow(1)]
        // [DataRow(24)]
        // [DataRow(26)]
        // [DataRow(49)]
        // public async Task Can_Refresh(int clickedRowIndex)
        // {
        //     var fakeProvider = new Mock<IGridProvider>();
        //     var dataSet = Enumerable.Repeat(new MyDto(), 55).ToList();

        //     var pageSize = BlazorGrid<MyDto>.DefaultPageSize;
        //     fakeProvider.Setup(x => x.GetAsync<MyDto>(
        //         It.IsAny<string>(),
        //         It.Is<int>(o => o >= 0),
        //         It.Is<int>(l => l >= 1),
        //         It.IsAny<string>(),
        //         It.IsAny<bool>(),
        //         It.IsAny<string>()
        //     ))
        //         .ReturnsAsync((string url, int offset, int len, string _, bool __, string ___) =>
        //         {
        //             var fakeResult = new BlazorGridResult<MyDto>
        //             {
        //                 Data = dataSet.Skip(offset).Take(len).ToList(),
        //                 TotalCount = dataSet.Count
        //             };

        //             return fakeResult;
        //         }
        //         );

        //     Services.AddSingleton(fakeProvider.Object);
        //     Services.AddSingleton<NavigationManager>(new MockNav());

        //     Func<MyDto, string> href = (MyDto _) => "/unit-test";

        //     var unit = RenderComponent<BlazorGrid<MyDto>>(
        //         Parameter(nameof(BlazorGrid<MyDto>.SourceUrl), "unit-test"),
        //         Parameter(nameof(BlazorGrid<MyDto>.Href), href),
        //         Template<MyDto>(nameof(ChildContent), (row) => (builder) =>
        //         {
        //             builder.AddContent(0, RenderComponent<GridCol>(
        //                 ChildContent($"<span>this is row id {row.Name}</span>")
        //             ));
        //         })
        //     );

        //     fakeProvider.VerifyAll();

        //     var expectedRowCount = pageSize;

        //     if (clickedRowIndex + 1 > pageSize)
        //     {
        //         // Load another page
        //         await unit.Instance.LoadMoreAsync();
        //         expectedRowCount += pageSize;
        //     }

        //     var rows = unit.FindAll(".grid-row.clickable");
        //     Assert.AreEqual(expectedRowCount, rows.Count);

        //     var clicky = rows.ElementAt(clickedRowIndex);
        //     clicky.Click();

        //     Assert.AreEqual(clickedRowIndex, unit.Instance.LastClickedRowIndex);

        //     // Modify the row so we can later verify that a re-render has occured
        //     var clickedDto = dataSet.ElementAt(clickedRowIndex);
        //     var updatedDto = new MyDto(clickedDto.Id);

        //     fakeProvider.Setup(x => x.ReloadAsync<MyDto>(It.IsAny<string>(), clickedDto))
        //         .ReturnsAsync(updatedDto);

        //     await unit.Instance.RefreshAsync();

        //     var gridRow = unit.FindAll(".grid-row.clickable").ElementAt(clickedRowIndex);
        //     Assert.IsTrue(gridRow.TextContent.Contains(updatedDto.Name.ToString()), "The new row id was not rendered");
        // }
    }
}