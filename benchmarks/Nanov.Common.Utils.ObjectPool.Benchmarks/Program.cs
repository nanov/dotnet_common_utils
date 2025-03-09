#define ALL
#define HIGH_CONTENTION
#define SINGLE_THREADED

using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

namespace Nanov.Common.Utils.ObjectPool.Benchmarks {
	// A simple test class that we'll allocate/pool
	public class TestObject {
		public const int BufferSize = 1024;
		public int Id { get; set; }

		// We'll use a char array instead of a string to avoid allocations
		private readonly char[] _nameBuffer;
		public int NameLength { get; set; }
		public byte[] Buffer { get; private set; }

		public TestObject() {
			_nameBuffer = new char[32]; // Fixed size buffer for name
			Buffer = new byte[BufferSize]; // 1KB buffer to make allocation more noticeable
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetName(int value) {
			// Convert the value to chars and store in the buffer without allocating a string

		}
	}

	// Our object pool strategy
	public struct TestObjectStrategy : IObjectPoolStrategy<TestObject, int> {
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TestObject Create(int param) {
			var obj = new TestObject { Id = param };
			return obj;
		}

		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Prepare(TestObject value, int param) {
			value.Id = param;
		}

		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clean(TestObject value) {
			// value.Id = -1;
			// value.NameLength = 0;
			// Array.Clear(value.Buffer, 0, value.Buffer.Length);
		}
	}

	public class Program {
		public static void Main(string[] args) {
			var summary = BenchmarkRunner.Run<ObjectPoolBenchmarks>();
			Console.WriteLine(summary);
		}
	}

