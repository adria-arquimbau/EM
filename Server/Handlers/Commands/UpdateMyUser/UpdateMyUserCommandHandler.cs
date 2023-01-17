﻿using EventsManager.Server.Data;
using EventsManager.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Commands.UpdateMyUser;

public class UpdateMyUserCommandHandler : AsyncRequestHandler<UpdateMyUserCommandRequest>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateMyUserCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    protected override async Task Handle(UpdateMyUserCommandRequest request, CancellationToken cancellationToken)
    {
        if (request.UserId != request.UserDto.Id)
        {
            throw new NotSameUserForUpdateException();
        }

        var userEntity = await _dbContext.Users.SingleAsync(x => x.Id == request.UserId, cancellationToken: cancellationToken);
        
        userEntity.Name = request.UserDto.Name;
        userEntity.FamilyName = request.UserDto.FamilyName;
        userEntity.Address = request.UserDto.Address;
        userEntity.City = request.UserDto.City;
        userEntity.Country = request.UserDto.Country;
        userEntity.PostalCode = request.UserDto.PostalCode;
        userEntity.PhoneNumber = request.UserDto.PhoneNumber;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}