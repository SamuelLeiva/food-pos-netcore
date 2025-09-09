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
            builder.Property(u => u.Id)
                    .IsRequired();
            builder.Property(u => u.Names)
                    .IsRequired()
                    .HasMaxLength(200);
            builder.Property(u => u.FirstSurname)
                    .IsRequired()
                    .HasMaxLength(200);
            builder.Property(u => u.LastSurname)
                    .IsRequired()
                    .HasMaxLength(200);
            builder.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(200);

            builder
            .HasMany(u => u.Roles)
            .WithMany(u => u.Users)
            .UsingEntity<UserRoles>(
                ur => ur
                    .HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId),
                ur => ur
                    .HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId),
                ur =>
                {
                    ur.HasKey(t => new { t.UserId, t.RoleId });
                });

            builder
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId);

        }
    }

}
