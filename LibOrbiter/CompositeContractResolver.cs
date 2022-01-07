using System.Collections;
using Newtonsoft.Json.Serialization;

namespace LibOrbiter;

public class CompositeContractResolver : IContractResolver, IEnumerable<IContractResolver>
{
	private readonly List<IContractResolver> _contractResolvers = new();

	public IEnumerator<IContractResolver> GetEnumerator()
	{
		return _contractResolvers.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable) _contractResolvers).GetEnumerator();
	}

	public void Add(IContractResolver contractResolver)
	{
		if (contractResolver == null) throw new ArgumentNullException(nameof(contractResolver));
		_contractResolvers.Add(contractResolver);
	}

	public JsonContract ResolveContract(Type type)
	{
		return _contractResolvers.Select(resolver => resolver.ResolveContract(type)).FirstOrDefault()!;
	}
}