namespace HiveSpace.MediaService.Func.Core.Enums;

[Flags]
public enum StoragePermissions
{
    Read = 1,
    Write = 2,
    Delete = 4,
    Create = 8
}
