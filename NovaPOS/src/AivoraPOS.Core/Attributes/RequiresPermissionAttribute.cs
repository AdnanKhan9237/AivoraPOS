using AivoraPOS.Core.Enums;

namespace AivoraPOS.Core.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequiresPermissionAttribute : Attribute
{
    public RequiresPermissionAttribute(Permission permission)
    {
        Permission = permission;
    }

    public Permission Permission { get; }
}
