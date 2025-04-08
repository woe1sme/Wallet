using Wallet.API.Models.WalletOfFamily;
using Wallet.API.Services.Abstractions;
using WalletOfFamily = Wallet.Domain.Models.WalletOfFamily;
using Wallet.Domain.SeedWork;
using AutoMapper;
using Wallet.API.Applications.Exceptions;
using Wallet.Domain.Repositories;
using Wallet.Domain.Models.WalletOfFamily;
using Microsoft.EntityFrameworkCore;
using Wallet.API.Models.Base;
using Wallet.Contracts.Wallet;

namespace Wallet.API.Services;

public class SubWalletService : ISubWalletService
{
    private readonly ISubWalletRepository<SubWallet, Guid> _subWalletRepository;
    private readonly IRepository<WalletOfFamily.Wallet, Guid> _walletRepository;
    private readonly IMapper _mapper;
    private readonly IPublishService _publishService;
    private readonly ILogger<SubWalletService> _logger;

    public SubWalletService(
        ISubWalletRepository<SubWallet, Guid> subWalletRepository,
        IRepository<WalletOfFamily.Wallet, Guid> walletRepository,
        ILogger<SubWalletService> logger,
        IMapper mapper,
        IPublishService publishService)
    {
        _logger = logger ?? throw new WalletAPIException($"Property {nameof(logger)} cannot be null");
        _subWalletRepository = subWalletRepository ?? throw new WalletAPIException($"Property {nameof(subWalletRepository)} cannot be null");
        _walletRepository = walletRepository ?? throw new WalletAPIException($"Property {nameof(walletRepository)} cannot be null");
        _mapper = mapper ?? throw new WalletAPIException($"Property {nameof(mapper)} cannot be null");
        _publishService = publishService;
    }

