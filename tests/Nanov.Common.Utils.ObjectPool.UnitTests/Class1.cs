using Nanov.Common.Utils.ObjectPool.Internal;

namespace Nanov.Common.Utils.ObjectPool.UnitTests;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

// Mock implementation for testing
public class TestObject {
	public int Id { get; set; }
	public string? Data { get; set; }
	public bool IsCleaned { get; set; }
}

public struct TestObjectPoolStrategy : IObjectPoolStrategy<TestObject> {
	private int _counter = 0;

	public TestObjectPoolStrategy()
		=> _counter = 0;
	public TestObject Create<TParam>(TParam param) {
		if (param is int id) {
			return new TestObject { Id = id, IsCleaned = false };
		}
		else {
			return new TestObject { Id = Interlocked.Increment(ref _counter), IsCleaned = false };
		}
	}

	public void Prepare<TParam>(TestObject obj, TParam param) {
		if (param is string data) {
			obj.Data = data;
		}

		obj.IsCleaned = false;
	}

	public void Clean(TestObject obj) {
		obj.Data = null;
		obj.IsCleaned = true;
	}
}

[TestFixture]
public class ConcurrentLightStackTests {
	[Test]
	public void Constructor_WithCapacity_SetsCapacityCorrectly() {
		// Arrange & Act
		var stack = new ConcurrentLightStack<TestObject>(42);

		// Assert
		Assert.That(stack.Count, Is.EqualTo(0));
		// We can't directly test capacity as it's private, but we can push elements up to capacity
		for (int i = 0; i < 42; i++) {
			Assert.That(stack.TryPush(new TestObject()), Is.True);
		}

		Assert.That(stack.TryPush(new TestObject()), Is.False); // This should fail
	}

	[Test]
	public void TryPush_WhenStackIsEmpty_ReturnsTrue() {
		// Arrange
		var stack = new ConcurrentLightStack<TestObject>(10);
		var obj = new TestObject { Id = 1 };

		// Act
		var result = stack.TryPush(obj);

		// Assert
		Assert.That(result, Is.True);
		Assert.That(stack.Count, Is.EqualTo(1));
	}

	[Test]
	public void TryPush_WhenStackIsFull_ReturnsFalse() {
		// Arrange
		var stack = new ConcurrentLightStack<TestObject>(1);
		stack.TryPush(new TestObject { Id = 1 });

		// Act
		var result = stack.TryPush(new TestObject { Id = 2 });

		// Assert
		Assert.That(result, Is.False);
		Assert.That(stack.Count, Is.EqualTo(1));
	}

