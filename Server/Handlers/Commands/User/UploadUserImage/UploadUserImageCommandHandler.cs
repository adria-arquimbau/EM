using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EventsManager.Server.Data;
using EventsManager.Server.Settings;
using EventsManager.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventsManager.Server.Handlers.Commands.User.UploadUserImage;

public class UploadUserImageCommandHandler : IRequestHandler<UploadUserImageCommandRequest, UploadResult>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHostEnvironment _env;
    private readonly BlobStorageSettings _blobStorageOptions;

    public UploadUserImageCommandHandler(ApplicationDbContext dbContext, IOptions<BlobStorageSettings> blobStorageOptions, IHostEnvironment env)
    {
        _dbContext = dbContext;
        _env = env;
        _blobStorageOptions = blobStorageOptions.Value; 
    }


    public async Task<UploadResult> Handle(UploadUserImageCommandRequest request, CancellationToken cancellationToken)
    {
        var uploadResult = new UploadResult();
        
        var user = await _dbContext.Users
            .SingleAsync(x => x.Id == request.UserId, cancellationToken: cancellationToken);
        
        var blobClient = new BlobClient(_blobStorageOptions.ConnectionString, _blobStorageOptions.ProfileImageContainerName, user.Id + "-user-picture");
        await blobClient.UploadAsync(request.File.OpenReadStream(), new BlobHttpHeaders { ContentType = request.File.ContentType }, conditions: null, cancellationToken: cancellationToken);
        
        user.ImageUrl = blobClient.Uri;

        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return uploadResult;
    }
}
