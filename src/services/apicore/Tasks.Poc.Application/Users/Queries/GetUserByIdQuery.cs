namespace Tasks.Poc.Application.Users.Queries;

using MediatR;
using Tasks.Poc.Contracts.DTOs;
using Tasks.Poc.Application.Interfaces;
using Tasks.Poc.Chassis.Mapper;
using Tasks.Poc.Domain.Entities;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper<User, UserDto> _mapper;

    public GetUserByIdHandler(IUnitOfWork unitOfWork, IMapper<User, UserDto> mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
        return _mapper.Map(user);
    }
}
