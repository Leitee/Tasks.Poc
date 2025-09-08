namespace Tasks.Poc.Application.Users;

using System.Collections.Generic;
using System.Linq;
using Tasks.Poc.Contracts.DTOs;
using Tasks.Poc.Domain.Entities;

public static partial class MapperExtensions
{
    public static UserDto? ToDto(this User user)
    {
        if (user == null)
        {
            return null;
        }

        return new(
            user.Id,
            user.Name,
            user.Email,
            user.CreatedAt,
            user.LastLoginAt);
    }

    public static IEnumerable<UserDto> ToDtos(this IEnumerable<User> users) =>
        users.Select(static user => user.ToDto()!);
}