	[Test]
	public void TryPop_WhenStackHasItems_ReturnsItemAndTrue() {
		// Arrange
		var stack = new ConcurrentLightStack<TestObject>(10);
		var obj = new TestObject { Id = 42 };
		stack.TryPush(obj);

		// Act
		var success = stack.TryPop(out var result);

		// Assert
		Assert.That(success, Is.True);
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Id, Is.EqualTo(42));
		Assert.That(stack.Count, Is.EqualTo(0));
	}

	[Test]
	public void TryPop_WhenStackIsEmpty_ReturnsFalse() {
		// Arrange
		var stack = new ConcurrentLightStack<TestObject>(10);

		// Act
		var success = stack.TryPop(out var result);

		// Assert
		Assert.That(success, Is.False);
		Assert.That(result, Is.Null);
	}

	[Test]
	public void PushThenPop_MultipleItems_WorksCorrectly() {
		// Arrange
		var stack = new ConcurrentLightStack<TestObject>(10);
		var obj1 = new TestObject { Id = 1 };
		var obj2 = new TestObject { Id = 2 };
		var obj3 = new TestObject { Id = 3 };

		// Act & Assert
		Assert.That(stack.TryPush(obj1), Is.True);
		Assert.That(stack.TryPush(obj2), Is.True);
		Assert.That(stack.TryPush(obj3), Is.True);
		Assert.That(stack.Count, Is.EqualTo(3));

		Assert.That(stack.TryPop(out var result3), Is.True);
		Assert.That(result3!.Id, Is.EqualTo(3)); // LIFO order

		Assert.That(stack.TryPop(out var result2), Is.True);
		Assert.That(result2!.Id, Is.EqualTo(2));

		Assert.That(stack.TryPop(out var result1), Is.True);
		Assert.That(result1!.Id, Is.EqualTo(1));

		Assert.That(stack.Count, Is.EqualTo(0));
		Assert.That(stack.TryPop(out _), Is.False);
	}

	[Test]
	public void ConcurrentPushAndPop_ThreadSafety() {
		// Arrange
		const int capacity = 10;
		const int operationsPerThread = 10000;
		const int pushThreads = 4;
		const int popThreads = 4;

		var stack = new ConcurrentLightStack<TestObject>(capacity);
		var countPushed = 0;
		var countPopped = 0;
		var allItemsPopped = new List<TestObject>();
		var lockObject = new Lock();
		var uniquieId = 0;

		// Act
		var pushTasks = Enumerable.Range(0, pushThreads)
			.Select(ti => Task.Run(() => {
				for (int i = 0; i < operationsPerThread; i++) {
					var obj = new TestObject { Id = Interlocked.Increment(ref uniquieId), IsCleaned = false };
					if (stack.TryPush(obj)) {
						Interlocked.Increment(ref countPushed);
					}

					// Small delay to increase chances of thread interleaving
					if (i % 100 == 0) Thread.Yield();
				}
			})).ToArray();

		var popTasks = Enumerable.Range(0, popThreads)
			.Select(ti => Task.Run(() => {
				var locallyPopped = new List<TestObject>();
				for (int i = 0; i < operationsPerThread; i++) {
					if (stack.TryPop(out var obj) && obj != null) {
						Interlocked.Increment(ref countPopped);
						locallyPopped.Add(obj);
					}

					// Small delay to increase chances of thread interleaving
					if (i % 100 == 0) Thread.Yield();
				}

				lock (lockObject) {
					allItemsPopped.AddRange(locallyPopped);
				}
			})).ToArray();

		// Wait for all tasks to complete
		Task.WaitAll(pushTasks.Concat(popTasks).ToArray());

		// Assert
		Console.WriteLine($"Items pushed: {countPushed}, Items popped: {countPopped}");
		Assert.That(countPopped, Is.LessThanOrEqualTo(countPushed));
		Assert.That(stack.Count, Is.EqualTo(countPushed - countPopped));

		// Check for duplicates in popped items (would indicate a thread safety issue)
		var duplicates = allItemsPopped
			.GroupBy(x => x.Id)
			.Where(g => g.Count() > 1)
			.Select(g => g.Key)
			.ToList();

		Assert.That(duplicates, Is.Empty, "There should be no duplicate objects popped");
	}
}

[TestFixture]
public class ConcurrentObjectPoolTests {
	[Test]
	public void Rent_WhenPoolIsEmpty_CreatesNewObject() {
		// Arrange
		var strategy = new TestObjectPoolStrategy();
		var pool = new ConcurrentObjectPool<TestObject, TestObjectPoolStrategy>(strategy, 10);

		// Act
		var obj = pool.Rent(42);

		// Assert
		Assert.That(obj, Is.Not.Null);
		Assert.That(obj.Id, Is.EqualTo(42));
		Assert.That(obj.IsCleaned, Is.False);
	}

	[Test]
	public void RentAndReturn_ReusesSameObject() {
		// Arrange
		var strategy = new TestObjectPoolStrategy();
		var pool = new ConcurrentObjectPool<TestObject, TestObjectPoolStrategy>(strategy, 10);
		var obj = pool.Rent(1);
		obj.Data = "test data";

		// Act
		pool.Return(obj);
		var rentedAgain = pool.Rent("new data");

		// Assert
		Assert.That(rentedAgain, Is.SameAs(obj), "Pool should return the same object instance");
		Assert.That(rentedAgain.Id, Is.EqualTo(1), "Object ID should be preserved");
		Assert.That(rentedAgain.Data, Is.EqualTo("new data"), "Object should be prepared with new data");
		Assert.That(rentedAgain.IsCleaned, Is.False, "Object should not be in cleaned state after renting");
	}

