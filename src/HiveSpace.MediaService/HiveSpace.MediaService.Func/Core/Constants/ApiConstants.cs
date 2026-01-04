namespace HiveSpace.MediaService.Func.Core.Constants;

public static class ApiConfigs
{
    public const string Version = "v1";
    public const string RouteBase = $"{Version}/media";

    // Feature Routes
    public const string ConfirmUpload = $"{RouteBase}/{{fileId}}/confirm";
    public const string PresignUrl = $"{RouteBase}/presign-url";
}
