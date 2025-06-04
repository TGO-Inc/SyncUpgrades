namespace System.Runtime.CompilerServices;

internal static class IsExternalInit;

[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Property, Inherited = false)]
internal sealed class RequiredMemberAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
internal sealed class CompilerFeatureRequiredAttribute : Attribute
{
    internal CompilerFeatureRequiredAttribute(string featureName){ }
}