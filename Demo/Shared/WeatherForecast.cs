using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Text;
using BlazorGrid.Abstractions;

namespace Demo.Shared
{
    public class WeatherForecast : IGridRow
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public string Summary { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string RowId => Date.ToString();
    }
}
