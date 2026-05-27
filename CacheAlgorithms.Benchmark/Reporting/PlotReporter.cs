using ScottPlot;

namespace CacheAlgorithms.Benchmark.Reporting;

/// <summary>
/// Generates PNG charts from benchmark results using ScottPlot.
/// Produces two views per workload:
///   1. Bar chart of hit rate by strategy at each capacity.
///   2. Line chart of hit rate versus capacity, one line per strategy.
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

        // Group bars by capacity, within each group one bar per strategy.
        // This produces clustered bars that read naturally as "at capacity X,
        // here's how each strategy compared".
        var capacities = rows.Select(r => r.Capacity).Distinct().OrderBy(c => c).ToList();
        var strategies = rows.Select(r => r.StrategyName).Distinct().OrderBy(s => s).ToList();

        var bars = new List<Bar>();
        for (var ci = 0; ci < capacities.Count; ci++)
        {
            for (var si = 0; si < strategies.Count; si++)
            {
                var match = rows.FirstOrDefault(r =>
                    r.Capacity == capacities[ci] && r.StrategyName == strategies[si]);

                if (match is null)
                {
                    continue;
                }

                // Position bars within each capacity group:
                // group center is ci, strategies fan out by a small offset.
                var position = ci + (si - (strategies.Count - 1) / 2.0) * 0.18;

                bars.Add(new Bar
                {
                    Position = position,
                    Value = match.HitRate * 100, // percent for readability
                    FillColor = Palette.GetColor(si),
                    Label = strategies[si],
                });
            }
        }

        plot.Add.Bars(bars);

        plot.Title($"Hit rate by strategy — {rows[0].WorkloadName}");
        plot.YLabel("Hit rate, %");
        plot.XLabel("Capacity");
        plot.Axes.SetLimitsY(0, 100);

        // Show capacity labels under each group of bars.
        var tickPositions = Enumerable.Range(0, capacities.Count).Select(i => (double)i).ToArray();
        var tickLabels = capacities.Select(c => c.ToString()).ToArray();
        plot.Axes.Bottom.SetTicks(tickPositions, tickLabels);

        plot.ShowLegend();
        plot.SavePng(path, PlotWidthPx, PlotHeightPx);
    }

    private static void SaveHitRateVsCapacityChart(IReadOnlyList<BenchmarkResult> rows, string path)
    {
        var plot = new Plot();

        var strategies = rows.Select(r => r.StrategyName).Distinct().OrderBy(s => s).ToList();

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
            scatter.MarkerSize = 8;
            scatter.LineWidth = 2;
        }

        plot.Title($"Hit rate vs. capacity — {rows[0].WorkloadName}");
        plot.XLabel("Capacity");
        plot.YLabel("Hit rate, %");
        plot.Axes.SetLimitsY(0, 100);

        plot.ShowLegend();
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

    /// <summary>
    /// Stable color palette: every strategy gets the same color across all plots,
    /// so the reader can recognize "blue line" as the same algorithm in every chart.
    /// </summary>
    private static class Palette
    {
        private static readonly Color[] Colors =
        [
            new Color(31, 119, 180),   // blue
            new Color(255, 127, 14),   // orange
            new Color(44, 160, 44),    // green
            new Color(214, 39, 40),    // red
            new Color(148, 103, 189),  // purple
        ];

        public static Color GetColor(int index) => Colors[index % Colors.Length];
    }
}