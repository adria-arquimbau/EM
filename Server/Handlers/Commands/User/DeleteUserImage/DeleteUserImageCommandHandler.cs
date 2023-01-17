using EventsManager.Server.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Commands.User.DeleteUserImage;

public class DeleteUserImageCommandHandler : AsyncRequestHandler<DeleteUserImageCommandRequest>
{
    private readonly ApplicationDbContext _dbContext;

    public DeleteUserImageCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    protected override async Task Handle(DeleteUserImageCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .SingleAsync(x => x.Id == request.UserId, cancellationToken: cancellationToken);
        
        user.ImageUrl = null;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}