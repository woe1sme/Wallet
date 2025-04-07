using Wallet.Domain.Models.Enums;
using Wallet.Domain.SeedWork;

namespace Wallet.Domain.Models.BaseEntity;

public abstract class FinancialBase : Entity<Guid>
{
    public FinancialBase(Guid id, decimal balance, Currency currency) : base(id)
    {
        Balance = balance;
        Currency = currency;
    }

    public decimal Balance { get; protected set; }
    public Currency Currency { get; protected set; }

    public virtual void ChangeCurrency(Currency currency)
    {
        if (Currency != currency && Balance != 0)
            throw new AggregateException("Balance must be 0");

        Currency = Currency.HasFlag(currency) ? currency : throw new AggregateException("ID must be greater than 0");
    }

    public virtual void AddMoney(decimal amount)
    {
        Balance += amount >= 0 ? amount : throw new AggregateException("Money must be greater than 0");
    }

    /// <summary>
    /// Списать деньги со счета
    /// </summary>
    /// <param name="amount">сумма</param>
    /// <exception cref="AccountException">Ошибка при выполнение списания</exception>
    public virtual void WriteOffMoney(decimal amount)
    {
        if (amount < 0)
            throw new AggregateException($"Property {nameof(amount)} cannot be negative");

        if (amount > Balance)
            throw new AggregateException($"Property {nameof(amount)} cannot be greater than balance");

        Balance -= amount;
    }
}