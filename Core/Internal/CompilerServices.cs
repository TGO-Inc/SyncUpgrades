// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

// ReSharper disable once UnusedType.Global
internal static class IsExternalInit;

// ReSharper disable once RedundantAttributeUsageProperty
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Property, Inherited = false)]
// ReSharper disable once UnusedType.Global
internal sealed class RequiredMemberAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
// ReSharper disable once UnusedType.Global
internal sealed class CompilerFeatureRequiredAttribute : Attribute
{
    // ReSharper disable once UnusedParameter.Local
    internal CompilerFeatureRequiredAttribute(string featureName){ }
}