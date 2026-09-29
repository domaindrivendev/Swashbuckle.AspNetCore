---
name: performance-benchmark
description: Write and run ad hoc BenchmarkDotNet benchmarks to validate the performance impact of a code change in Swashbuckle.AspNetCore. Use this when asked to benchmark, profile, or validate the performance of a change.
---

# Ad Hoc Performance Benchmarking

When you need to validate the performance impact of a code change, follow this process to write a
[BenchmarkDotNet](https://benchmarkdotnet.org/) benchmark and compare the baseline and changed code locally.

Benchmarks live in the `perf/Swashbuckle.AspNetCore.Benchmarks` project, which references the `src/` projects directly,
and are run with the [`benchmark.ps1`](../../../benchmark.ps1) script at the repository root. Benchmarks are not run as
part of CI, so any performance claim in a PR must be backed by results you produced locally.

## Step 1: Write the Benchmark

Look at the existing benchmarks in `perf/Swashbuckle.AspNetCore.Benchmarks` first and extend one if it already covers
the code being changed. Otherwise, add a new benchmark class to that project that exercises the specific operation
being changed.

### Project notes

- The project uses `BenchmarkSwitcher`, so every public benchmark class in the assembly is discovered automatically.
- `Swashbuckle.AspNetCore.SwaggerGen` grants `InternalsVisibleTo` to the benchmarks project, so internal types (such
  as `XmlCommentsDocumentHelper`) can be benchmarked directly.
- Test fixtures (fake controllers, annotated types, `ApiDescriptionFactory`, etc.) are shared with the test projects by
  linking the source files with `<Compile Include="..." Link="..." />` in the project file. Reuse fixtures this way
  rather than duplicating them, and do not add new package references to the project.
- The project must compile without warnings, as `TreatWarningsAsErrors` is enabled (see `AGENTS.md`).

### Best practices

For comprehensive guidance, see the
[Microbenchmark Design Guidelines](https://github.com/dotnet/performance/blob/main/docs/microbenchmark-design-guidelines.md).

- **Move initialization to `[GlobalSetup]`**: Build options, resolvers, fixtures, and input data outside of the
  measured code.
- **Watch for state that accumulates across invocations.** Generators and filters often mutate their inputs (for
  example a `SchemaRepository`, `OpenApiDocument`, or `OpenApiOperation`). If state carried over between invocations
  would change what is measured (e.g. a schema already existing in the repository causes a `$ref` short-circuit),
  create fresh inputs inside the benchmark method, or reset them in `[IterationSetup]` only when the operation is long
  enough for that to be accurate.
- **Return values** from benchmark methods to prevent dead code elimination.
- **Avoid manual loops**: BenchmarkDotNet invokes the benchmark many times automatically.
- **Use `[MemoryDiagnoser]`**: Allocation reductions are a common goal of performance changes in this repository.
- **Use realistic inputs**: Prefer representative models and APIs (nested types, collections, enums, nullable members,
  many endpoints) over trivial ones, and use `[Params]` to show how the change scales.
- **Benchmark class requirements**: Must be `public`, not `sealed`, not `static`, and must be a `class`.
- **Processor affinity**: If the benchmark environment's processors include both performance and efficiency cores (e.g., on Apple M1/M2 or Intel hybrid architectures), consider pinning the benchmark to the performance cores to reduce noise. This can be achieved using the `--affinity` option in BenchmarkDotNet to specify an affinity mask to set for the benchmark process.

### Example: Schema generation benchmark

```csharp
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Benchmarks;

[MemoryDiagnoser]
public class SchemaGeneratorBenchmark
{
    private SchemaGenerator _generator;

    [GlobalSetup]
    public void Setup()
    {
        var generatorOptions = new SchemaGeneratorOptions();
        var serializerOptions = new JsonSerializerOptions();
        var resolver = new JsonSerializerDataContractResolver(serializerOptions, generatorOptions);

        _generator = new SchemaGenerator(generatorOptions, resolver);
    }

    [Benchmark]
    public IOpenApiSchema GenerateSchema()
    {
        // Use a new repository each time so the schema is generated rather than returned as a cached $ref
        var repository = new SchemaRepository();
        return _generator.GenerateSchema(typeof(Order), repository);
    }

    public class Order
    {
        public int Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderLine> Lines { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class OrderLine
    {
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public decimal? Discount { get; set; }
    }

    public enum OrderStatus
    {
        Pending,
        Shipped,
    }
}
```

For end-to-end scenarios (for example the cost of generating a full document for an application), prefer building the
generator from real application services in `[GlobalSetup]` using one of the sample applications under
`test/WebSites/`, rather than hand-constructing many `ApiDescription` instances.

## Step 2: Prepare the Baseline

At this point the change is typically already present in the working tree. Do not stash, revert, or otherwise discard
the user's changes to create the baseline. Instead, use a separate [git worktree](https://git-scm.com/docs/git-worktree)
for the baseline:

```powershell
git worktree add ../Swashbuckle.AspNetCore-baseline <base-commit>
```

Where `<base-commit>` is the commit the change is based on (for example `master`, or the output of
`git merge-base HEAD origin/master`).

If the benchmark is new or has been modified, copy it (and any project file changes it needs) into the baseline
worktree so that exactly the same benchmark code runs against both versions. If the benchmark depends on new APIs
introduced by the change, it cannot run against the baseline as-is; write the benchmark against APIs that exist in
both versions instead.

## Step 3: Run the Benchmark

Run the benchmark in both the baseline worktree and the working tree, on the same machine, one after the other, with
as little other activity on the machine as possible.

To run **all** benchmarks, use the script in each tree:

```powershell
./benchmark.ps1
```

`benchmark.ps1` always runs every benchmark (`--filter *`) by default. To run a single benchmark class while iterating, invoke
the project directly with a filter:

```powershell
dotnet run --project perf/Swashbuckle.AspNetCore.Benchmarks --configuration Release --framework net10.0 -- --filter "*SchemaGeneratorBenchmark*"
```

Useful BenchmarkDotNet arguments (see the
[console arguments documentation](https://benchmarkdotnet.org/articles/guides/console-args.html)):

- `--job short` for a quick, less precise run while iterating (also available as `./benchmark.ps1 -Job short`). Use the
  default job for results you report.
- `--artifacts <path>` to keep the results for the baseline and the change in separate directories.
- `--memory` to enable the memory diagnoser without changing the benchmark code.

When finished, remove the baseline worktree with `git worktree remove ../Swashbuckle.AspNetCore-baseline`.

## Step 4: Report the Results

Report the results as the BenchmarkDotNet Markdown summary tables for both the baseline and the change, including:

- The machine and runtime details from the summary header (OS, CPU, .NET SDK and runtime versions).
- Both the `Mean` and `Allocated` columns, and the ratio or percentage change between baseline and change.
- Any regressions, even if the change is a net improvement overall, with an explanation of why they occur.

Differences within the reported error margins, or of a few percent in either direction, should be described as noise
rather than as an improvement or regression.

Do not commit ad hoc benchmarks unless the user asks for them to be kept, in which case they should be added to the
`perf/Swashbuckle.AspNetCore.Benchmarks` project following the conventions of the existing benchmarks.

> 📝 **AI-generated content disclosure:** When posting benchmark results to GitHub under a user's credentials — i.e.
> the account is **not** a dedicated bot account or app — you **MUST** include a concise, visible note (e.g. a
> `> [!NOTE]` alert) at the bottom of the content stating which agent generated it. Skip this only if the user
> explicitly asks you to omit it.

## Additional Resources

- [Microbenchmark Design Guidelines](https://github.com/dotnet/performance/blob/main/docs/microbenchmark-design-guidelines.md)
- [BenchmarkDotNet console arguments](https://benchmarkdotnet.org/articles/guides/console-args.html)
