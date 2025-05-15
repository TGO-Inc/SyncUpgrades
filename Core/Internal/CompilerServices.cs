namespace System.Runtime.CompilerServices;

public static class IsExternalInit;

[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Property, Inherited = false)]
public sealed class RequiredMemberAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class CompilerFeatureRequiredAttribute : Attribute
{
    public CompilerFeatureRequiredAttribute(string featureName){ }
}