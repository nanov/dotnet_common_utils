using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Nanov.Common.Utils.ObjectPool;

public interface IObjectPoolStrategy<T, in TConstructParams> where T : class {
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	T Create(TConstructParams param);

	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void Prepare(T value, TConstructParams param);

	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void Clean(T value);
}
