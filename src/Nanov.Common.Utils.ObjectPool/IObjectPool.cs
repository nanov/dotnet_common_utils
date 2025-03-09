using System.Runtime.CompilerServices;

namespace Nanov.Common.Utils.ObjectPool;

internal interface IObjectPool<in T>  where T : class {
	void Return(T item);
}

public interface IObjectPool<T, in TConstructParams> where T : class {
	uint Count { get; }
	T Rent(TConstructParams param);
	RentedValue<T> RentValue(TConstructParams param, out T item);
	void Return(T item);
}


public readonly ref struct RentedValue<T> where T : class {
	private readonly IObjectPool<T> _pool;
	private readonly T _value;
	internal RentedValue(IObjectPool<T> pool, T value)
		=> (_pool, _value) = (pool, value);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose()
		=> _pool.Return(_value);
}

