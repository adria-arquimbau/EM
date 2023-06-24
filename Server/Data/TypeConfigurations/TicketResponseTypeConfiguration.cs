using EventsManager.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Server.Data.TypeConfigurations;

public class TicketResponseTypeConfiguration: IEntityTypeConfiguration<TicketResponse>
{
    public void Configure(EntityTypeBuilder<TicketResponse> builder) 
    {
        builder.HasKey(x => x.Id);
    }
}