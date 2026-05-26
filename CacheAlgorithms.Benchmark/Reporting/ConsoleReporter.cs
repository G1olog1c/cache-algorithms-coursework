namespace CacheAlgorithms.Benchmark.Reporting;

/// <summary>
/// Prints benchmark results to the console as a formatted table.
/// Groups rows by workload so each section reads as an independent experiment.
/// </summary>
public static class ConsoleReporter
{
    // Column widths chosen to fit a typical 120-column terminal without wrapping.
    // Adjust here if you add new columns or rename strategies to longer labels.
    private const int StrategyWidth = 18;
    private const int CapacityWidth = 10;
    private const int HitRateWidth = 12;
    private const int MissRateWidth = 12;
    private const int EvictionsWidth = 12;
    private const int AvgNsWidth = 14;
    private const int TotalMsWidth = 12;

    public static void Print(IReadOnlyList<BenchmarkResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        if (results.Count == 0)
        {
            Console.WriteLine("No benchmark results to display.");
            return;
        }

        // Preserve the order in which workloads first appeared in the input,
        // so the console output mirrors the order the runner produced.
        var workloadOrder = new Dictionary<string, int>();
        foreach (var result in results)
        {
            if (!workloadOrder.ContainsKey(result.WorkloadName))
            {
                workloadOrder[result.WorkloadName] = workloadOrder.Count;
            }
        }

        var grouped = results
            .GroupBy(r => r.WorkloadName)
            .OrderBy(g => workloadOrder[g.Key]);

        foreach (var workloadGroup in grouped)
        {
            PrintSection(workloadGroup.Key, workloadGroup.ToList());
        }
    }

    private static void PrintSection(string workloadName, IReadOnlyList<BenchmarkResult> rows)
    {
        var first = rows[0];

        Console.WriteLine();
        Console.WriteLine($"Workload: {workloadName}");
        Console.WriteLine($"Key space: {first.KeySpaceSize}, Requests per run: {first.RequestCount}");
        Console.WriteLine(new string('─', TotalLineWidth()));

        PrintHeader();
        Console.WriteLine(new string('─', TotalLineWidth()));

        // Sort rows so the table reads naturally: by capacity, then by strategy name.
        // This makes it easy to compare strategies side by side at a fixed capacity.
        var ordered = rows.OrderBy(r => r.Capacity).ThenBy(r => r.StrategyName);
        foreach (var row in ordered)
        {
            PrintRow(row);
        }

        Console.WriteLine(new string('─', TotalLineWidth()));
    }

    private static void PrintHeader()
    {
        Console.Write("Strategy".PadRight(StrategyWidth));
        Console.Write("Capacity".PadLeft(CapacityWidth));
        Console.Write("Hit rate".PadLeft(HitRateWidth));
        Console.Write("Miss rate".PadLeft(MissRateWidth));
        Console.Write("Evictions".PadLeft(EvictionsWidth));
        Console.Write("Avg (ns)".PadLeft(AvgNsWidth));
        Console.Write("Total (ms)".PadLeft(TotalMsWidth));
        Console.WriteLine();
    }

    private static void PrintRow(BenchmarkResult result)
    {
        Console.Write(result.StrategyName.PadRight(StrategyWidth));
        Console.Write(result.Capacity.ToString().PadLeft(CapacityWidth));
        Console.Write($"{result.HitRate:P2}".PadLeft(HitRateWidth));
        Console.Write($"{result.MissRate:P2}".PadLeft(MissRateWidth));
        Console.Write(result.Evictions.ToString().PadLeft(EvictionsWidth));
        Console.Write($"{result.AverageNanosecondsPerRequest:F1}".PadLeft(AvgNsWidth));
        Console.Write($"{result.TotalDuration.TotalMilliseconds:F2}".PadLeft(TotalMsWidth));
        Console.WriteLine();
    }

    private static int TotalLineWidth() =>
        StrategyWidth + CapacityWidth + HitRateWidth + MissRateWidth
        + EvictionsWidth + AvgNsWidth + TotalMsWidth;
}