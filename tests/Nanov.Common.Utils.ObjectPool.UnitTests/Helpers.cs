namespace Nanov.Common.Utils.ObjectPool.UnitTests;
public struct TestPoolStrategy : IObjectPoolStrategy<TestObject, string> {
	public TestObject Create(string param) {
		return new TestObject { Id = Guid.NewGuid(), Data = param };
	}

	public void Prepare(TestObject obj, string param) {
		obj.Data = param;
		obj.PrepareCount++;
	}

	public void Clean(TestObject obj) {
		obj.Data = null;
		obj.CleanCount++;
	}
}

// Test object class
public class TestObject {
	public Guid Id { get; set; }
	public string Data { get; set; }
	public int PrepareCount { get; set; }
	public int CleanCount { get; set; }
}
