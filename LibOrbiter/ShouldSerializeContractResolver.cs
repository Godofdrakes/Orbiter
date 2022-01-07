using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LibOrbiter;

public class ShouldSerializeContractResolver : DefaultContractResolver
{
	protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
	{
		var property = base.CreateProperty(member, memberSerialization);
		
		var collectionInterface = property.PropertyType?.GetInterface(nameof(ICollection));
		if (collectionInterface != null)
		{
			property.ShouldSerialize = instance =>
				(instance.GetType().GetProperty(property.PropertyName!)!.GetValue(instance) as ICollection)!.Count > 0;
		}

		return property;
	}
}