namespace Tasks.Poc.Application.Users.Queries;

using MediatR;
using Tasks.Poc.Contracts.DTOs;
using Tasks.Poc.Application.Interfaces;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
        return user?.ToDto();
    }
}
