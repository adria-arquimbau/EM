using EventsManager.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Server.Data.TypeConfigurations;

public class TicketTypeConfiguration: IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder) 
    {
        builder.HasKey(x => x.Id);
    }
}
