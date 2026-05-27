using System.Globalization;
using System.Text;

namespace CacheAlgorithms.Benchmark.Reporting;

/// <summary>
/// Writes benchmark results to a CSV file using invariant culture
/// (decimal point, not comma) so the output is portable across locales
/// and parseable by Excel, LibreOffice, pandas, R, etc.
/// </summary>
public static class CsvReporter
{
    public static void Save(IReadOnlyList<BenchmarkResult> results, string filePath)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var builder = new StringBuilder();
        builder.AppendLine(
            "Workload,Strategy,Capacity,KeySpaceSize,RequestCount,"
            + "Hits,Misses,Evictions,HitRate,MissRate,AvgNanoseconds,TotalMilliseconds");

        foreach (var r in results)
        {
            // Invariant culture prevents locale-dependent decimal separators —
            // e.g. on a Ukrainian system, "0,5" would otherwise appear instead of "0.5".
            builder.AppendLine(string.Join(',',
                Escape(r.WorkloadName),
                Escape(r.StrategyName),
                r.Capacity.ToString(CultureInfo.InvariantCulture),
                r.KeySpaceSize.ToString(CultureInfo.InvariantCulture),
                r.RequestCount.ToString(CultureInfo.InvariantCulture),
                r.Hits.ToString(CultureInfo.InvariantCulture),
                r.Misses.ToString(CultureInfo.InvariantCulture),
                r.Evictions.ToString(CultureInfo.InvariantCulture),
                r.HitRate.ToString("F4", CultureInfo.InvariantCulture),
                r.MissRate.ToString("F4", CultureInfo.InvariantCulture),
                r.AverageNanosecondsPerRequest.ToString("F2", CultureInfo.InvariantCulture),
                r.TotalDuration.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture)));
        }

        File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// Wraps a field in quotes if it contains characters that would break CSV parsing
    /// (comma, quote, newline). Doubles any inner quotes per RFC 4180.
    /// </summary>
    private static string Escape(string field)
    {
        if (field.IndexOfAny([',', '"', '\n', '\r']) < 0)
        {
            return field;
        }

        return "\"" + field.Replace("\"", "\"\"") + "\"";
    }
}