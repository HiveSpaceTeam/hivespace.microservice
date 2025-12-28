using HiveSpace.Application.Shared.Commands;
using MediatR;

namespace HiveSpace.MediaService.Api.Features.ConfirmUpload;

public record ConfirmUploadCommand(Guid Id, string EntityId) : ICommand;
