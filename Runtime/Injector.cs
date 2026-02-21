using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime
{
	public interface IInjectionContext
	{
		object this[Type type] { get; }
	}

	public class InjectionContext: IInjectionContext
	{
		private Dictionary<Type, object> injectedTypes = new Dictionary<Type, object>();
		public void AddToContext<T>(Type type, T val)
		{
			injectedTypes.Add(type,val);
		}
		public object this[Type type] => injectedTypes[ type ] ?? throw new Exception($"Type of {type} has not been registered!");
	}

	public class Injector
	{
		private readonly InjectionContext context;
		public Injector()
		{
			context = new InjectionContext();
		}
		public bool TryInject()
		{
			return false;
		}

		public void Register<T>(T val)
		{
			context.AddToContext(typeof(T), val);
		}

		public GameObject InstantiateMonoBehavior<T>(GameObject obj) where T : Component
		{
			var component = obj.GetComponent<T>();
			if( !component )
			{
				//Injection<T>.Inject();
				//throw 
			}
			return obj;
		}
	}
}
