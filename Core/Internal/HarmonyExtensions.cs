using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace SyncUpgrades.Core.Internal;

internal class IgnoreMethodNotFoundPatchExceptionAttribute : HarmonyAttribute;

internal static class HarmonyExtensions
{
    private static bool IsDefined<T>(this Type type)
        => type.IsDefined(typeof(T));
    private static bool IsDefined<T>(this MethodInfo method)
        => method.IsDefined(typeof(T));
    
    private static bool IsDefined<T>(this MethodInfo method, bool inherit)
        => method.IsDefined(typeof(T), inherit);

    public static void PatchAllSafe(this Harmony harmony)
    {
        var currentAssembly = Assembly.GetExecutingAssembly();
        IEnumerable<Type> types = AccessTools.GetTypesFromAssembly(currentAssembly)
                                             .Where(t => t.IsDefined<HarmonyPatch>() && t.IsClass);
        foreach (Type? type in types)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                       .Where(m => m.IsDefined<HarmonyPatch>())
                                       .ToArray();
            
            foreach (MethodInfo method in methods)
                harmony.SafePatch(type, method);
        }
    }
    
    private static void SafePatch(this Harmony harmony, Type type, MethodInfo method)
    {
        // get target class
        Type targetType = type.GetCustomAttribute<HarmonyPatch>()?.info.declaringType
            ?? method.GetCustomAttribute<HarmonyPatch>()?.info.declaringType
            ?? throw new InvalidOperationException("Could not find target type for Harmony patch.");
        
        string methodName = type.GetCustomAttribute<HarmonyPatch>()?.info.methodName
            ?? method.GetCustomAttribute<HarmonyPatch>()?.info.methodName
            ?? throw new InvalidOperationException("Could not find target method name for Harmony patch.");
        
        Type[]? parameterTypes = type.GetCustomAttribute<HarmonyPatch>()?.info.argumentTypes
            ?? method.GetCustomAttribute<HarmonyPatch>()?.info.argumentTypes
            ?? null;
        
        CallType callType = GetCallType(method);
        MethodInfo? targetMethod = AccessTools.Method(targetType, methodName, parameterTypes);
        
        bool isSafePatch = method.IsDefined<IgnoreMethodNotFoundPatchExceptionAttribute>();
        if (isSafePatch && targetMethod is null)
        {
            Entry.LogSource.LogWarning($"Failed to patch method {method.Name} in type {type.FullName}");
            return;
        }
        
        DoRawPatch(harmony, targetMethod, method, callType);
    }

    private static void DoRawPatch(Harmony harmony, MethodInfo targetMethod, MethodInfo method, CallType callType)
    {
        switch (callType)
        {
            case CallType.Prefix:
                harmony.Patch(targetMethod, prefix: new HarmonyMethod(method));
                break;
            case CallType.Postfix:
                harmony.Patch(targetMethod, postfix: new HarmonyMethod(method));
                break;
            case CallType.Transpiler:
                harmony.Patch(targetMethod, transpiler: new HarmonyMethod(method));
                break;
            case CallType.Finalizer:
                harmony.Patch(targetMethod, finalizer: new HarmonyMethod(method));
                break;
            case CallType.ILManipulator:
                harmony.Patch(targetMethod, ilmanipulator: new HarmonyMethod(method));
                break;
            default:
                throw new InvalidOperationException($"Unknown call type: {callType}");
        }
    }
    
    private static CallType GetCallType(MethodInfo method)
    {
        if (method.IsDefined<HarmonyPrefix>())
            return CallType.Prefix;
        if (method.IsDefined<HarmonyPostfix>())
            return CallType.Postfix;
        if (method.IsDefined<HarmonyTranspiler>())
            return CallType.Transpiler;
        if (method.IsDefined<HarmonyFinalizer>())
            return CallType.Finalizer;
        if (method.IsDefined<HarmonyILManipulator>())
            return CallType.ILManipulator;

        throw new InvalidOperationException($"Method {method.Name} does not have a valid Harmony call type defined.");
    }
    
    private enum CallType
    {
        Prefix,
        Postfix,
        Transpiler,
        Finalizer,
        ILManipulator,
    }
}

