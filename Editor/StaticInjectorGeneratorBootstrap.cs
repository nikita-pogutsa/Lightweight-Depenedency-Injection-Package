using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Editor.Roslyn;
using Runtime;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Editor
{
    public static class StaticInjectorGeneratorBootstrap
    {
        static StaticInjectorGeneratorBootstrap()
        {
            CompilationPipeline.compilationFinished += RegenerateDependencyFiles;
        }

        private static void RegenerateDependencyFiles(object obj)
        {
            Debug.Log("Generating");
        }
    }


    public sealed class AssemblyCache : AssetPostprocessor
    {
        internal const string PackageTempsDirectory = "LightInject";
        static readonly byte[] Separator = new byte[] {0};

        private string asmdfHash;


        [MenuItem("LightInject/Regenerate Static Injector")]
        public static void Regenerate()
        {
            RecalculateHash();
            StaticInjectorGenerator.RegenerateStaticInjector();
        }


        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var asset in importedAssets)
            {
                if (asset.EndsWith(".asmdef"))
                {
                }
            }
        }

        private static void RecalculateHash()
        {
            var assetsPaths = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset")
                .Select(AssetDatabase.GUIDToAssetPath).OrderBy(path => path);

            var sha = SHA256.Create();
            foreach (var assetPath in assetsPaths)
            {
                var assetBytes = File.ReadAllBytes(assetPath);
                sha.TransformBlock(assetBytes, 0, assetBytes.Length, null, 0);
                sha.TransformBlock(Separator, 0, Separator.Length, null, 0);
            }

            sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            var computedHash = Convert.ToBase64String(sha.Hash);

            if (!AssetDatabase.IsValidFolder($"Assets/{PackageTempsDirectory}"))
            {
                AssetDatabase.CreateFolder("Assets", PackageTempsDirectory);
            }

            var assemblyCacheStateSo = AssemblyCacheState.instance;
            if (assemblyCacheStateSo.Hash != computedHash | assemblyCacheStateSo.AssemblyList == null)
            {
                assemblyCacheStateSo.Save(computedHash);


                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var assemblyList = (from assembly in assemblies
                    let referencedAssemblies = assembly.GetReferencedAssemblies()
                    where referencedAssemblies.Any
                        (referenced => referenced.Name == "Utils.LightInject")
                    select assembly).ToList();
                assemblyCacheStateSo.SaveAssemblyList(assemblyList);
            }
        }
    }

    internal static class StaticInjectorGenerator
    {
        public static void RegenerateStaticInjector()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(@"
				using System;
				using System.Collections.Generic;
				using Runtime;
				using UnityEngine;

				namespace LightInject.Generated
				{");

            //GenerateInjectionRegistry(builder);
            GenerateConcreteImpl(builder);
            GenerateSubscriptionClass(builder);
            //namespace bracket closed
            builder.AppendLine("}");
            if (!AssetDatabase.IsValidFolder("Assets/Source/LightInject"))
            {
                AssetDatabase.CreateFolder("Assets/Source", "LightInject");
            }

            File.WriteAllText(Path.Combine(
                Application.dataPath, "Source",
                "LightInject", "Generated_Inject.cs"), builder.ToString());
            // File.WriteAllText(Path.Combine(
            //         Application.dataPath, "Source", "LightInject", "StaticInjectorGenerator_Inject.cs"),
            //     builder.ToString());
        }

        private static void GenerateSubscriptionClass(StringBuilder builder)
        {
            builder.Append(@"
            internal static class StaticInjectorSubscription
                {
                    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
                    internal static void Subscribe()
                        {");
            var typeList = AssemblyCacheState.instance.TypeList;
            foreach (var type in typeList)
            {
                builder.AppendLine($"{type.Name}_InjectorImpl.Register();");
            }
            builder.AppendLine(@"}
                }
             ");
        }


        private static void GenerateInjectionRegistry(StringBuilder builder)
        {
            builder.Append(@"

                    public static class StaticInjectorRegistry
                    {
					private static readonly Dictionary<RuntimeTypeHandle, Action<object, IInjectionContext>> StaticContext = new();

					public static bool TryRegister<T>(Action<T, IInjectionContext> action)
					{
						return StaticContext.TryAdd(typeof(T).TypeHandle,
							(obj, context) => { action?.Invoke((T) obj, context); });
					}

			        public static void Inject<T>(T instance, IInjectionContext injectionContext)
			        {
			            var type = typeof(T);
			            if (StaticContext.TryGetValue(type.TypeHandle, out var obj))
			            {
			                obj?.Invoke(instance, injectionContext);
			            }
			        }
                    }");
        }

        private static void GenerateConcreteImpl(StringBuilder builder)
        {
            var typeList = AssemblyCacheState.instance.TypeList;

            foreach (var type in typeList)
            {
                builder.AppendLine($"public static class {type.Name}_InjectorImpl");
                builder.AppendLine("{");
                builder.Append(@" internal static void Register()
                    {");
                builder.Append($"StaticInjectorRegistry.TryRegister<{type.Name}>(InjectInternal);");

                builder.Append($"void InjectInternal({type.Name} inst, IInjectionContext ctx)");
                builder.AppendLine("{");

                var injectAttrFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(field => field.GetCustomAttribute<InjectAttributeSpecific>() != null);
                foreach (var fieldInfo in injectAttrFields)
                {
                    if (fieldInfo.GetCustomAttribute<InjectAttributeSpecific>() != null)
                    {
                        //var fieldType = fieldInfo.GetCustomAttribute<InjectAttributeSpecific>().GetType();
                        builder.AppendFormat(@"
                                inst.{0} = ctx[typeof({1})] as {2};", fieldInfo.Name, fieldInfo.FieldType,
                            fieldInfo.FieldType);
                    }
                }

                builder.AppendLine("}");
                builder.AppendLine("}");
                builder.AppendLine("}");
            }
        }
    }
}