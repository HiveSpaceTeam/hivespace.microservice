using HiveSpace.Application.Shared.Commands;


namespace HiveSpace.MediaService.Api.Features.PresignUrl;

public record PresignUrlCommand(
    string FileName, 
    string ContentType, 
    long FileSize, 
    string EntityType, 
    string? EntityId) : ICommand<PresignUrlResponse>;
