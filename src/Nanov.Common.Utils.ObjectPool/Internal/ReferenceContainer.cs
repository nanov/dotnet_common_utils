using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Nanov.Common.Utils.ObjectPool.Internal;

internal struct ReferenceContainer<T> where T : class {
	private T? _value;
	
	public T? Value {
		readonly get => _value;
		set => _value = value;
	}	
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool SetIfNull(T value) {
		if (_value is not null)
			return false;
		_value = value;
		return true;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TrySave(T value)
		=> _value is null && Interlocked.CompareExchange(ref _value, value, null) == null;
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryRetrieve([MaybeNullWhen(false)] out T item) {
		item = _value;
		return (item is not null
						&& item == Interlocked.CompareExchange(ref _value, null, item));
	}
	
	
}