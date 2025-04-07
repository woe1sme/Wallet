using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallet.Domain.Models.AccountOfPerson;

namespace Wallet.Infrastructure.Configurations;

internal class AccountConfiguration
    : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {

        builder.HasKey(a => a.Id); // Устанавливаем ключевое поле Id

        //builder.Property(a => a.Id)
        //    .ValueGeneratedOnAdd(); // Указываем, что Id генерируется при добавлении

        builder.Property(a => a.Balance)
            .IsRequired(); // Поле Balance обязательно

        builder.Property(a => a.Currency)
            .IsRequired(); // Поле Currency обязательно

        builder.Property(a => a.Description)
            .IsRequired()
            .HasMaxLength(255); // Максимальная длина строки Description

        builder.Property(a => a.ProfileId)
            .IsRequired(); // Поле ProfileId обязательно

    }
}
