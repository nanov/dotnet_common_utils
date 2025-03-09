using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Nanov.Common.Utils.ObjectPool.Internal;

/// <summary>
/// Light bounded concurrent stack
/// TODO: detailed description regarding guarantees and boundaries
/// </summary>
/// <typeparam name="T"></typeparam>
public class ConcurrentLightStack<T> where T : class {
	public uint Count => _tail;

	private readonly T?[] _stack;
	private readonly uint _capacity;
	private uint _tail;
	
	public ConcurrentLightStack(uint maxCapacity = 128) {
		_stack = new T[maxCapacity];
		_capacity = maxCapacity;
		_tail = 0;
	}


	public bool TryPush(T elem) {
		while (true) {
			var current = Interlocked.CompareExchange(ref _tail, 0, 0);
			if (current >= _capacity)
				return false;
			
			var newSize = current + 1;
			if (Interlocked.CompareExchange(ref _tail, newSize, current) != current)
				continue;
			
			var now = Interlocked.CompareExchange(ref _stack[current], elem, null);
			return now is null;
		}
	}

	public bool TryPop([NotNullWhen(true)] out T? elem) {
		while (true) {
			var current = Interlocked.CompareExchange(ref _tail, 0, 0);
			if (current <= 0) {
				elem = null;
				return false;
			}

			var newSize = current - 1;
			if (Interlocked.CompareExchange(ref _tail, newSize, current) != current)
				continue;
			
			elem = Interlocked.Exchange(ref _stack[newSize], null);
			return elem is not null;
		}
	}
}
