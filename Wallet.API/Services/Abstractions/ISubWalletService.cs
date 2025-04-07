using Wallet.API.Models.Base;
using Wallet.API.Models.WalletOfFamily;

namespace Wallet.API.Services.Abstractions;

public interface ISubWalletService
{
    /// <summary>
    /// Создаёт новый подкошелёк.
    /// </summary>
    /// <param name="subWallet">Модель данных для создания нового подкошелька.</param>
    /// <param name="cancellation">Токен отмены операции.</param>
    /// <returns>Созданный подкошелёк.</returns>
    public Task<SubWalletReadModel> CreateSubWallet(SubWalletWriteModel subWallet, CancellationToken cancellation);

    /// <summary>
    /// Обновляет информацию подкошелька.
    /// </summary>
    /// <param name="id">Идентификатор подкошелька.</param>
    /// <param name="subWalletUpdateModel">Модель данных для обновления подкошелька.</param>
    /// <param name="cancellation">Токен отмены операции.</param>
    /// <returns>Обновлённый подкошелёк.</returns>
    public Task<SubWalletReadModel> UpdateSubWallet(Guid id, SubWalletWriteModel subWalletUpdateModel, CancellationToken cancellation);

    /// <summary>
    /// Удаляет подкошелёк по указанному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор подкошелька.</param>
    /// <param name="cancellation">Токен отмены операции.</param>
    /// <returns>Результат операции удаления.</returns>
    public Task<bool> DeleteSubWallet(Guid id, CancellationToken cancellation);

    /// <summary> 
    /// Переводит указанную сумму денег между основным кошельком и подкошельком. 
    /// </summary> 
    /// <param name="transferFunds">Модель данных для перевода средств.</param> 
    /// <param name="cancellation">Токен отмены операции.</param> 
    /// <returns>Задача выполнения операции перевода средств.</returns>
    public Task TransferFundsBetweenSubWalletAndWallet(SubWalletTransferFundsWriteModel transferFunds, CancellationToken cancellation);

    /// <summary>
    /// Получает все подкошельки с пагинацией
    /// </summary>
    /// <param name="pageIndex">Индекс страницы</param>
    /// <param name="pageSize">Количество записей на странице</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Список всех подкошельков с пагинацией</returns>
    public Task<PaginatedItems<SubWalletReadModel>> GetSubWallets(int pageIndex, int pageSize, CancellationToken cancellation);

    /// <summary>
    /// Получает подкошелёк по Id
    /// </summary>
    /// <param name="subWalletId">(обязательный): Id подкошелька</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Подкошелёк с указанным Id</returns>
    public Task<SubWalletReadModel> GetSubWallet(Guid subWalletId, CancellationToken cancellation);
}
