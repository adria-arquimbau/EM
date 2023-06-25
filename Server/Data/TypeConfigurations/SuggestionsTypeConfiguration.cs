using EventsManager.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Server.Data.TypeConfigurations;

public class SuggestionsTypeConfiguration : IEntityTypeConfiguration<Suggestion>
{
    public void Configure(EntityTypeBuilder<Suggestion> builder) 
    {
        builder.HasKey(x => x.Id);
    }
}