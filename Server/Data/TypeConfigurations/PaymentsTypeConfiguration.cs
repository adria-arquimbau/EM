using EventsManager.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Server.Data.TypeConfigurations;

public class PaymentsTypeConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder) 
    {
        builder.HasKey(x => x.Id);
        builder.Property(r => r.Result)
            .HasConversion(v => 
                v.ToString(), v => (PaymentResult)Enum.Parse(typeof(PaymentResult), v));
    }
}