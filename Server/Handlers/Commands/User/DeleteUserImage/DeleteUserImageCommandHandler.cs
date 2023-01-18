﻿using Azure.Storage.Blobs;
using EventsManager.Server.Data;
using EventsManager.Shared.Exceptions;
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

        if (user.ImageUrl == null)
        {
            throw new UserDoesNotHavePictureException();
        }
        
        var fileName = user.ImageUrl.ToString().Split("-user-picture-").Last();
        var blobClient = new BlobClient(_blobStorageOptions.ConnectionString, _blobStorageOptions.ContainerName, user.Id + "-user-picture");

        await blobClient.DeleteAsync(cancellationToken: cancellationToken);
        user.ImageUrl = null;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}