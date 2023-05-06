using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations
{
    public class DirectMessageEntityTypeConfiguration : IEntityTypeConfiguration<MessageEntity>
    {
        public void Configure(EntityTypeBuilder<MessageEntity> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
            builder.Property(x => x.ThreadId).IsRequired();
            builder.Property(x => x.Body).IsRequired();
            builder.Property(x => x.OwnerId).IsRequired();
            builder.Property(x => x.HasReplay).HasDefaultValue(false);
            builder.Property(x => x.Seen).HasDefaultValue(false);
            builder.Property(x => x.ReplayMessageId);


            builder.Property(x => x.CreatedAt).HasDefaultValueSql("getDate()");

            builder.ToTable($"tbl{nameof(MessageEntity)}");
        }
    }
}

