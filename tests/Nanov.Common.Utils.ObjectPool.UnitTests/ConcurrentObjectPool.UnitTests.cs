using System.Collections.Concurrent;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Nanov.Common.Utils.ObjectPool.UnitTests;

// Test implementation of the IObjectPoolStrategy

[TestFixture]
public class ConcurrentObjectPoolTests {
	[Test]
	public void CreatePool_WithValidCapacity_InitializesCorrectly() {
		// Arrange & Act
		var pool = new ConcurrentObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), 10);

		// Assert
		ClassicAssert.AreEqual(0u, pool.Count);
	}

	[Test]
	public void Rent_FromEmptyPool_CreatesNewObject() {
		// Arrange
		var pool = new ConcurrentObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), 10);

		// Act
		var obj = pool.Rent("test");

		// Assert
		ClassicAssert.IsNotNull(obj);
		ClassicAssert.AreEqual("test", obj.Data);
		ClassicAssert.AreEqual(0u, pool.Count);
	}

	[Test]
	public void Return_ToEmptyPool_AddsObjectToPool() {
		// Arrange
		var pool = new ConcurrentObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), 10);
		var obj = pool.Rent("test");

		// Act
		pool.Return(obj);

		// Assert
		ClassicAssert.AreEqual(1u, pool.Count);
		ClassicAssert.IsNull(obj.Data); // Object should be cleaned
	}

	[Test]
	public void RentAfterReturn_ReusesObject() {
		// Arrange
		var pool = new ConcurrentObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), 10);
		var obj1 = pool.Rent("test1");
		var id1 = obj1.Id;
		pool.Return(obj1);

		// Act
		var obj2 = pool.Rent("test2");

		// Assert
		ClassicAssert.AreEqual(id1, obj2.Id); // Same object
		ClassicAssert.AreEqual("test2", obj2.Data); // Data updated
		ClassicAssert.AreEqual(1, obj2.PrepareCount); // Prepare was called
		ClassicAssert.AreEqual(1, obj2.CleanCount); // Clean was called when returned
	}

	[Test]
	public void RentValue_CreatesRentedValue() {
		// Arrange
		var pool = new ConcurrentObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), 10);

		// Act
		using (var rentedValue = pool.RentValue("test", out var obj)) {
			// Assert
			ClassicAssert.IsNotNull(obj);
			ClassicAssert.AreEqual("test", obj.Data);
		} // Implicitly returns to pool here

		// After dispose, object should be back in the pool
		ClassicAssert.AreEqual(1u, pool.Count);
	}

	[Test]
	public void Return_MultipleTimes_FillsPoolUpToCapacity() {
		// Arrange
		uint capacity = 5;
		var pool = new ConcurrentObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), capacity);
		var objects = new List<TestObject>();

		// Create objects beyond capacity
		for (int i = 0; i < capacity * 2; i++) {
			objects.Add(pool.Rent($"test{i}"));
		}

		// Act - return all objects
		foreach (var obj in objects) {
			pool.Return(obj);
		}

		// Assert - pool should be filled up to capacity
		ClassicAssert.AreEqual(capacity, pool.Count);
	}

	[Test]
	public void MultiThreaded_RentAndReturn_WorksCorrectly() {
		// Arrange
		int threadCount = 8;
		int operationsPerThread = 1000;
		uint poolCapacity = 20;
		var pool = new ConcurrentObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), poolCapacity);
		var allObjects = new ConcurrentBag<TestObject>();

		// Act
		Parallel.For(0, threadCount, threadId => {
			for (int i = 0; i < operationsPerThread; i++) {
				var obj = pool.Rent($"thread{threadId}-op{i}");
				// Simulate work
				Thread.Sleep(1);
				// Store unique objects for verification
				if (!allObjects.Contains(obj)) {
					allObjects.Add(obj);
				}

				pool.Return(obj);
			}
		});

		// Assert
		ClassicAssert.IsTrue(pool.Count > 0);
		ClassicAssert.IsTrue(pool.Count <= poolCapacity);

		// Should have created far fewer objects than operations due to reuse
		ClassicAssert.IsTrue(allObjects.Count < threadCount * operationsPerThread);
	}

	[Test]
	public void Pool_WithSmallCapacity_HandlesOverflow() {
		// Arrange
		uint capacity = 3;
		var pool = new ConcurrentObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), capacity);
		var objects = new List<TestObject>();

		// Act
		for (int i = 0; i < 10; i++) {
			var obj = pool.Rent($"test{i}");
			objects.Add(obj);
		}

		// Return all objects
		foreach (var obj in objects) {
			pool.Return(obj);
		}

		// Assert
		ClassicAssert.AreEqual(capacity, pool.Count);

		// Check all objects can be reused
		var reusedObjects = new List<TestObject>();
		for (int i = 0; i < capacity; i++) {
			reusedObjects.Add(pool.Rent($"reused{i}"));
		}

		// Verify pool is now empty
		ClassicAssert.AreEqual(0u, pool.Count);

		// Verify reused objects are from the original set
		foreach (var reused in reusedObjects) {
			ClassicAssert.IsTrue(objects.Exists(obj => obj.Id == reused.Id));
		}
	}
}