	[MemoryDiagnoser]
	[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
	[CategoriesColumn]
	[Config(typeof(BenchmarkConfig))]
	public class ObjectPoolBenchmarks {
		private ILogger _logger = new ConsoleLogger();
		private int _highContentionThreadCount = Process.GetCurrentProcess().Threads.Count * 2;
		private const int ThreadCount = 8;
		private class BenchmarkConfig : ManualConfig {
			public BenchmarkConfig() {
				AddJob(Job.ShortRun.WithIterationCount(3)); // Using ShortRun for quicker results
			}
		}

		// [Params(10, 100, 1000)]
		[Params(10, 100, 1000)] public int OperationsPerInvoke { get; set; }
		private ConcurrentObjectPool<TestObject, TestObjectStrategy, int> _concurrentObjectPool;
		private ObjectPool<TestObject, TestObjectStrategy, int> _objectPool;
		private readonly TestObjectStrategy _strategy = new TestObjectStrategy();

		[GlobalSetup]
		public void IterationSetup() {
			const int poolSize = 1000;
			_concurrentObjectPool = new ConcurrentObjectPool<TestObject, TestObjectStrategy, int>(_strategy, poolSize);
			_objectPool = new ObjectPool<TestObject, TestObjectStrategy, int>(_strategy, poolSize);
			for (var i = 0; i < poolSize; i++) {
				_concurrentObjectPool.Return(_strategy.Create(i));
				_objectPool.Return(_strategy.Create(i));
			}
		}
#if SINGLE_THREADED
		[Benchmark(Description = "DirectAllocation", Baseline = true)]
		[BenchmarkCategory("SingleThreaded")]
		public void DirectAllocation_SingleThreaded() {
			for (var i = 0; i < OperationsPerInvoke; i++) {
				var obj = new TestObject { Id = i };
				obj.SetName(i);
				// Use the object to prevent dead code elimination
				if (obj.Id < 0) throw new Exception("Should never happen");
			}
		}

		[Benchmark(Description = "ObjectPool")]
		[BenchmarkCategory("SingleThreaded")]
		public void ObjectPool_SingleThreaded() {
			for (var i = 0; i < OperationsPerInvoke; i++) {
				using var _ = _objectPool.RentValue(i, out var obj);
				// Use the object to prevent dead code elimination
				if (obj.Id < 0) throw new Exception("Should never happen");
			}
		}
#endif
#if ALL

		[Benchmark(Description="DirectAllocation", Baseline = true)]
		[BenchmarkCategory("MultiThreaded")]
		public void DirectAllocation_MultiThreaded() {

			Parallel.For(0, ThreadCount, threadId => {
				var iterationsPerThread = OperationsPerInvoke;
				for (var i = 0; i < iterationsPerThread; i++) {
					var id = threadId * iterationsPerThread + i;
					var obj = new TestObject { Id = id };
					obj.SetName(id);
					// Use the object to prevent dead code elimination
					if (obj.Id < 0) throw new Exception("Should never happen");
				}
			});
		}

		[Benchmark(Description = "ConcurrentObjectPool")]
		[BenchmarkCategory("MultiThreaded")]
		public void ConcurrentObjectPool_MultiThreaded() {

			Parallel.For(0, ThreadCount, threadId => {
				var iterationsPerThread = OperationsPerInvoke;
				for (var i = 0; i < iterationsPerThread; i++) {
					using var _ = _concurrentObjectPool.RentValue(threadId * iterationsPerThread + i, out var obj);
					// Use the object to prevent dead code elimination
					if (obj.Id < 0) throw new Exception("Should never happen");
				}
			});
		}

		// More realistic scenario with work simulation
		[Benchmark(Description = "DirectAllocation", Baseline = true)]
		[BenchmarkCategory("RealWorld")]
		public void DirectAllocation_WithWork() {
			var iterations = OperationsPerInvoke;
			for (var i = 0; i < iterations; i++) {
				var obj = new TestObject { Id = i };
				obj.SetName(i);
				// Simulate some work with the object
				SimulateWork(obj);
			}
		}

		[Benchmark(Description = "ObjectPool")]
		[BenchmarkCategory("RealWorld")]
		public void ObjectPool_WithWork() {
			var iterations = OperationsPerInvoke;
			for (var i = 0; i < iterations; i++) {
				using var _ = _objectPool.RentValue(i, out var obj);
				// Simulate some work with the object
				SimulateWork(obj);
			}
		}

		[Benchmark(Description = "DirectAllocation", Baseline = true)]
		[BenchmarkCategory("RealWorldParallel")]
		public void DirectAllocation_WithWorkParallel() {

			Parallel.For(0, ThreadCount, threadId => {
				var iterationsPerThread = OperationsPerInvoke;
				for (var i = 0; i < iterationsPerThread; i++) {
					var id = threadId * iterationsPerThread + i;
					var obj = new TestObject { Id = id };
					obj.SetName(id);
					SimulateWork(obj);
				}
			});
		}

		[Benchmark(Description = "ConcurrentObjectPool")]
		[BenchmarkCategory("RealWorldParallel")]
		public void ConcurrentObjectPool_WithWorkParallel() {
			Parallel.For(0, ThreadCount, threadId => {
				var iterationsPerThread = OperationsPerInvoke;
				for (var i = 0; i < iterationsPerThread; i++) {
					using var _ = _concurrentObjectPool.RentValue(threadId * iterationsPerThread + i, out var obj);
					SimulateWork(obj);
				}
			});
		}
#endif
#if HIGH_CONTENTION
		// Add contention benchmarks to see how the pool performs under high contention
		[Benchmark(Description = "DirectAllocation", Baseline = true)]
		[BenchmarkCategory("HighContention")]
		public void DirectAllocation_HighContention() {

			Parallel.For(0, _highContentionThreadCount, _ => {
				var iterationsPerThread = OperationsPerInvoke;
				for (var i = 0; i < iterationsPerThread; i++) {
					// All threads allocate objects with the same ID to simulate high contention
					var obj = new TestObject { Id = 1 };
					obj.SetName(1);
					if (obj.Id < 0) throw new Exception("Should never happen");
				}
			});
		}

		[Benchmark(Description = "ConcurrentObjectPool")]
		[BenchmarkCategory("HighContention")]
		public void ConcurrentObjectPool_HighContention() {
			Parallel.For(0, _highContentionThreadCount, t => {
				var len = OperationsPerInvoke;
				for (var i = 0; i < len; i++) {
					// All threads request objects with the same ID to simulate high contention
					using var _ = _concurrentObjectPool.RentValue(1, out var obj);
					if  (obj.Id < 0) throw new Exception("Should never happen");
				}
			});
		}
#endif

		private void SimulateWork(TestObject obj) {
			// Simulate some CPU-bound work
			for (var i = 0; i < TestObject.BufferSize; i += 64)
				obj.Buffer[i] = (byte)(obj.Id & 0xFF);

			// Use NameLength to prevent dead code elimination
			obj.Buffer[0] = (byte)(obj.NameLength & 0xFF);

			// Small delay to simulate some work
			SpinWait.SpinUntil(() => false, 3);
		}
	}
}