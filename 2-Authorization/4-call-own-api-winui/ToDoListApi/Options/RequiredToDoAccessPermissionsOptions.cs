namespace ToDoListApi.Options;

public class RequiredTodoAccessPermissionsOptions
{
    public const string RequiredTodoAccessPermissions = "RequiredTodoAccessPermissions";

    // Set these keys as constants to make them accessible to the 'RequiredScopeOrAppPermission' attribute. You can add
    // multiple spaces separated entries for each string in the 'appsettings.json' file and they will be used by the
    // 'RequiredScopeOrAppPermission' attribute.
    public const string RequiredDelegatedTodoReadClaimsKey =
        $"{RequiredTodoAccessPermissions}:RequiredDelegatedTodoReadClaims";

    public const string RequiredDelegatedTodoWriteClaimsKey =
        $"{RequiredTodoAccessPermissions}:RequiredDelegatedTodoWriteClaims";

    public const string RequiredApplicationTodoReadWriteClaimsKey =
        $"{RequiredTodoAccessPermissions}:RequiredApplicationTodoReadWriteClaims";
}