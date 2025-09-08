namespace Tasks.Poc.Application.Users;

using System.Collections.Generic;
using System.Linq;
using Tasks.Poc.Chassis.Mapper;
using Tasks.Poc.Contracts.DTOs;
using Tasks.Poc.Domain.Entities;

public class UserMapper : IMapper<User, UserDto>
{
    public UserDto? Map(User? source)
    {
        if (source == null)
        {
            return null;
        }

        return new(
            source.Id,
            source.Name,
            source.Email,
            source.CreatedAt,
            source.LastLoginAt);
    }

    public IReadOnlyList<UserDto> Map(IReadOnlyList<User> sources) =>
        sources.Select(source => Map(source)!).ToList().AsReadOnly();
}
