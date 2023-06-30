using Duende.IdentityServer.EntityFramework.Options;
using EventsManager.Server.Data.TypeConfigurations;
using EventsManager.Server.Models;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventsManager.Server.Data;

public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
{
    public DbSet<Event> Events { get; set; }
    public DbSet<RegistrationRolePassword> RegistrationRolePasswords { get; set; }
    public DbSet<Registration> Registrations { get; set; }
    public override DbSet<ApplicationUser> Users { get; set; }  
    public DbSet<Suggestion> Suggestions { get; set; }  
    public DbSet<EventPrice> EventPrices { get; set; }  
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketResponse> TicketResponses { get; set; }
    public DbSet<Payment> Payments { get; set; }
    
    public ApplicationDbContext(DbContextOptions options, IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
    {
    }
        
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new EventTypeConfiguration());
        modelBuilder.ApplyConfiguration(new RegistrationTypeConfiguration());
        modelBuilder.ApplyConfiguration(new RegistrationRolePasswordTypeConfiguration());
        modelBuilder.ApplyConfiguration(new EventPricesTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TicketTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TicketResponseTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SuggestionsTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentsTypeConfiguration());
        
        modelBuilder.Entity<Event>()
            .HasQueryFilter(entity => !entity.IsDeleted);
    }
}