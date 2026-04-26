using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.MediaService.Core.Features.Media.Commands.ConfirmUpload;

public record ConfirmUploadCommand(Guid FileId, string EntityId) : ICommand;
