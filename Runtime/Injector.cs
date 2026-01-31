using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime
{
	public static class Injection<T>
	{
		public static void Inject()
		{
		}
	}

	public class Injector
	{
		private Dictionary<Type, object> injectedTypes = new Dictionary<Type, object>();
		public bool TryInject()
		{
			return false;
		}

		public void Register<T>(T val)
		{
			injectedTypes.Add(typeof(T), val);
		}

		public GameObject InstantiateMonoBehavior<T>(GameObject obj) where T : Component
		{
			var component = obj.GetComponent<T>();
			if( !component )
			{
				Injection<T>.Inject();
				//throw 
			}
			return obj;
		}
	}
}
