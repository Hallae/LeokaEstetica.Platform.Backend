using LeokaEstetica.Platform.Models.Entities.Moderation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeokaEstetica.Platform.Models.Mappings.Moderation;

public partial class VacancyRemarkConfiguration : IEntityTypeConfiguration<VacancyRemarkEntity>
{
     public void Configure(EntityTypeBuilder<VacancyRemarkEntity> entity)
    {
        entity.ToTable("VacanciesRemarks", "Moderation");

        entity.HasKey(e => e.RemarkId);

        entity.Property(e => e.RemarkId)
            .HasColumnName("RemarkId")
            .HasColumnType("bigserial")
            .IsRequired();

        entity.Property(e => e.VacancyId)
            .HasColumnName("VacancyId")
            .HasColumnType("bigint")
            .IsRequired();

        entity.Property(e => e.FieldName)
            .HasColumnName("FieldName")
            .HasColumnType("varchar(150)")
            .HasMaxLength(150)
            .IsRequired();
        
        entity.Property(e => e.RemarkText)
            .HasColumnName("RemarkText")
            .HasColumnType("varchar(500)")
            .HasMaxLength(500)
            .IsRequired();
        
        entity.Property(e => e.RussianName)
            .HasColumnName("RussianName")
            .HasColumnType("varchar(500)")
            .HasMaxLength(500)
            .IsRequired();
        
        entity.Property(e => e.ModerationUserId)
            .HasColumnName("ModerationUserId")
            .HasColumnType("int")
            .IsRequired();
        
        entity.Property(e => e.DateCreated)
            .HasColumnName("DateCreated")
            .HasColumnType("timestamp")
            .IsRequired();
        
        entity.Property(e => e.RemarkStatusId)
            .HasColumnName("RemarkStatusId")
            .HasColumnType("int")
            .IsRequired();

        entity.HasIndex(u => u.RemarkId)
            .HasDatabaseName("PK_RemarkId")
            .IsUnique();
        
        entity.HasOne(p => p.UserProject)
            .WithMany(b => b.VacancyRemarks)
            .HasForeignKey(p => p.VacancyId)
            .HasConstraintName("FK_Vacancies_UserVacancies_VacancyId");
        
        entity.HasOne(p => p.ModerationUser)
            .WithMany(b => b.VacancyRemarks)
            .HasForeignKey(p => p.ModerationUserId)
            .HasConstraintName("FK_Users_UserId_ModerationUserId");
        
        entity.HasOne(p => p.RemarkStatuse)
            .WithMany(b => b.VacancyRemarks)
            .HasForeignKey(p => p.RemarkStatusId)
            .HasConstraintName("FK_Moderation_RemarksStatuses_RemarkStatusId");

        OnConfigurePartial(entity);
    }

    partial void OnConfigurePartial(EntityTypeBuilder<VacancyRemarkEntity> entity);
}