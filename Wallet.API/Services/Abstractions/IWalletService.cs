using Wallet.API.Models.Base;
using Wallet.API.Models.WalletOfFamily;
using WalletOfFamily = Wallet.API.Models.WalletOfFamily;

namespace Wallet.API.Services.Abstractions;

/// <summary>
/// Интерфейс сервиса Кошельков
/// </summary>
public interface IWalletService
{

    /// <summary>
    /// Создает новый кошелёк
    /// </summary>
    /// <param name="newWallet">Информация о новом кошельке семьи</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Созданный кошелёк</returns>
    /// <exception cref="WalletAPIException">Непредвиденная ошибка</exception>
    public Task<WalletReadModel> CreateWallet(WalletWriteModel newWallet, CancellationToken cancellation);

    /// <summary>
    /// Получает все кошельки с пагинацией
    /// </summary>
    /// <param name="pageIndex">Индекс страницы (по умолчанию 1)</param>
    /// <param name="pageSize">Количество записей на странице (по умолчанию 10)</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Список всех кошельков с пагинацией</returns>
    /// <exception cref="WalletAPIException">Выбрасывается, если произошла ошибка при получении списка кошельков</exception>
    public Task<PaginatedItems<WalletOfFamily.WalletReadModel>> GetWallets(int pageIndex, int pageSize, CancellationToken cancellation);

    /// <summary>
    /// Получает кошелёк по Id
    /// </summary>
    /// <param name="walletId">(обязательный): Id кошелька</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Кошелёк с указанным Id</returns>
    /// <exception cref="WalletAPIException">Выбрасывается, если произошла ошибка при получении кошелька</exception>
    public Task<WalletOfFamily.WalletReadModel> GetWallet(Guid walletId, CancellationToken cancellation);

    /// <summary>
    /// Обновляет информацию по кошельку
    /// </summary>
    /// <param name="walletId">Id кошелька</param>
    /// <param name="updateWallet">Данные для обновления</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Обновлённая информация по кошельку</returns>
    /// <exception cref="WalletAPIException">Выбрасывается, если произошла ошибка при обновлении кошелька</exception>
    public Task<WalletReadModel> UpdateWallet(Guid walletId, WalletWriteModel updateWallet, CancellationToken cancellation);


    /// <summary> 
    /// Удаляет кошелёк по указанному идентификатору. 
    /// </summary> 
    /// <param name="walletId">Идентификатор удаляемого кошелька.</param> 
    /// <param name="cancellation">Токен отмены операции.</param> 
    /// <returns>Задача выполнения операции удаления кошелька.</returns>
    public Task DeleteWallet(Guid walletId, CancellationToken cancellation);

    /// <summary> 
    /// Переводит указанную сумму денег между основным кошельком и подкошельком. 
    /// </summary> 
    /// <param name="transferFunds">Модель данных для перевода средств.</param> 
    /// <param name="cancellation">Токен отмены операции.</param> 
    /// <returns>Задача выполнения операции перевода средств.</returns>
    public Task TransferFundsBetweenWalletAndSubWallet(WalletTransferFundsWriteModel transferFunds, CancellationToken cancellation);
}
