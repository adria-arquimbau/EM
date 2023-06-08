using EventsManager.Server.Models;
using EventsManager.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Server.Data.TypeConfigurations;

public class RegistrationRolePasswordTypeConfiguration : IEntityTypeConfiguration<RegistrationRolePassword>
{
    public void Configure(EntityTypeBuilder<RegistrationRolePassword> builder) 
    {
        builder.HasKey(x => x.Id);
        builder.Property(r => r.Role)
            .HasConversion(v => 
                v.ToString(), v => (RegistrationRole)Enum.Parse(typeof(RegistrationRole), v));
    }
}