using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime
{
	
	public static class StaticInjectorRegistry
	{
		private static readonly Dictionary<RuntimeTypeHandle, Action<object, IInjectionContext>> StaticContext = new();

		public static bool TryRegister<T>(Action<T, IInjectionContext> action)
		{
			return StaticContext.TryAdd(typeof(T).TypeHandle,
				(obj, context) => { action?.Invoke((T) obj, context); });
		}

		public static bool Inject<T>(T instance, IInjectionContext injectionContext)
		{
			var type = typeof(T);
			if (StaticContext.TryGetValue(type.TypeHandle, out var obj))
			{
				obj?.Invoke(instance, injectionContext);
				return true;
			}

			return false;
		}
	}
	
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
		private readonly InjectionContext context = new();

		public bool TryInject<T>(T instance)
		{
			StaticInjectorRegistry.Inject(instance, context);
			return false;
		}

		public void Record<T>(T val)
		{
			context.AddToContext(typeof(T), val);
		}

		public GameObject InstantiateMonoBehavior<T>(GameObject obj) where T : Component
		{
			var component = obj.GetComponent<T>();
			if( !component )
			{
				
			}
			return obj;
		}
	}
}
