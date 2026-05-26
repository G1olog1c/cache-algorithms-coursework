using CacheAlgorithms.Core;
using CacheAlgorithms.Benchmark.Reporting;
using CacheAlgorithms.Benchmark.Workload;

// Smoke test: a single workload, all strategies, one capacity.
// Will be replaced with the full experiment driver later.
const int seed = 42;
const int keySpace = 1000;
const int requests = 10_000;
const int capacity = 100;

var workload = new ZipfianWorkload(keySpace, skew: 1.0, seed);

var results = new List<BenchmarkResult>
{
    BenchmarkRunner.Run(new FifoCache<int, int>(capacity), workload, requests),
    BenchmarkRunner.Run(new LruCache<int, int>(capacity), workload, requests),
    BenchmarkRunner.Run(new LfuCache<int, int>(capacity), workload, requests),
    BenchmarkRunner.Run(new MruCache<int, int>(capacity), workload, requests),
};

ConsoleReporter.Print(results);