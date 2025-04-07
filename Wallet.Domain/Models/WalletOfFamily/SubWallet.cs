using Wallet.Domain.Exceptions;
using Wallet.Domain.Models.Enums;

namespace Wallet.Domain.Models.WalletOfFamily;

public class SubWallet : Wallet
{
    public SubWallet(Wallet parentWallet, Guid id, decimal balance, Currency currency, string description)
        : base(id, balance, currency, description)
    {
        ParentWallet = parentWallet ?? throw new SubWalletException($"Property {nameof(parentWallet)} cannot be null", new ArgumentNullException(nameof(parentWallet)));
        _familyMembers = new List<FamilyMember>();
    }
    public SubWallet(Guid id, decimal balance, Currency currency, string description)
        : base(id, balance, currency, description)
    {
        _familyMembers = new List<FamilyMember>();
    }

    // Пустой конструктор
    protected SubWallet() : base() { }

    /// <summary>
    /// Родительский кошелёк
    /// </summary>
    public virtual Wallet ParentWallet { get; protected set; }
    /// <summary>
    /// Персональный кошелёк
    /// </summary>
    public bool IsPrivateWallet => _familyMembers.Any();
    private List<FamilyMember> _familyMembers = [];
    /// <summary>
    /// Члены семьи которые могут использовать кошелёк
    /// </summary>
    public virtual IEnumerable<FamilyMember> FamilyMembers => _familyMembers.AsReadOnly();

    /// <summary>
    /// Добавить пользователя кошелька
    /// </summary>
    /// <param name="newOwner">Новый член семьи</param>
    /// <exception cref="SubWalletException">Ошибка когда новый пользователь равен null</exception>
    public void AddFamilyMember(FamilyMember newOwner)
    {
        if (newOwner is null)
            throw new SubWalletException($"Property {nameof(newOwner)} cannot be null", new ArgumentNullException(nameof(newOwner)));

        if (!_familyMembers.Contains(newOwner))
        {
            _familyMembers.Add(newOwner);
        }
    }

    /// <summary>
    /// Удалить пользователя кошелька
    /// </summary>
    /// <param name="owner">Член семьи</param>
    /// <exception cref="SubWalletException">Ошибка когда пользователь равен null</exception>
    public void RemoveFamilyMember(FamilyMember owner)
    {
        if (owner is null)
            throw new SubWalletException($"Property {nameof(owner)} cannot be null", new ArgumentNullException(nameof(owner)));

        _familyMembers.Remove(owner);
    }
    /// <summary>
    /// Списать деньги со счёта
    /// </summary>
    /// <param name="amount">сумма</param>
    /// <exception cref="SubWalletException">Ошибка когда подкошелёк имеет пользователей</exception>
    public override void WriteOffMoney(decimal amount)
    {
        if (IsPrivateWallet)
            throw new SubWalletException($"Sub wallet {Description} is private");

        try
        {
            base.WriteOffMoney(amount);
        }
        catch (WalletException e)
        {
            throw new SubWalletException(e.Message, e);
        }
    }

    /// <summary>
    /// Списать деньги со счёта
    /// </summary>
    /// <param name="amount">Сумма</param>
    /// <param name="owner">Пользователь кошелька</param>
    /// <exception cref="SubWalletException">Ошибка когда пользователь не прошел проверку</exception>
    public virtual void WriteOffMoney(decimal amount, FamilyMember? owner)
    {
        if (!IsPrivateWallet)
            throw new SubWalletException($"Sub wallet {Description} is not private");

        if (owner is null)
            throw new SubWalletException($"Property {nameof(owner)} cannot be null", new ArgumentNullException(nameof(owner)));

        if (!_familyMembers.Contains(owner))
            throw new SubWalletException($"Member {owner.Name} cannot use the wallet");

        try
        {
            base.WriteOffMoney(amount);
        }
        catch (WalletException e)
        {
            throw new SubWalletException(e.Message, e);
        }
    }
}