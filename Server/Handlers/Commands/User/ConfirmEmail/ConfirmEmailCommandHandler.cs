using EventsManager.Server.Data;
using EventsManager.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Commands.User.ConfirmEmail;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommandRequest>
{
    private readonly ApplicationDbContext _dbContext;

    public ConfirmEmailCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ConfirmEmailCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken: cancellationToken);
        if (user == null)
        {
            throw new UserNotFoundException(request.UserId);
        }
        
        user.EmailConfirmed = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}