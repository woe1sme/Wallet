using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wallet.API.Applications.Exceptions;
using Wallet.API.Models.AccountOfPerson;
using Wallet.API.Models.Base;
using Wallet.API.Services.Abstractions;
using Wallet.Contracts.Account;
using Wallet.Contracts.Wallet;
using Wallet.Domain.Models.AccountOfPerson;
using Wallet.Domain.Models.WalletOfFamily;
using Wallet.Domain.Repositories;
using Wallet.Domain.SeedWork;
using WalletOfFamily = Wallet.Domain.Models.WalletOfFamily;

namespace Wallet.API.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository<Account, Guid> _accountRepository;
    private readonly IRepository<WalletOfFamily.Wallet, Guid> _walletRepository;
    private readonly IMapper _mapper;
    private readonly IPublishService _publishService;
    private readonly ILogger<AccountService> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр сервиса PersonalAccountService.
    /// </summary>
    /// <param name="accountRepository">Репозиторий для работы с персональными счетами (обязательный)</param>
    /// <param name="logger">Сервис для логирования (обязательный)</param>
    /// <param name="mapper">Сервис для маппинга объектов (обязательный)</param>
    /// <exception cref="WalletAPIException">Выбрасывается если свойство имеет NULL</exception>
    public AccountService(
        IAccountRepository<Account, Guid> accountRepository,
        IRepository<WalletOfFamily.Wallet, Guid> walletRepository,
        ILogger<AccountService> logger,
        IMapper mapper,
        IPublishService publishService)
    {
        _logger = logger ?? throw new WalletAPIException($"Property {nameof(logger)} cannot be null");
        _accountRepository = accountRepository ?? throw new WalletAPIException($"Property {nameof(accountRepository)} cannot be null");
        _walletRepository = walletRepository ?? throw new WalletAPIException($"Property {nameof(walletRepository)} cannot be null");
        _mapper = mapper ?? throw new WalletAPIException($"Property {nameof(mapper)} cannot be null");
        _publishService = publishService;
    }

    /// <summary>
    /// Создает новый персональный счет
    /// </summary>
    /// <param name="newAccount">Информация о новом персональном счете</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Созданный персональный счет</returns>
    /// <exception cref="WalletAPIException">Непредвиденная ошибка</exception>
    public async Task<AccountReadModel> CreatePersonalAccount(AccountWriteModel newAccount, CancellationToken cancellation)
    {
        try
        {
            // Проверяем, существует ли уже счет для этого профиля и валюты
            var accountIsCreated = await _accountRepository.GetAll()
                .AnyAsync(a => a.ProfileId == newAccount.ProfileId && a.Currency.ToString() == newAccount.Currency, cancellation);

            if (accountIsCreated)
                throw new WalletAPIException($"User's (ProfileId = {newAccount.ProfileId}) account already exists");

            // Создаем новый счет
            var account = _mapper.Map<Account>(newAccount);

            await _accountRepository.AddAsync(account, cancellation);
            await _accountRepository.SaveAsync(cancellation);

            await _publishService.PublishAsync(message: new AccountCreated(account.Id, account.Description, account.ProfileId));

            return _mapper.Map<AccountReadModel>(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating personal account");
            throw new WalletAPIException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Обновляет информацию по персональному счету
    /// </summary>
    /// <param name="accountId">Id персонального счета</param>
    /// <param name="updateAccount">Данные для обновления</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Обновленная информация по персональному счету</returns>
    /// <exception cref="WalletAPIException">Выбрасывается, если произошла ошибка при обновлении персонального счета</exception>
    public async Task<AccountReadModel> UpdateAccount(Guid accountId, AccountWriteModel updateAccount, CancellationToken cancellation)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(accountId, cancellation);

            _logger.LogInformation("Changing description of account with Id {accountId} to '{Description}'.", accountId, updateAccount.Description);
            account.SetDescription(updateAccount.Description);

            await _accountRepository.UpdateAsync(account);
            await _accountRepository.SaveAsync(cancellation);

            _logger.LogInformation("Successfully updated account with Id {accountId}.", accountId);

            await _publishService.PublishAsync(message: new AccountUpdated(account.Id, account.Description));

            return _mapper.Map<AccountReadModel>(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating account with Id {accountId}.", accountId);
            throw new WalletAPIException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Удаляет персональный счет по указанному идентификатору.
    /// </summary>
    /// <param name="accountId">Идентификатор удаляемого персонального счета.</param>
    /// <param name="cancellation">Токен отмены операции.</param>
    /// <returns>Задача выполнения операции удаления персонального счета.</returns>
    /// <exception cref="WalletAPIException">
    /// Выбрасывается, если баланс персонального счета больше нуля.
    /// Выбрасывается, если произошла ошибка при удалении персонального счета.
    /// </exception>
    public async Task DeleteAccount(Guid accountId, CancellationToken cancellation)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(accountId, cancellation);

            if (account.Balance > 0)
            {
                _logger.LogWarning("Cannot delete account {accountId}. It has a balance of {Balance}.", accountId, account.Balance);
                throw new WalletAPIException($"Cannot be deleted. The account (Id = {accountId}) has a balance.");
            }

            await _accountRepository.DeleteAsync(accountId, cancellation);
            await _accountRepository.SaveAsync(cancellation);

            _logger.LogInformation("Successfully deleted account {accountId}.", accountId);

            await _publishService.PublishAsync(message: new AccountDeleted(account.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting account {accountId}.", accountId);
            throw new WalletAPIException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Переводит указанную сумму денег с личного счета на кошелек семьи.
    /// </summary>
    /// <param name="transferFunds">Модель данных для перевода средств.</param>
    /// <param name="cancellation">Токен отмены операции.</param>
    /// <returns>Задача выполнения операции перевода средств.</returns>
    /// <exception cref="WalletAPIException">
    /// Выбрасывается, если на личном счете недостаточно средств.
    /// Выбрасывается, если у счета и кошелька разные валюты.
    /// Выбрасывается, если произошла ошибка при сохранении изменений.
    /// </exception>
    public async Task TransferFundsFromAccountToWallet(AccountTransferFundsWriteModel transferFunds, CancellationToken cancellation)
    {
        try
        {
            var fromAccount = await _accountRepository.GetByIdAsync(transferFunds.FromAccountId, cancellation);
            var toWallet = await _accountRepository.GetSameWalletById<WalletOfFamily.Wallet>(transferFunds.ToWalletId, cancellation);

            // Проверка валюты
            if (fromAccount.Currency != toWallet.Currency)
            {
                _logger.LogWarning("Cannot transfer funds between account and wallet with different currencies.");
                throw new WalletAPIException("Cannot transfer funds between account and wallet with different currencies.");
            }

            _logger.LogInformation("Writing off {amount} from account {fromAccountId}.", transferFunds.Amount, transferFunds.FromAccountId);
            fromAccount.WriteOffMoney(transferFunds.Amount, fromAccount.ProfileId);

            _logger.LogInformation("Adding {amount} to wallet {toWalletId}.", transferFunds.Amount, transferFunds.ToWalletId);
            toWallet.AddMoney(transferFunds.Amount);

            await _accountRepository.UpdateAsync(fromAccount);
            await _walletRepository.UpdateAsync(toWallet);
            await _accountRepository.SaveAsync(cancellation);
            await _walletRepository.SaveAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while transferring funds from account to wallet.");
            throw new WalletAPIException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Добавляет деньги на личный счёт
    /// </summary>
    /// <param name="addFundsToAccount">Модель, содержащая информацию для добавления денег на счёт</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Задача выполнения операции добавления денег</returns>
    /// <exception cref="WalletAPIException">Выбрасывается, если произошла ошибка при добавлении денег или если профиль не соответствует владельцу счёта</exception>
    public async Task AddFundsToAccount(AddFundsToAccountWriteModel addFundsToAccount, CancellationToken cancellation)
    {
        try
        {
            // Получаем счёт по ID
            var account = await _accountRepository.GetByIdAsync(addFundsToAccount.AccountId, cancellation);

            // Проверяем, что профиль соответствует владельцу счёта
            if (account.ProfileId != addFundsToAccount.AccountId)
            {
                _logger.LogWarning("Profile Id mismatch. Account Id = {accountId}, Profile Id = {profileId}, Account Profile Id = {accountProfileId}", addFundsToAccount.AccountId, addFundsToAccount.ProfileId, account.ProfileId);
                throw new WalletAPIException("Only the account owner can add funds to this account.");
            }

            // Добавляем деньги на счёт
            _logger.LogInformation("Adding {amount} to account {accountId}.", addFundsToAccount.Amount, addFundsToAccount.AccountId);
            account.AddMoney(addFundsToAccount.Amount);

            await _accountRepository.SaveAsync(cancellation);
            _logger.LogInformation("Successfully added {amount} to account {accountId}.", addFundsToAccount.Amount, addFundsToAccount.AccountId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding funds to account {accountId}.", addFundsToAccount.AccountId);
            throw new WalletAPIException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Получает все персональные счета с пагинацией
    /// </summary>
    /// <param name="pageIndex">(обязательный): Индекс страницы (по умолчанию 1)</param>
    /// <param name="pageSize">(обязательный): Количество записей на странице (по умолчанию 10)</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Список всех персональных счетов с пагинацией</returns>
    /// <exception cref="WalletAPIException">Выбрасывается, если произошла ошибка при получении списка персональных счетов</exception>
    public async Task<PaginatedItems<AccountReadModel>> GetAccounts(int pageIndex, int pageSize, CancellationToken cancellation)
    {
        _logger.LogInformation("Fetching accounts with pagination. PageIndex: {pageIndex}, PageSize: {pageSize}.", pageIndex, pageSize);
        try
        {
            var accounts = _accountRepository.GetAll();

            var accountsPaginated = await accounts
                .OrderBy(a => a.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellation);

            bool hasMoreItems = await accounts.Skip(pageIndex * pageSize)
                .AnyAsync(cancellation);

            var items = _mapper.Map<List<AccountReadModel>>(accountsPaginated);

            _logger.LogInformation("Successfully fetched {iCount} accounts for PageIndex: {pageIndex}, PageSize: {pageSize}.", items.Count, pageIndex, pageSize);
            return new PaginatedItems<AccountReadModel>(items, pageIndex, items.Count, hasMoreItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching accounts with pagination.");
            throw new WalletAPIException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Получает персональный счёт по Id
    /// </summary>
    /// <param name="accountId">(обязательный): Id персонального счёта</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Персональный счёт с указанным Id</returns>
    /// <exception cref="WalletAPIException">Выбрасывается, если произошла ошибка при получении персонального счёта</exception>
    public async Task<AccountReadModel> GetAccount(Guid accountId, CancellationToken cancellation)
    {
        try
        {
            var accountDb = await _accountRepository.GetByIdAsync(accountId, cancellation);

            _logger.LogInformation("Successfully fetched account with Id {accountId}.", accountId);
            return _mapper.Map<AccountReadModel>(accountDb);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching account with Id {accountId}.", accountId);
            throw new WalletAPIException(ex.Message, ex);
        }
    }


}

