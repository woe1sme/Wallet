using Wallet.Domain.Exceptions;
using Wallet.Domain.Models.BaseEntity;
using Wallet.Domain.Models.Enums;

namespace Wallet.Domain.Models.AccountOfPerson;

public class Account : FinancialBase
{
    public Account(Guid id, decimal balance, Currency currency, string description, Guid profileId) : base(id, balance, currency)
    {
        Description = description ?? throw new AccountException($"Property {nameof(description)} cannot be empty", new AggregateException(nameof(description)));
        ProfileId = profileId != Guid.Empty ? profileId : throw new AccountException($"Property {nameof(profileId)} must be greater than 0", new AggregateException(nameof(description)));
    }

    // Пустой конструктор
    protected Account() : base(default, default, default) { }

    /// <summary>
    /// Описание счета пользователя
    /// </summary>
    public string Description { get; protected set; }

    /// <summary>
    ///  Id профиля пользователя
    /// </summary>
    public Guid ProfileId { get; protected set; }

    /// <summary>
    /// Пополнить счет
    /// </summary>
    /// <param name="amount">сумма</param>
    /// <exception cref="AccountException">Ошибка когда не выполнены условия</exception>
    public override void AddMoney(decimal amount)
    {
        try
        {
            base.AddMoney(amount);
        }
        catch (Exception e)
        {
            throw new AccountException(e.Message, e);
        }
    }

    public void SetDescription(string description)
    {
        Description = description;
    }

    /// <summary>
    /// Списать деньги со счета
    /// </summary>
    /// <param name="amount">сумма</param>
    /// <param name="profileId">Id пользователя делающего списание</param>
    /// <exception cref="AccountException">Ошибка при выполнение списания</exception>
    public virtual void WriteOffMoney(decimal amount, Guid profileId)
    {
        if (ProfileId != profileId)
            throw new AccountException("Profile id mismatch");

        if (amount < 0)
            throw new AccountException($"Property {nameof(amount)} cannot be negative", new AggregateException(nameof(amount)));

        if (amount > Balance)
            throw new AccountException($"Property {nameof(amount)} cannot be greater than balance", new AggregateException(nameof(amount)));

        Balance -= amount;
    }
}