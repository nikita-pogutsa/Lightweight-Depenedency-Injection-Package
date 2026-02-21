using System;
using System.Collections.Generic;
using Runtime;

namespace Editor
{
    // public static class StaticInjectorRegistry
    // {
    //     private static readonly Dictionary<RuntimeTypeHandle, Action<object, IInjectionContext>> StaticContext = new();
    //
    //     public static bool TryRegister<T>(Action<T, IInjectionContext> action) where T: class, new()
    //     {
    //         return StaticContext.TryAdd(typeof(T).TypeHandle,
    //             (obj, context) => { action?.Invoke((T) obj, context); });
    //     }
    //
    //     public static void Inject<T>(T instance, IInjectionContext injectionContext) where T: class, new()
    //     {
    //         var type = typeof(T);
    //         if (StaticContext.TryGetValue(type.TypeHandle, out var obj))
    //         {
    //             obj?.Invoke(instance, injectionContext);
    //         }
    //     }
    // }
    //
    //
    // public static class GridIml
    // {
    //     internal static void Register()
    //     {
    //         StaticInjectorRegistry.TryRegister<string> (InjectInternal);
    //
    //         void InjectInternal(string inst, IInjectionContext ctx)
    //         {
    //             //inst.DEPENDENCY = ctx[typeof(DEPENDENCY)] as DEPENDENCY;
    //         }
    //     }
    // }
}