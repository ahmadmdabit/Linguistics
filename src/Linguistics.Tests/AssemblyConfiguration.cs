// AssemblyConfiguration.cs
// This attribute enables parallel execution for the entire test assembly.
// ParallelScope.Fixtures means that different [TestFixture] classes can run at the same time.
// This is the safest and most common level of parallelism.
[assembly: Parallelizable(ParallelScope.Fixtures)]
