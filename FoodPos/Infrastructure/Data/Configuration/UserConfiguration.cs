using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class UserConfiguration
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("User");
            builder.Property(p => p.Id)
                    .IsRequired();
            builder.Property(p => p.Names)
                    .IsRequired()
                    .HasMaxLength(200);
            builder.Property(p => p.FirstSurname)
                    .IsRequired()
                    .HasMaxLength(200);
            builder.Property(p => p.LastSurname)
                    .IsRequired()
                    .HasMaxLength(200);
            builder.Property(p => p.Email)
                    .IsRequired()
                    .HasMaxLength(200);

            builder
            .HasMany(p => p.Roles)
            .WithMany(p => p.Users)
            .UsingEntity<UserRoles>(
                j => j
                    .HasOne(pt => pt.Role)
                    .WithMany(t => t.UserRoles)
                    .HasForeignKey(pt => pt.RoleId),
                j => j
                    .HasOne(pt => pt.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(pt => pt.UserId),
                j =>
                {
                    j.HasKey(t => new { t.UserId, t.RoleId });
                });

        }
    }

}
