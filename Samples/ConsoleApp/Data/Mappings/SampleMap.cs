using ConsoleApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sillycore.EntityFramework.Mapping;

namespace ConsoleApp.Data.Mappings
{
    public class SampleMap : EntityMappingConfiguration<Sample>
    {
        public override void Map(EntityTypeBuilder<Sample> builder)
        {
            builder.ToTable("Samples");
        }
    }
}