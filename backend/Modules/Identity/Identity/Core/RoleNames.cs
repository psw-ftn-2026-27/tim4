namespace Identity.Core;

// The role names, written once: registration, the startup roles, the demo users and the
// contract's explorer filter all refer to them, so a typo cannot silently empty a filter.
internal static class RoleNames
{
    public const string Administrator = "administrator";
    public const string Explorer = "explorer";
}
