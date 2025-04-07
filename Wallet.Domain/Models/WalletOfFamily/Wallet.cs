using Wallet.Domain.Exceptions;
using Wallet.Domain.Models.BaseEntity;
using Wallet.Domain.Models.Enums;

namespace Wallet.Domain.Models.WalletOfFamily;

/// <summary>
/// Кошелёк семьи
/// </summary>
public class Wallet : FinancialBase
{
    public Wallet(Guid id, decimal balance, Currency currency, string description)
        : base(id, balance, currency)
    {
        Description = description ?? throw new WalletException($"Property {nameof(description)} cannot be empty", new AggregateException(nameof(description)));
    }

    // Пустой конструктор
    protected Wallet() : base(default, default, default) { }
    /// <summary>
    /// Описание счета
    /// </summary>
    public string Description { get; protected set; }
    /// <summary>
    /// Семья к которой принадлежит кошелёк
    /// </summary>
    public virtual Family Family { get; protected set; }
    
    private List<SubWallet> _subWallets = [];
    /// <summary>
    /// Подкошельки
    /// </summary>
    public virtual IEnumerable<SubWallet> SubWallets => _subWallets.AsReadOnly();
    /// <summary>
    /// Есть подкошельки
    /// </summary>
    public bool IsSubWallets => _subWallets.Any();

    /// <summary>
    /// Пополнить баланс
    /// </summary>
    /// <param name="amount">сумма</param>
    /// <exception cref="WalletException">Ошибка когда сумма меньше нуля</exception>
    public override void AddMoney(decimal amount)
    {
        try
        {
            base.AddMoney(amount);
        }
        catch (Exception e)
        {
            throw new WalletException(e.Message, e);
        }
    }

    /// <summary>
    /// Списать деньги со счета
    /// </summary>
    /// <param name="amount">Сумма</param>
    /// <exception cref="WalletException">Ошибка при несоблюдение условия</exception>
    public override void WriteOffMoney(decimal amount)
    {
        try
        {
            base.WriteOffMoney(amount);
        }
        catch (Exception e)
        {
            throw new WalletException(e.Message, e);
        }
    }

    /// <summary>
    /// Создать подкошелёк кошелька
    /// </summary>
    /// <param name="defaultBalance">Баланс кошелька по умолчанию</param>
    /// <param name="description">Описание кошелка</param>
    /// <returns>Возвращает созданный подкошелёк</returns>
    /// <exception cref="WalletException">Ошибка при создании подкошелка</exception>
    public virtual SubWallet CreateSubWallet(decimal defaultBalance, string description)
    {
        if (description == string.Empty)
            throw new WalletException($"Property {nameof(description)} cannot be empty", new AggregateException(nameof(description)));

        if (defaultBalance < 0)
            throw new WalletException("Amount must be greater than 0", new AggregateException(nameof(defaultBalance)));

        if (defaultBalance > Balance)
            throw new WalletException("Amount must be less than Balance", new AggregateException(nameof(defaultBalance)));

        try
        {
            var subWallet = new SubWallet(this, Guid.NewGuid(), defaultBalance, Currency, description);
            subWallet.AddFamily(Family);
            _subWallets.Add(subWallet);
            return subWallet;
        }
        catch (Exception e)
        {
            throw new WalletException(e.Message, e);
        }
    }

    public virtual void AddFamily(Family? family)
    {
        Family = family ?? throw new WalletException($"Property {nameof(family)} cannot be null", new ArgumentNullException(nameof(family)));
    }

    public virtual void ChangeDescription(string description)
    {
        Description = description ?? throw new WalletException($"Property {nameof(description)} cannot be empty", new AggregateException(nameof(description)));
    }
}