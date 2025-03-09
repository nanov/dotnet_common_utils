using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Nanov.Common.Utils.ObjectPool;


public class ConcurrentObjectPool<T, TStrategy, TConstructParams> : IObjectPool<T>, IObjectPool<T, TConstructParams>
	where T : class
	where TStrategy : struct, IObjectPoolStrategy<T, TConstructParams> {
	private readonly TStrategy _strategy;

	private uint _maxStored = 0;
	private readonly uint _poolSize;
	private const uint MaxStoredIncrement = 3;

	private ReferenceContainer _pocket;
	private readonly Memory<ReferenceContainer> _pool;

	public uint Count {
			get {
				var count = _pocket.Count;
				
				var maxStored = _maxStored;
				var span = _pool.Span.Slice(0, (int)maxStored);
				foreach (ref var el in span)
					count += el.Count;
					
				return count;
			}
	}

	public ConcurrentObjectPool(TStrategy strategy, uint maxCapacity) {
		_strategy = strategy;
		_poolSize = maxCapacity - 1;
		_pool = new ReferenceContainer[_poolSize];
	}

	public T Rent(TConstructParams param) {
		if (_pocket.TryRetrieve(out var item)) {
			_strategy.Prepare(item, param);
			return item;
		}

		// thread-safe read
		var maxStored = _maxStored;
		if (maxStored == 0)
			return _strategy.Create(param);
		
		var span = maxStored < _poolSize 
			? _pool.Span.Slice(0, (int)maxStored)
			: _pool.Span;

		foreach (ref var element in span) {
			if (!element.TryRetrieve(out item))
				continue;
			_strategy.Prepare(item, param);
			return item;
		}

		return _strategy.Create(param);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RentedValue<T> RentValue(TConstructParams param, out T item) {
		item = Rent(param);
		return new RentedValue<T>(this, item);
	}
	
	public void Return(T obj) {
		_strategy.Clean(obj);
		if (_pocket.SetIfNull(obj))
			return;
		
		var span = _pool.Span;
		var i = 0;
		foreach (ref var element in span) {
			i++;
			if (!element.SetIfNull(obj))
				continue;
			
			var m = _maxStored;
			if (i > m) Interlocked.CompareExchange(ref _maxStored, Math.Min(m + MaxStoredIncrement, _poolSize), m);
			return;
		}
	}

	
	private struct ReferenceContainer {
		private T? _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool SetIfNull(T value) {
			if (_value is not null)
				return false;
			_value = value;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TrySave(T value)
			=> Interlocked.CompareExchange(ref _value, value, null) == null;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryRetrieve([MaybeNullWhen(false)] out T item) {
			item = _value;
			return (item is not null
							&& item == Interlocked.CompareExchange(ref _value, null, item));
		}
		
		public uint Count
			=> _value is null ? (uint)0 : 1;
	}
}