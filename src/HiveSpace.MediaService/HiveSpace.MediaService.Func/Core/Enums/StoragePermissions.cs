namespace HiveSpace.MediaService.Func.Core.Enums;

[Flags]
public enum StoragePermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    Create = 8,
    List = 16
}
