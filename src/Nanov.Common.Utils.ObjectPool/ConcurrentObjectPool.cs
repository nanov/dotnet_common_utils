using System.Diagnostics.CodeAnalysis;
using Nanov.Common.Utils.ObjectPool.Internal;

namespace Nanov.Common.Utils.ObjectPool;

public class ConcurrentObjectPool<T, TStrategy>
	where T : class
	where TStrategy : struct, IObjectPoolStrategy<T> {
	
	private readonly TStrategy _strategy;
	private readonly ConcurrentLightStack<T>	_stack;

	private uint _maxStored = 0;
	private const uint MaxStoredIncrement = 3;
	
	private ReferenceContainer<T> _pocket;
	private Memory<ReferenceContainer<T>> _pool;

	public ConcurrentObjectPool(TStrategy strategy, uint maxCapacity) {
		_strategy = strategy;
		_pool = new ReferenceContainer<T>[maxCapacity-1];
		_stack = new ConcurrentLightStack<T>(maxCapacity);
	}

	public T Rent<TParam>(TParam param) {
		if (_pocket.TryRetrieve(out var item)) {
			_strategy.Prepare(item, param);
			return item;
		}
		
		var elments = _pool.Span;
		var len = elments.Length;
		for (var i = 0; i < len && i < _maxStored; i++)
			if (elments[i].TryRetrieve(out item)) {
				_strategy.Prepare(item, param);
				return item;
			}

		return _strategy.Create(param);
	}

	public void Return(T obj) {
		_strategy.Clean(obj);
		if (_pocket.SetIfNull(obj))
			return;
		var elements = _pool;
		var len = elements.Length;
		var span = elements.Span;

		for (var i = 0; i < len; i++) {
			if (!span[i].SetIfNull(obj))
				continue;
			var m = _maxStored;
			if (i >= m) Interlocked.CompareExchange(ref _maxStored, m + MaxStoredIncrement, m);
		}
	}
}