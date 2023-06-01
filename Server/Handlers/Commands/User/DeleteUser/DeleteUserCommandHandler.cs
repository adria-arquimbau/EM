using EventsManager.Server.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Commands.User.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommandRequest>
{
    private readonly ApplicationDbContext _dbContext;

    public DeleteUserCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task Handle(DeleteUserCommandRequest request, CancellationToken cancellationToken)
    {
        await _dbContext.Users
            .Where(x => x.Id == request.UserId)
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);
    }
}