    /// <summary>
    /// Создаёт новый подкошелёк.
    /// </summary>
    /// <param name="subWallet">Модель данных для создания нового подкошелька.</param>
    /// <param name="cancellation">Токен отмены операции.</param>
    /// <returns>Созданный подкошелёк.</returns>
    /// <exception cref="WalletAPIException">
    /// Выбрасывается, если произошла ошибка при создании подкошелька.
    /// </exception>
    public async Task<SubWalletReadModel> CreateSubWallet(SubWalletWriteModel subWallet, CancellationToken cancellation)
    {
        _logger.LogInformation("Attempting to create a new sub-wallet for parent wallet ID {ParentWalletId}.", subWallet.ParentWalletId);

        try
        {
            // Проверка существования родительского кошелька
            var parentWallet = await _subWalletRepository.GetParentWalletById<WalletOfFamily.Wallet>(subWallet.ParentWalletId, cancellation);

            // Проверка на соответствие семьи
            if (parentWallet.Family.Id != subWallet.Family.Id)
            {
                _logger.LogWarning("Family with ID {FamilyId} was not found.", parentWallet.Family.Id);
                throw new WalletAPIException($"Family with ID {parentWallet.Family.Id} not found.");
            }

            // Проверка валюты
            if (parentWallet.Currency.ToString() != subWallet.Currency)
            {
                _logger.LogWarning("Currency mismatch: Parent wallet currency is {ParentWalletCurrency}, Sub-wallet currency is {SubWalletCurrency}.", (int)parentWallet.Currency, subWallet.Currency);
                throw new WalletAPIException("Currency mismatch between parent wallet and sub-wallet.");
            }

            // Создание нового подкошелька
            var newSubWallet = parentWallet.CreateSubWallet(subWallet.Balance, subWallet.Description);
            _logger.LogInformation("Created a new sub-wallet with balance {Balance} and description '{Description}' for parent wallet ID {ParentWalletId}.", subWallet.Balance, subWallet.Description, subWallet.ParentWalletId);

            // Добавление членов семьи к подкошельку
            var familyMembers = _mapper.Map<List<WalletOfFamily.FamilyMember>>(subWallet.FamilyMembers);
            _logger.LogInformation("Adding {Count} family members to the new sub-wallet.", familyMembers.Count);
            foreach (var familyMember in familyMembers)
            {
                newSubWallet.AddFamilyMember(familyMember);
                _logger.LogInformation("Added family member with ID {FamilyMemberId} to the new sub-wallet.", familyMember.Id);
            }

            // Сохранение подкошелька в репозитории
            await _subWalletRepository.AddAsync(newSubWallet, cancellation);
            await _subWalletRepository.SaveAsync(cancellation);

            await _publishService.PublishAsync(message: new WalletCreated(newSubWallet.Id, newSubWallet.Description));

            _logger.LogInformation("New sub-wallet successfully added to the repository and saved.");

            return _mapper.Map<SubWalletReadModel>(newSubWallet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new sub-wallet.");
            throw new WalletAPIException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Обновляет информацию подкошелька.
    /// </summary>
    /// <param name="id">Идентификатор подкошелька.</param>
    /// <param name="subWalletUpdateModel">Модель данных для обновления подкошелька.</param>
    /// <param name="cancellation">Токен отмены операции.</param>
    /// <returns>Обновлённый подкошелёк.</returns>
    /// <exception cref="WalletAPIException">
    /// Выбрасывается, если произошла ошибка при обновлении подкошелька.
    /// </exception>
    public async Task<SubWalletReadModel> UpdateSubWallet(Guid id, SubWalletWriteModel subWalletUpdateModel, CancellationToken cancellation)
    {
        _logger.LogInformation("Attempting to update sub-wallet with ID {SubWalletId}.", id);

        try
        {
            // Получение существующего подкошелька
            var subWallet = await _subWalletRepository.GetByIdAsync(id, cancellation);

            // Обновление данных подкошелька (кроме семьи, баланса, родительского кошелька)
            if (!string.IsNullOrWhiteSpace(subWalletUpdateModel.Description))
            {
                subWallet.ChangeDescription(subWalletUpdateModel.Description);
                _logger.LogInformation("Updated description for sub-wallet with ID {SubWalletId}.", id);
            }

            var familyMembers = _mapper.Map<List<FamilyMember>>(subWalletUpdateModel.FamilyMembers);
            // Обновляем членов семьи
            if (familyMembers?.Count > 0)
            {
                var newMembers = familyMembers.Where(fm => !subWallet.FamilyMembers.Select(swfm => swfm.Id).Contains(fm.Id));
                var removedMembers = subWallet.FamilyMembers.Where(swfm => !familyMembers.Select(fm => fm.Id).Contains(swfm.Id));
                foreach (var member in removedMembers)
                {
                    subWallet.RemoveFamilyMember(member);
                    _logger.LogInformation("Removed family member with ID {FamilyMemberId} from sub-wallet with ID {SubWalletId}.", member.Id, id);
                }

                foreach (var member in newMembers)
                {
                    subWallet.AddFamilyMember(member);
                    _logger.LogInformation("Added family member with ID {FamilyMemberId} to sub-wallet with ID {SubWalletId}.", member.Id, id);
                }
            }

            // Сохранение обновленного подкошелька
            await _subWalletRepository.UpdateAsync(subWallet);
            await _subWalletRepository.SaveAsync(cancellation);
            _logger.LogInformation("Successfully updated and saved sub-wallet with ID {SubWalletId}.", id);

            await _publishService.PublishAsync(message: new WalletUpdated(subWallet.Id, subWallet.Description));

            return _mapper.Map<SubWalletReadModel>(subWallet);
        }
        catch (WalletAPIException ex)
        {
            _logger.LogWarning(ex, "WalletAPIException: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the sub-wallet.");
            throw new WalletAPIException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Удаляет подкошелёк по указанному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор подкошелька.</param>
    /// <param name="cancellation">Токен отмены операции.</param>
    /// <returns>Результат операции удаления.</returns>
    public async Task<bool> DeleteSubWallet(Guid id, CancellationToken cancellation)
    {
        _logger.LogInformation("Attempting to delete sub-wallet with ID {id}.", id);

        try
        {
            var subWallet = await _subWalletRepository.GetByIdAsync(id, cancellation);

            if (subWallet.Balance != 0)
            {
                _logger.LogWarning("Sub-wallet with ID {id} cannot be deleted. Balance is not zero.", id);
                throw new WalletAPIException("Sub-wallet cannot be deleted. Balance is not zero.");
            }

            if (subWallet.IsSubWallets)
            {
                _logger.LogWarning("Sub-wallet with ID {id} cannot be deleted. It has sub-wallets.", id);
                throw new WalletAPIException("Sub-wallet cannot be deleted. The sub-wallet has sub-wallets.");
            }

            await _subWalletRepository.DeleteAsync(id, cancellation);
            await _subWalletRepository.SaveAsync(cancellation);

            _logger.LogInformation("Deleted sub-wallet with ID {id} successfully.", id);

            await _publishService.PublishAsync(message: new WalletDeleted(subWallet.Id));

            return true;
        }
        catch (WalletAPIException ex)
        {
            _logger.LogWarning(ex, "WalletAPIException encountered while deleting sub-wallet with ID {id}: {message}", id, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while deleting sub-wallet with ID {id}.", id); // Логирование
            throw new WalletAPIException(ex.Message, ex);
        }
    }

    /// <summary> 
    /// Переводит указанную сумму денег между основным кошельком и подкошельком. 
    /// </summary> 
    /// <param name="transferFunds">Модель данных для перевода средств.</param> 
    /// <param name="cancellation">Токен отмены операции.</param> 
    /// <returns>Задача выполнения операции перевода средств.</returns> 
    /// <exception cref="WalletAPIException"> 
    /// Выбрасывается, если кошелёк-отправитель и кошелёк-получатель принадлежат разным семьям. 
    /// Выбрасывается, если на кошельке-отправителе недостаточно средств. 
    /// Выбрасывается, если у кошельков разные валюты. 
    /// Выбрасывается, если произошла ошибка при сохранении изменений.
    /// </exception>
    public async Task TransferFundsBetweenSubWalletAndWallet(SubWalletTransferFundsWriteModel transferFunds, CancellationToken cancellation)
    {
        try
        {
            var fromSubWallet = await _subWalletRepository.GetByIdAsync(transferFunds.FromWalletId, cancellation);
            var toWallet = await _subWalletRepository.GetParentWalletById<WalletOfFamily.Wallet>(transferFunds.ToWalletId, cancellation);

            // Проверка на соответствие семей
            if (fromSubWallet.Family.Id != toWallet.Family.Id)
            {
                _logger.LogWarning("Cannot transfer funds between wallets of different families. FromSubWallet's Family Id = {fromFamilyId}, ToWallet's Family Id = {toFamilyId}", fromSubWallet.Family.Id, toWallet.Family.Id);
                throw new WalletAPIException($"Cannot transfer funds between wallets of different families. FromWallet's Family Id = {fromSubWallet.Family.Id}, ToWallet's Family Id = {toWallet.Family.Id}");
            }

            // Проверка валюты
            if (fromSubWallet.Currency != toWallet.Currency)
            {
                _logger.LogWarning("Cannot transfer funds between wallets with different currencies.");
                throw new WalletAPIException("Cannot transfer funds between wallets with different currencies.");
            }

            FamilyMember? owner = fromSubWallet.Family.HeadMember.Id == transferFunds.FamilyMemberId
                ? fromSubWallet.Family.HeadMember
                : fromSubWallet.FamilyMembers.FirstOrDefault(fm => fm.Id == transferFunds.FamilyMemberId);

            _logger.LogInformation("Writing off {amount} from subwallet {fromWalletId}.", transferFunds.Amount, transferFunds.FromWalletId);
            fromSubWallet.WriteOffMoney(transferFunds.Amount, owner);

            _logger.LogInformation("Adding {amount} to wallet {toWalletId}.", transferFunds.Amount, transferFunds.ToWalletId);
            toWallet.AddMoney(transferFunds.Amount);

            await _subWalletRepository.UpdateAsync(fromSubWallet);
            await _walletRepository.UpdateAsync(toWallet);
            await _subWalletRepository.SaveAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while transferring funds between wallets.");
            throw new WalletAPIException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Получает все подкошельки с пагинацией
    /// </summary>
    /// <param name="pageIndex">(обязательный): Индекс страницы (по умолчанию 1)</param>
    /// <param name="pageSize">(обязательный): Количество записей на странице (по умолчанию 10)</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Список всех подкошельков с пагинацией</returns>
    /// <exception cref="WalletAPIException">Выбрасывается, если произошла ошибка при получении списка подкошельков</exception>
    public async Task<PaginatedItems<SubWalletReadModel>> GetSubWallets(int pageIndex, int pageSize, CancellationToken cancellation)
    {
        _logger.LogInformation("Fetching subwallets with pagination. PageIndex: {pageIndex}, PageSize: {pageSize}.", pageIndex, pageSize);
        try
        {
            var subWallets = _subWalletRepository.GetAll();

            var subWalletsPaginated = await subWallets
                .OrderBy(sw => sw.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellation);

            bool hasMoreItems = await subWallets.Skip(pageIndex * pageSize)
                .AnyAsync(cancellation);

            var items = _mapper.Map<List<SubWalletReadModel>>(subWalletsPaginated);

            _logger.LogInformation("Successfully fetched {iCount} subwallets for PageIndex: {pageIndex}, PageSize: {pageSize}.", items.Count, pageIndex, pageSize);
            return new PaginatedItems<SubWalletReadModel>(items, pageIndex, items.Count, hasMoreItems);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching subwallets with pagination.");
            throw new WalletAPIException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Получает подкошелёк по Id
    /// </summary>
    /// <param name="subWalletId">(обязательный): Id подкошелька</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Подкошелёк с указанным Id</returns>
    /// <exception cref="WalletAPIException">Выбрасывается, если произошла ошибка при получении подкошелька</exception>
    public async Task<SubWalletReadModel> GetSubWallet(Guid subWalletId, CancellationToken cancellation)
    {
        try
        {
            var subWalletDb = await _subWalletRepository.GetByIdAsync(subWalletId, cancellation);
            _logger.LogInformation("Successfully fetched subwallet with Id {subWalletId}.", subWalletId);
            return _mapper.Map<SubWalletReadModel>(subWalletDb);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching subwallet with Id {subWalletId}.", subWalletId);
            throw new WalletAPIException(ex.Message, ex);
        }
    }
}
