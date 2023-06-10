using EventsManager.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Server.Data.TypeConfigurations;

public class EventPricesTypeConfiguration : IEntityTypeConfiguration<EventPrice>
{
    public void Configure(EntityTypeBuilder<EventPrice> builder) 
    {
        builder.HasKey(x => x.Id);
    }
}