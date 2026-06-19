using HiveSpace.Application.Shared.Queries;
using HiveSpace.UserService.Application.Users.Dtos;

namespace HiveSpace.UserService.Application.Users.Queries.GetUserProfile;

public record GetUserProfileQuery : IQuery<GetUserProfileResponseDto>;