	[Test]
	public void Return_CleansObject() {
		// Arrange
		var strategy = new TestObjectPoolStrategy();
		var pool = new ConcurrentObjectPool<TestObject, TestObjectPoolStrategy>(strategy, 10);
		var obj = pool.Rent(1);
		obj.Data = "test data";
		obj.IsCleaned = false;

		// Act
		pool.Return(obj);

		// Assert
		Assert.That(obj.Data, Is.Null, "Object data should be cleaned");
		Assert.That(obj.IsCleaned, Is.True, "Object should be marked as cleaned");
	}

	[Test]
	public void Return_WhenPoolIsFull_ObjectIsDiscarded() {
		// Arrange
		var strategy = new TestObjectPoolStrategy();
		var pool = new ConcurrentObjectPool<TestObject, TestObjectPoolStrategy>(strategy, 1);

		var obj1 = pool.Rent(1);
		var obj2 = pool.Rent(2);

		// Return first object to fill the pool
		pool.Return(obj1);

		// Act - returning second object to a full pool
		pool.Return(obj2);

		// Rent two objects to see what's in the pool
		var rent1 = pool.Rent(3);
		var rent2 = pool.Rent(4);

		// Assert
		Assert.That(rent1, Is.SameAs(obj1), "First rented object should be the first returned one");
		Assert.That(rent2.Id, Is.EqualTo(4), "Second rented object should be newly created");
		Assert.That(rent2, Is.Not.SameAs(obj2), "Second rented object should not be the discarded one");
	}

	[Test]
	public void ConcurrentRentAndReturn_ThreadSafety() {
		// Arrange
		const int capacity = 100;
		const int operationsPerThread = 1000;
		const int numThreads = 8;

		var strategy = new TestObjectPoolStrategy();
		var pool = new ConcurrentObjectPool<TestObject, TestObjectPoolStrategy>(strategy, capacity);
		var countRented = 0;
		var countReturned = 0;
		var uniqueObjectIds = new HashSet<int>();
		var lockObject = new object();

		// Act
		var tasks = Enumerable.Range(0, numThreads)
			.Select(threadId => Task.Run(() => {
				var localRented = new List<TestObject>();
				var random = new Random(threadId);

				for (int i = 0; i < operationsPerThread; i++) {
					// 70% chance to rent, 30% chance to return
					if (random.NextDouble() < 0.7 || localRented.Count == 0) {
						var obj = pool.Rent(random.Next(1, 1000));
						Interlocked.Increment(ref countRented);
						localRented.Add(obj);

						lock (lockObject) {
							uniqueObjectIds.Add(obj.Id);
						}
					}
					else {
						int indexToReturn = random.Next(localRented.Count);
						var objToReturn = localRented[indexToReturn];
						localRented.RemoveAt(indexToReturn);

						pool.Return(objToReturn);
						Interlocked.Increment(ref countReturned);
					}

					// Small delay to increase chances of thread interleaving
					if (i % 50 == 0) Thread.Yield();
				}

				// Return all remaining rented objects
				foreach (var obj in localRented) {
					pool.Return(obj);
					Interlocked.Increment(ref countReturned);
				}
			})).ToArray();

		// Wait for all tasks to complete
		Task.WaitAll(tasks);

		// Assert
		Console.WriteLine($"Total objects rented: {countRented}, Total objects returned: {countReturned}");
		Assert.That(countReturned, Is.EqualTo(countRented), "All rented objects should be returned");

		// Additional check: rent all objects from the pool and verify they're all clean
		var allPooledObjects = new List<TestObject>();
		while (pool.Rent(0) is var obj && allPooledObjects.Count < capacity) {
			allPooledObjects.Add(obj);
		}

		Assert.That(allPooledObjects.All(o => o.IsCleaned == false),
			"All objects from the pool should have been prepared/cleaned correctly");
	}
}