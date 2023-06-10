using EventsManager.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Server.Data.TypeConfigurations;

public class EventTypeConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasMany(e => e.Owners)
            .WithMany(u => u.OwnedEvents)
            .UsingEntity(j => j.ToTable("UserEventOwner"));
        builder.HasOne(x => x.Creator)
            .WithMany(u => u.CreatorEvents)
            .HasForeignKey(x => x.CreatorId) // Assuming CreatorId exists in your Event model
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}