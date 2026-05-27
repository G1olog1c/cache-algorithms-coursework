using CacheAlgorithms.Core;
using CacheAlgorithms.Benchmark.Reporting;
using CacheAlgorithms.Benchmark.Workload;

const int seed = 42;
const int keySpaceSize = 1000;
const int requestsPerRun = 10_000;

var capacities = new[] { 50, 100, 500 };

// Build each workload's reference sequence once and reuse it across all
// strategies and capacities. This way every strategy sees the EXACT same
// requests, so the only varying factor is the eviction policy itself.
var workloads = new IWorkloadGenerator[]
{
    new UniformWorkload(keySpaceSize, seed),
    new ZipfianWorkload(keySpaceSize, skew: 1.0, seed),
};

var results = new List<BenchmarkResult>();

foreach (var workload in workloads)
{
    // Materialise the request sequence once per workload.
    // Same sequence will be replayed for every (capacity × strategy) combination.
    var requests = workload.Generate(requestsPerRun).ToArray();

    foreach (var capacity in capacities)
    {
        ICacheStrategy<int, int>[] strategies =
        [
            new FifoCache<int, int>(capacity),
            new LruCache<int, int>(capacity),
            new LfuCache<int, int>(capacity),
            new MruCache<int, int>(capacity),
        ];

        foreach (var strategy in strategies)
        {
            var result = BenchmarkRunner.Run(strategy, workload, requests);
            results.Add(result);
            Console.WriteLine(
                $"  ✓ {workload.Name,-22} cap={capacity,4}  {strategy.Name,-6}  "
                + $"hit={result.HitRate:P2}");
        }
    }
}

Console.WriteLine();
Console.WriteLine("=== Console report ===");
ConsoleReporter.Print(results);

var outputDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "results");
outputDir = Path.GetFullPath(outputDir);

var csvPath = Path.Combine(outputDir, "benchmark_results.csv");
CsvReporter.Save(results, csvPath);
Console.WriteLine();
Console.WriteLine($"CSV saved: {csvPath}");

PlotReporter.Save(results, outputDir);
Console.WriteLine($"PNG charts saved to: {outputDir}");