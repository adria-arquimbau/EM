using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Event = EventsManager.Server.Models.Event;

namespace EventsManager.Server.Handlers.Commands.Events.Create;

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommandRequest>
{
    private readonly ApplicationDbContext _context;

    public CreateEventCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task Handle(CreateEventCommandRequest request, CancellationToken cancellationToken)
    {
        var owner = await _context.Users.SingleAsync(x => x.Id == request.UserId, cancellationToken);

        var existingEvent = await _context.Events.SingleOrDefaultAsync(x => x.Name == request.Request.Name, cancellationToken);
        if (existingEvent != null)
        {
            throw new Exception("Event with this name already exists");
        }
        
        var newEvent = new Event(request.Request.Name, request.Request.Description, request.Request.Location, request.Request.MaxRegistrations, owner, request.Request.StartDate.ToUniversalTime(), request.Request.FinishDate.ToUniversalTime(), request.Request.OpenRegistrationsDate.ToUniversalTime(), request.Request.CloseRegistrationsDate.ToUniversalTime());

        var registration = new Registration(owner, RegistrationRole.Staff, RegistrationState.Accepted, newEvent)
        {
            CheckedIn = true
        };
        newEvent.Registrations.Add(registration);
        newEvent.IsFree = request.Request.IsFree;   
        
        newEvent.Prices.Add(new EventPrice(request.Request.FirstPrice == 0 ? 1 : request.Request.FirstPrice, request.Request.CloseRegistrationsDate.ToUniversalTime()));
        
        var productName = "Event Product: " + newEvent.Name;
        var productId = await CreateProductInStripeAsync(productName);
        newEvent.ProductId = productId; 
            
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    private static async Task<string> CreateProductInStripeAsync(string productName)
    {
        var productOptions = new ProductCreateOptions
        {
            Name = productName
        };

        var productService = new ProductService();
        var product = await productService.CreateAsync(productOptions);

        return product.Id;
    }

}
    