using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"FrameworkCommon.dll",
		"FrameworkRuntime.dll",
		"UnityEngine.CoreModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// System.Action<object>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<object>
	// System.Predicate<object>
	// }}

	public void RefMethods()
	{
		// bool Framework.Runtime.MGameModule.GameModuleManager.TryGetGameModule<object>(object&)
		// object Framework.Runtime.Modules.UI.PrefabBind.PrefabBinder.GetObj<object>(string)
		// object Framework.Runtime.Modules.UI.PrefabBind.PrefabBinder.TryFindObject<object>(string)
		// object Framework.Runtime.UI.PannelManager.OpenPanel<object>(string,int)
		// object Framework.Utils.Utility.Json.ToObject<object>(string)
		// object Framework.Utils.Utility.Json.IJsonHelper.ToObject<object>(string)
		// object Framework.Utils.Utility.ReflectionUtil.CreateInstance<object>()
		// System.Void Game.Modules.GameModuleBase.RegisterHandler<object>()
		// bool Game.Modules.GameModuleBase.TryGetHandler<object>(object&)
		// object UnityEngine.Component.GetComponent<object>()
	}
}