using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Nanov.Common.Utils.ObjectPool.UnitTests;

[TestFixture]
public class ObjectPoolTests {
	[Test]
	public void CreatePool_WithValidCapacity_InitializesCorrectly() {
		// Arrange & Act
		var pool = new ObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), 10);

		// Assert
		ClassicAssert.AreEqual(0u, pool.Count);
	}

	[Test]
	public void Rent_FromEmptyPool_CreatesNewObject() {
		// Arrange
		var pool = new ObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), 10);

		// Act
		var obj = pool.Rent("test");

		// Assert
		ClassicAssert.IsNotNull(obj);
		ClassicAssert.AreEqual("test", obj.Data);
		ClassicAssert.AreEqual(0u, pool.Count);
	}

	[Test]
	public void Return_ToPool_AddsObjectToPool() {
		// Arrange
		var pool = new ObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), 10);
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
		var pool = new ObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), 10);
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
	public void RentValue_ReturnsCorrectObject() {
		// Arrange
		var pool = new ObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), 10);

		// Act
		using (pool.RentValue("test", out var obj)) {
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
		var pool = new ObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), capacity);
		var objects = new List<TestObject>();

		// Create objects beyond capacity
		for (var i = 0; i < capacity * 2; i++)
			objects.Add(pool.Rent($"test{i}"));

		// Act - return all objects
		foreach (var obj in objects)
			pool.Return(obj);

		// Assert - pool should be filled up to capacity
		ClassicAssert.AreEqual(capacity, pool.Count);
	}

	[Test]
	public void Rent_FromFilledPool_RemovesObjectFromPool() {
		// Arrange
		var pool = new ObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), 5);
		var objects = new List<TestObject>();

		// Fill pool
		for (var i = 0; i < 5; i++) {
			var obj = pool.Rent($"fill{i}");
			objects.Add(obj);
		}

		foreach (var obj in objects)
			pool.Return(obj);

		// Verify pool is filled
		ClassicAssert.AreEqual(5u, pool.Count);

		// Act
		var rentedObj = pool.Rent("rent-test");

		// Assert
		ClassicAssert.AreEqual(4u, pool.Count);
		ClassicAssert.AreEqual("rent-test", rentedObj.Data);
	}

	[Test]
	public void Rent_MultipleTimes_EmptiesPool() {
		// Arrange
		const uint capacity = 3;
		var pool = new ObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), capacity);
		var initialObjects = new List<TestObject>();

		// Fill pool
		for (var i = 0; i < capacity; i++) {
			var obj = pool.Rent($"initial{i}");
			initialObjects.Add(obj);
		}

		foreach (var obj in initialObjects)
			pool.Return(obj);

		ClassicAssert.AreEqual(capacity, pool.Count);

		// Act
		var rentedObjects = new List<TestObject>();
		for (var i = 0; i < capacity; i++)
			rentedObjects.Add(pool.Rent($"rented{i}"));

		// Assert
		ClassicAssert.AreEqual(0u, pool.Count);

		// Verify rented objects match initial objects (by ID)
		// Objects should be returned in reverse order (LIFO)
		for (var i = 0; i < capacity; i++)
			ClassicAssert.AreEqual(initialObjects[(int)(capacity - 1 - i)].Id, rentedObjects[i].Id);
	}

	[Test]
	public void Return_BeyondCapacity_IgnoresExcessObjects() {
		// Arrange
		const uint capacity = 2;
		var pool = new ObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), capacity);
		var objects = new List<TestObject>();

		// Create objects
		for (var i = 0; i < capacity + 2; i++)
			objects.Add(pool.Rent($"test{i}"));

		// Act - return all objects
		foreach (var obj in objects)
			pool.Return(obj);

		// Assert
		ClassicAssert.AreEqual(capacity, pool.Count);

		// Rent all objects from pool
		var reusedObjects = new List<TestObject>();
		for (var i = 0; i < capacity; i++)
			reusedObjects.Add(pool.Rent($"reused{i}"));

		// Verify only the last returned objects are in the pool (LIFO behavior)
		foreach (var reused in reusedObjects) {
			var index = objects.FindIndex(obj => obj.Id == reused.Id);
			ClassicAssert.GreaterOrEqual(index, 0);
			// The reused objects should be from the first ones returned
			ClassicAssert.LessOrEqual(index, objects.Count - capacity);
		}
	}

	[Test]
	public void Pool_WithZeroCapacity_WorksAsFactory() {
		// Arrange
		var pool = new ObjectPool<TestObject, TestPoolStrategy, string>(new TestPoolStrategy(), 0);

		// Act
		var obj1 = pool.Rent("test1");
		pool.Return(obj1);
		var obj2 = pool.Rent("test2");

		// Assert
		ClassicAssert.AreEqual(0u, pool.Count);
		ClassicAssert.AreNotEqual(obj1.Id, obj2.Id); // Different objects
	}
}