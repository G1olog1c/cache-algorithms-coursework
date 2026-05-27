using ScottPlot;

namespace CacheAlgorithms.Benchmark.Reporting;

/// <summary>
/// Generates PNG charts from benchmark results using ScottPlot.
/// </summary>
public static class PlotReporter
{
    private const int PlotWidthPx = 900;
    private const int PlotHeightPx = 600;

    public static void Save(IReadOnlyList<BenchmarkResult> results, string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        Directory.CreateDirectory(outputDirectory);

        foreach (var workloadGroup in results.GroupBy(r => r.WorkloadName))
        {
            var safeName = MakeFileSafe(workloadGroup.Key);
            var rows = workloadGroup.ToList();

            SaveHitRateBarChart(rows, Path.Combine(outputDirectory, $"hitrate_bars_{safeName}.png"));
            SaveHitRateVsCapacityChart(rows, Path.Combine(outputDirectory, $"hitrate_vs_capacity_{safeName}.png"));
        }
    }

    private static void SaveHitRateBarChart(IReadOnlyList<BenchmarkResult> rows, string path)
    {
        var plot = new Plot();

        var capacities = rows.Select(r => r.Capacity).Distinct().OrderBy(c => c).ToList();
        var strategies = rows.Select(r => r.StrategyName).Distinct().OrderBy(s => s).ToList();

        // Sized so all bars in a group sit inside [groupIndex - 0.4 .. groupIndex + 0.4]
        // regardless of strategy count.
        const double groupWidth = 0.8;
        var barWidth = groupWidth / strategies.Count;

        for (var si = 0; si < strategies.Count; si++)
        {
            var bars = new List<Bar>();
            for (var ci = 0; ci < capacities.Count; ci++)
            {
                var match = rows.FirstOrDefault(r =>
                    r.Capacity == capacities[ci] && r.StrategyName == strategies[si]);

                if (match is null)
                {
                    continue;
                }

                // Within group ci, strategies are arranged left-to-right.
                // The leftmost strategy sits at ci - groupWidth/2 + barWidth/2.
                var position = ci - groupWidth / 2 + barWidth / 2 + si * barWidth;

                bars.Add(new Bar
                {
                    Position = position,
                    Value = match.HitRate * 100,
                    FillColor = Palette.GetColor(si),
                    Size = barWidth * 0.9, // small gap between adjacent bars
                });
            }

            // Add bars as a single series per strategy so the legend shows one entry per algorithm.
            var barPlot = plot.Add.Bars(bars);
            barPlot.LegendText = strategies[si];
        }

        plot.Title($"Hit rate by strategy — {rows[0].WorkloadName}");
        plot.YLabel("Hit rate, %");
        plot.XLabel("Capacity");
        plot.Axes.SetLimitsY(0, 100);

        var tickPositions = Enumerable.Range(0, capacities.Count).Select(i => (double)i).ToArray();
        var tickLabels = capacities.Select(c => c.ToString()).ToArray();
        plot.Axes.Bottom.SetTicks(tickPositions, tickLabels);

        plot.ShowLegend(Alignment.UpperLeft);
        plot.SavePng(path, PlotWidthPx, PlotHeightPx);
    }

    private static void SaveHitRateVsCapacityChart(IReadOnlyList<BenchmarkResult> rows, string path)
    {
        var plot = new Plot();

        var strategies = rows.Select(r => r.StrategyName).Distinct().OrderBy(s => s).ToList();
        var capacities = rows.Select(r => r.Capacity).Distinct().OrderBy(c => c).ToList();

        for (var si = 0; si < strategies.Count; si++)
        {
            var strategyRows = rows
                .Where(r => r.StrategyName == strategies[si])
                .OrderBy(r => r.Capacity)
                .ToList();

            if (strategyRows.Count == 0)
            {
                continue;
            }

            var xs = strategyRows.Select(r => (double)r.Capacity).ToArray();
            var ys = strategyRows.Select(r => r.HitRate * 100).ToArray();

            var scatter = plot.Add.Scatter(xs, ys);
            scatter.LegendText = strategies[si];
            scatter.Color = Palette.GetColor(si);
            scatter.MarkerSize = 10;
            scatter.LineWidth = 2;
        }

        plot.Title($"Hit rate vs. capacity — {rows[0].WorkloadName}");
        plot.XLabel("Capacity");
        plot.YLabel("Hit rate, %");
        plot.Axes.SetLimitsY(0, 100);

        // Show ticks only at the actual measured capacities — no empty gaps in between.
        var tickValues = capacities.Select(c => (double)c).ToArray();
        var tickLabels = capacities.Select(c => c.ToString()).ToArray();
        plot.Axes.Bottom.SetTicks(tickValues, tickLabels);

        plot.ShowLegend(Alignment.LowerRight);
        plot.SavePng(path, PlotWidthPx, PlotHeightPx);
    }

    private static string MakeFileSafe(string name)
    {
        var invalid = Path.GetInvalidFileNameChars()
            .Concat(['(', ')', '=', ' ', ','])
            .ToHashSet();

        var chars = name.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        return new string(chars);
    }

    private static class Palette
    {
        private static readonly Color[] Colors =
        [
            new Color(31, 119, 180),
            new Color(255, 127, 14),
            new Color(44, 160, 44),
            new Color(214, 39, 40),
            new Color(148, 103, 189),
        ];

        public static Color GetColor(int index) => Colors[index % Colors.Length];
    }
}