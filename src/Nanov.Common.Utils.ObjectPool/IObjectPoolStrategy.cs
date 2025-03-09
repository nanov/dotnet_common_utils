using System.Diagnostics.Contracts;

namespace Nanov.Common.Utils.ObjectPool;

public interface IObjectPoolStrategy<T> where T: class {
	[Pure]
	T Create<TParam>(TParam param);
	
	[Pure]
	void Prepare<TParam>(T value, TParam param);
	
	[Pure]
	void Clean(T value);
}