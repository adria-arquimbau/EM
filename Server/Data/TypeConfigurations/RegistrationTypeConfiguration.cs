using EventsManager.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Server.Data.TypeConfigurations;

public class RegistrationTypeConfiguration : IEntityTypeConfiguration<Registration>
{
    public void Configure(EntityTypeBuilder<Registration> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasOne(r => r.RegisteredUser)
            .WithMany()
            .HasForeignKey(r => r.RegisteredUserId)
            .IsRequired();
        builder.HasOne(r => r.Event)
            .WithMany()
            .HasForeignKey(r => r.EventId)
            .IsRequired();
        builder.Property(r => r.Role)
            .HasConversion(v => 
                v.ToString(), v => (RegistrationRole)Enum.Parse(typeof(RegistrationRole), v));
        builder.Property(r => r.State)
            .HasConversion(v => 
                v.ToString(), v => (RegistrationState)Enum.Parse(typeof(RegistrationState), v));
    }
}