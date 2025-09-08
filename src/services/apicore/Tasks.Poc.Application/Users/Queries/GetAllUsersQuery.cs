namespace Tasks.Poc.Application.Users.Queries;

using MediatR;
using Tasks.Poc.Contracts.DTOs;
using Tasks.Poc.Application.Interfaces;

public record GetAllUsersQuery : IRequest<List<UserDto>>;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllUsersHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.UserRepository.GetAllAsync(cancellationToken);

        return users
                .ToDtos()
                .ToList();
    }
}
