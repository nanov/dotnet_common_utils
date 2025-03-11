using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Nanov.Common.Utils.ObjectPool;

public sealed class ObjectPool<T, TStrategy, TConstructParams> : IObjectPool<T>, IObjectPool<T, TConstructParams>
	where T : class
	where TStrategy : struct, IObjectPoolStrategy<T, TConstructParams> {

	private readonly PoolEntry[] _pool;
	private readonly TStrategy _strategy;
	private readonly uint _capacity;
	
	private uint _count;

	public uint Count {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _count;
	}

	public ObjectPool(TStrategy strategy, uint maxCapacity) {
		_capacity = maxCapacity;
		_strategy = strategy;
		_pool = new PoolEntry[maxCapacity];
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Rent(TConstructParams param) {
		if (_count == 0)
			return _strategy.Create(param);
		
		// eliminate bounds checking
		ref var e =  ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_pool), --_count);
		Debug.Assert(e.Value is not null);
		var item = e.Value!;
		e.Value = null;
		_strategy.Prepare(item, param);
		return item;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RentedValue<T> RentValue(TConstructParams param, out T item) {
		item = Rent(param);
		return new RentedValue<T>(this, item);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Return(T item) {
		if (!_strategy.Clean(item))
			return;
		
		if (_count >= _capacity)
			return;

		// Eliminate bounds checking
		ref var e =  ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_pool), _count++);
		Debug.Assert(e.Value is null);
		e.Value = item;
	}

	private struct PoolEntry {
		public T? Value;
	}
}