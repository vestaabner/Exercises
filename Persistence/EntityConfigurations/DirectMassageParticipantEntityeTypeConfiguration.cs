using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations
{
    public class DirectMassageParticipantEntityeTypeConfiguration : IEntityTypeConfiguration<ParticipantEntity>
    {

        public void Configure(EntityTypeBuilder<ParticipantEntity> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
            builder.Property(x => x.ThreadId).IsRequired();
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.Role).HasDefaultValue(ParticipantRoleEnum.Member);
            builder.Property(x => x.LastSeenAt);

            builder.Property(x => x.CreatedAt).HasDefaultValueSql("getDate()");

            builder.ToTable($"tbl{nameof(ParticipantEntity)}");
        }

    }
}

