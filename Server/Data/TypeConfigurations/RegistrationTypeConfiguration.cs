using EventsManager.Server.Models;
using EventsManager.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Server.Data.TypeConfigurations;

public class RegistrationTypeConfiguration : IEntityTypeConfiguration<Registration>
{
    public void Configure(EntityTypeBuilder<Registration> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasOne(r => r.User)
            .WithMany(u => u.Registrations)
            .HasForeignKey(r => r.UserId)
            .IsRequired();
        builder.HasOne(r => r.Event)
            .WithMany(e => e.Registrations) // Here, specify the inverse navigation property
            .IsRequired();
        builder.Property(r => r.Role)
            .HasConversion(v => 
                v.ToString(), v => (RegistrationRole)Enum.Parse(typeof(RegistrationRole), v));
        builder.Property(r => r.State)
            .HasConversion(v => 
                v.ToString(), v => (RegistrationState)Enum.Parse(typeof(RegistrationState), v));
        builder.Property(r => r.PaymentStatus)
            .HasConversion(v => 
                v.ToString(), v => (PaymentStatus)Enum.Parse(typeof(PaymentStatus), v));
        builder.HasMany(r => r.Payments)
            .WithOne(p => p.Registration)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
