using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations
{
    public class ThreadEntityTypeConfiguration : IEntityTypeConfiguration<ThreadEntity>
    {
        public void Configure(EntityTypeBuilder<ThreadEntity> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
            builder.Property(x => x.MessageCount);
            builder.Property(x => x.LastMessageId);
            builder.Property(x => x.LastMessageCreateAt);
            builder.Property(x => x.FirstMessageId);
            builder.Property(x => x.IsClosed).IsRequired();
            builder.Property(x => x.Key);

            builder.Property(x => x.CreatedAt).HasDefaultValueSql("getDate()");

            builder.ToTable($"tbl{nameof(ThreadEntity)}");
        }

    }
}

