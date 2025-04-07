using Wallet.API.Models.Base;
using Wallet.API.Models.AccountOfPerson;

namespace Wallet.API.Services.Abstractions;

/// <summary>
/// Интерфейс сервиса Персональных счетов
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Создает новый персональный счёт
    /// </summary>
    /// <param name="newAccount">Информация о новом персональном счёте</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Созданный персональный счёт</returns>
    public Task<AccountReadModel> CreatePersonalAccount(AccountWriteModel newAccount, CancellationToken cancellation);

    /// <summary>
    /// Получает все персональные счета с пагинацией
    /// </summary>
    /// <param name="pageIndex">Индекс страницы</param>
    /// <param name="pageSize">Количество записей на странице</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Список всех персональных счетов с пагинацией</returns>
    public Task<PaginatedItems<AccountReadModel>> GetAccounts(int pageIndex, int pageSize, CancellationToken cancellation);

    /// <summary>
    /// Получает персональный счёт по Id
    /// </summary>
    /// <param name="accountId">Id персонального счёта</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Персональный счёт с указанным Id</returns>
    public Task<AccountReadModel> GetAccount(Guid accountId, CancellationToken cancellation);

    /// <summary>
    /// Обновляет информацию по персональному счёту
    /// </summary>
    /// <param name="accountId">Id персонального счёта</param>
    /// <param name="updateAccount">Данные для обновления</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Обновленная информация по персональному счёту</returns>
    public Task<AccountReadModel> UpdateAccount(Guid accountId, AccountWriteModel updateAccount, CancellationToken cancellation);

    /// <summary> 
    /// Удаляет персональный счёт по указанному идентификатору. 
    /// </summary> 
    /// <param name="accountId">Идентификатор удаляемого персонального счёта.</param> 
    /// <param name="cancellation">Токен отмены операции.</param> 
    /// <returns>Задача выполнения операции удаления персонального счёта.</returns>
    public Task DeleteAccount(Guid accountId, CancellationToken cancellation);

    /// <summary> 
    /// Переводит указанную сумму денег между персональными счетами. 
    /// </summary> 
    /// <param name="transferFunds">Модель данных для перевода средств.</param> 
    /// <param name="cancellation">Токен отмены операции.</param> 
    /// <returns>Задача выполнения операции перевода средств.</returns>
    public Task TransferFundsFromAccountToWallet(AccountTransferFundsWriteModel transferFunds, CancellationToken cancellation);

    /// <summary> 
    /// Добавляет деньги на личный счёт
    /// </summary> 
    /// <param name="addFundsToAccount">Модель данных для добавления средств</param> 
    /// <param name="cancellation">Токен отмены операции</param> 
    /// <exception cref="WalletAPIException">Выбрасывается, если произошла ошибка при добавлении денег или если профиль не соответствует владельцу счёта</exception>
    /// <returns>Задача выполнения операции добавления денег</returns>
    public Task AddFundsToAccount(AddFundsToAccountWriteModel addFundsToAccount, CancellationToken cancellation);
}

