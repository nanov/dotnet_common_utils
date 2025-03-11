using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Nanov.Common.Utils.ObjectPool;


public sealed class ConcurrentObjectPool<T, TStrategy, TConstructParams> : IObjectPool<T>, IObjectPool<T, TConstructParams>
	where T : class
	where TStrategy : struct, IObjectPoolStrategy<T, TConstructParams> {
	private readonly TStrategy _strategy;

	private uint _maxStored = 0;
	private readonly uint _poolSize;
	private const uint MaxStoredIncrement = 3;

	private T? _fastItem;
	private readonly Memory<ReferenceContainer> _pool;

	public uint Count {
			get {
				var count = _fastItem is null ? 0u : 1u;
				
				var maxStored = _maxStored;
				if (maxStored is 0)
					return count;
				
				var span = maxStored<_poolSize 
					? _pool.Span.Slice(0, (int)maxStored)
					: _pool.Span;
				
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
		// fast path
		var item = _fastItem;
		if (item is not null &&
				item == Interlocked.CompareExchange(ref _fastItem, null, item)) {
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
			item = element.Value;
			if (item is null
					|| item != Interlocked.CompareExchange(ref element.Value, null, item))
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
		if (!_strategy.Clean(obj))
			return;

		// fast-path
		// intentionally not using interlocked
		if (_fastItem is null) {
			_fastItem = obj;
			return;
		}
		
		var span = _pool.Span;
		var i = 0;
		foreach (ref var element in span) {
			i++;
			if (element.Value is not null)
				continue;
			
			// intentionally not using interlocked
			element.Value = obj;
			
			var m = _maxStored;
			if (i > m) Interlocked.CompareExchange(ref _maxStored, Math.Min(m + MaxStoredIncrement, _poolSize), m);
			return;
		}
	}

	
	private struct ReferenceContainer {
		internal T? Value;

		/*
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool SetIfNull(T value) {
			if (Value is not null)
				return false;
			Value = value;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TrySave(T value)
			=> Interlocked.CompareExchange(ref _value, value, null) == null;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryRetrieve([MaybeNullWhen(false)] out T item) {
			item = Value;
			return (item is not null
							&& item == Interlocked.CompareExchange(ref Value, null, item));
		}
		*/
		
		public uint Count
			=> Value is null ? (uint)0 : 1;
	}
}