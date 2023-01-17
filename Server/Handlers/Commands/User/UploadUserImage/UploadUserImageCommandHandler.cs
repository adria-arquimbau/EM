using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EventsManager.Server.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventsManager.Server.Handlers.Commands.User.UploadUserImage;

public class UploadUserImageCommandHandler : AsyncRequestHandler<UploadUserImageCommandRequest>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly BlobStorageSettings _blobStorageOptions;

    public UploadUserImageCommandHandler(ApplicationDbContext dbContext, IOptions<BlobStorageSettings> blobStorageOptions)
    {
        _dbContext = dbContext;
        _blobStorageOptions = blobStorageOptions.Value; 
    }
    
    protected override async Task Handle(UploadUserImageCommandRequest request, CancellationToken cancellationToken)
    {
        var blobClient = new BlobClient(_blobStorageOptions.ConnectionString, _blobStorageOptions.ContainerName, request.File.FileName);
        await blobClient.UploadAsync(request.File.OpenReadStream(), new BlobHttpHeaders { ContentType = "image/jpeg" }, conditions: null, cancellationToken: cancellationToken);
        
        var user = await _dbContext.Users
            .SingleAsync(x => x.Id == request.UserId, cancellationToken: cancellationToken);
        
        user.ImageUrl = blobClient.Uri;
    }
}