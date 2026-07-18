using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using AivoraPOS.Core.Models.Reports;
using SkiaSharp;

namespace AivoraPOS.App.Helpers;

public static class ReportChartFactory
{
    public static readonly SKColor Primary = SKColor.Parse("#1F4E79");
    public static readonly SKColor Accent = SKColor.Parse("#2E86C1");
    public static readonly SKColor Success = SKColor.Parse("#1F7A1F");
    public static readonly SKColor Warning = SKColor.Parse("#D68910");
    public static readonly TimeSpan NoAnimation = TimeSpan.Zero;

    private static readonly SKColor[] PiePalette =
    [
        SKColor.Parse("#1F4E79"),
        SKColor.Parse("#2E86C1"),
        SKColor.Parse("#5DADE2"),
        SKColor.Parse("#85C1E9"),
        SKColor.Parse("#AED6F1"),
        SKColor.Parse("#D4E6F1")
    ];

    public static ISeries[] CreateColumnSeries(IReadOnlyList<ChartDataPoint> data, string seriesName = "Sales")
    {
        return
        [
            new ColumnSeries<decimal>
            {
                Name = seriesName,
                Values = data.Select(d => d.Value).ToArray(),
                Fill = new SolidColorPaint(Primary),
                AnimationsSpeed = NoAnimation
            }
        ];
    }

    public static ISeries[] CreateLineSeries(IReadOnlyList<ChartDataPoint> data, string seriesName = "Revenue")
    {
        return
        [
            new LineSeries<decimal>
            {
                Name = seriesName,
                Values = data.Select(d => d.Value).ToArray(),
                Stroke = new SolidColorPaint(Primary) { StrokeThickness = 3 },
                Fill = new SolidColorPaint(Primary.WithAlpha(40)),
                GeometryFill = new SolidColorPaint(Accent),
                GeometryStroke = new SolidColorPaint(Primary) { StrokeThickness = 2 },
                AnimationsSpeed = NoAnimation
            }
        ];
    }

    public static ISeries[] CreatePieSeries(IReadOnlyList<ChartDataPoint> data)
    {
        return data.Select((point, index) => new PieSeries<decimal>
        {
            Name = point.Label,
            Values = new[] { point.Value },
            Fill = new SolidColorPaint(PiePalette[index % PiePalette.Length]),
            AnimationsSpeed = NoAnimation
        }).Cast<ISeries>().ToArray();
    }

    public static Axis[] CreateCategoryXAxis(IReadOnlyList<ChartDataPoint> data, int labelRotation = 0) =>
    [
        new Axis
        {
            Labels = data.Select(d => d.Label).ToArray(),
            LabelsRotation = labelRotation,
            TextSize = 12,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(80))
        }
    ];

    public static Axis[] CreateValueYAxis(string format = "C0") =>
    [
        new Axis
        {
            Labeler = value => value.ToString(format),
            TextSize = 12,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(80))
        }
    ];
}
