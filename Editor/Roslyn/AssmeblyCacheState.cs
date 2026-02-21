using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Runtime;
using UnityEditor;
namespace Editor.Roslyn
{
	internal sealed class AssemblyCacheState : ScriptableSingleton<AssemblyCacheState>
	{
		private string _hash;
		private IEnumerable<string> _assemblyList;
		public string Hash => _hash;

		private IEnumerable<Assembly> _loadedAssemblies;
		private IEnumerable<Type> _typeList;

		public List<Assembly> ReferencedAssemblies
		{
			get
			{
				if( _loadedAssemblies == null || !_loadedAssemblies.Any() )
				{
					_loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
				}

				return _loadedAssemblies.Where(x =>
				{
					if (AssemblyList != null)
					{
						return AssemblyList.Contains(x.FullName);
					}

					return false;
				}).ToList();
			}
		}
		public IEnumerable<string> AssemblyList => _assemblyList;
		public IEnumerable<Type> TypeList => _typeList;

		public void Save(string hash)
		{
			_hash = hash;
		}
		public void SaveAssemblyList(List<Assembly> assemblyList)
		{
			_assemblyList = assemblyList.Select(x => x.FullName);
			_typeList = assemblyList.SelectMany(x=>x.GetTypes()).Where(x =>
			{
				var fields = x.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
				return fields.Any(fieldInfo => fieldInfo.GetCustomAttribute<InjectAttributeSpecific>() != null);
			});

		}
	}
}
