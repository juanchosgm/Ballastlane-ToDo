using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDo.Domain.Entities;

namespace ToDo.Infrastructure.Persistence.Configurations;

public sealed class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        // Persist the enum by its name so the stored value stays readable and
        // stable even if the numeric ordering of the enum ever changes.
        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.DueDate);

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        // Every list/detail query is scoped by owner, so index the foreign key.
        builder.HasIndex(x => x.UserId);
    }
}
