using Azure.Storage.Blobs;
using EventsManager.Server.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventsManager.Server.Handlers.Commands.User.DeleteUserImage;

public class DeleteUserImageCommandHandler : AsyncRequestHandler<DeleteUserImageCommandRequest>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly BlobStorageSettings _blobStorageOptions;

    public DeleteUserImageCommandHandler(ApplicationDbContext dbContext, IOptions<BlobStorageSettings> blobStorageOptions)
    {
        _dbContext = dbContext;
        _blobStorageOptions = blobStorageOptions.Value;
    }
    
    protected override async Task Handle(DeleteUserImageCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .SingleAsync(x => x.Id == request.UserId, cancellationToken: cancellationToken);
        
        user.ImageUrl = null;
        
        var blobClient = new BlobClient(_blobStorageOptions.ConnectionString, _blobStorageOptions.ContainerName, user.Id + "-user-picture");

        await blobClient.DeleteAsync(cancellationToken: cancellationToken);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
