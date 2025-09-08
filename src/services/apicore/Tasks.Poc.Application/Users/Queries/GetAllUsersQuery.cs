namespace Tasks.Poc.Application.Users.Queries;

using MediatR;
using Tasks.Poc.Contracts.DTOs;
using Tasks.Poc.Application.Interfaces;
using Tasks.Poc.Chassis.Mapper;
using Tasks.Poc.Domain.Entities;

public record GetAllUsersQuery : IRequest<List<UserDto>>;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper<User, UserDto> _userMapper;

    public GetAllUsersHandler(IUnitOfWork unitOfWork, IMapper<User, UserDto> userMapper)
    {
        _unitOfWork = unitOfWork;
        _userMapper = userMapper;
    }

    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.UserRepository.GetAllAsync(cancellationToken);

        return _userMapper.Map(users).ToList();
    }
}
