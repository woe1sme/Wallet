using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wallet.API.Applications.Exceptions;
using Wallet.API.Models.Base;
using Wallet.API.Models.WalletOfFamily;
using Wallet.API.Services.Abstractions;
using Wallet.Contracts.Wallet;
using Wallet.Domain.SeedWork;
using WalletOfFamily = Wallet.Domain.Models.WalletOfFamily;

namespace Wallet.API.Services
{
    /// <summary>
    /// Сервис Кошельков
    /// </summary>
    public class WalletService : IWalletService
    {
        private readonly IRepository<WalletOfFamily.Wallet, Guid> _walletRepository;
        private readonly IRepository<WalletOfFamily.Family, Guid> _familyRepository;
        private readonly IMapper _mapper;
        private readonly IPublishService _publishService;
        private readonly ILogger<WalletService> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса WalletService
        /// </summary>
        /// <param name="walletRepository">Репозиторий для работы с кошельками (обязательный)</param>
        /// <param name="familyRepository">Репозиторий для работы с семьями (обязательный)</param>
        /// <param name="mapper">Сервис для маппинга объектов (обязательный)</param>
        /// <exception cref="WalletAPIException">Выбрасывается если свойство имеет NULL</exception>
        public WalletService(IRepository<WalletOfFamily.Wallet, Guid> walletRepository,
            IRepository<WalletOfFamily.Family, Guid> familyRepository,
            ILogger<WalletService> logger,
            IMapper mapper,
            IPublishService publishService)
        {
            _logger = logger ?? throw new WalletAPIException($"Property {nameof(logger)} cannot be null");
            _walletRepository = walletRepository ?? throw new WalletAPIException($"Property {nameof(walletRepository)} cannot be null");
            _familyRepository = familyRepository ?? throw new WalletAPIException($"Property {nameof(familyRepository)} cannot be null");
            _mapper = mapper ?? throw new WalletAPIException($"Property {nameof(mapper)} cannot be null");
            _publishService = publishService;
        }

        /// <summary>
        /// Создает новый кошелёк
        /// </summary>
        /// <param name="newWallet">Информация о новом кошельке семьи</param>
        /// <param name="cancellation">Токен отмены операции</param>
        /// <returns>Созданный кошелёк</returns>
        /// <exception cref="WalletAPIException">Непредвиденная ошибка</exception>
        public async Task<WalletReadModel> CreateWallet(WalletWriteModel newWallet, CancellationToken cancellation)
        {
            try
            {
                // Проверяем, существует ли уже кошелек для этой семьи и валюты
                var walletIsCreated = await _walletRepository.GetAll()
                    .AnyAsync(w => w.Family.Id.Equals(newWallet.Family.Id) && w.Currency.ToString() == newWallet.Currency, cancellationToken: cancellation);

                if (walletIsCreated)
                    throw new WalletAPIException($"Family's (Id = {newWallet.Family.Id}) main wallet is created");

                // Проверка, существует ли семья в базе данных
                var family = await _familyRepository.GetAll()
                    .FirstOrDefaultAsync(f => f.Id == newWallet.Family.Id, cancellationToken: cancellation);

                family ??= await CreateFamily(newWallet.Family, cancellation);

                // Присваиваем существующую или новую запись семьи кошельку

                var wallet = _mapper.Map<WalletOfFamily.Wallet>(newWallet);
                wallet.AddFamily(family);

                await _walletRepository.AddAsync(wallet, cancellation);
                await _walletRepository.SaveAsync(cancellation);

                await _publishService.PublishAsync(message: new WalletCreated(wallet.Id, wallet.Description));

                return _mapper.Map<WalletReadModel>(wallet);

            }
            catch (Exception ex)
            {
                throw new WalletAPIException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Создает новую семью, если она не существует
        /// </summary>
        /// <param name="newFamily">Информация о новой семье</param>
        /// <param name="cancellation">Токен отмены операции</param>
        /// <returns>Созданная семья</returns>
        /// <exception cref="WalletAPIException">Непредвиденная ошибка</exception>
        private async Task<WalletOfFamily.Family?> CreateFamily(FamilyWriteModel newFamily, CancellationToken cancellation)
        {
            try
            {
                // Если семья не существует, создаем новую запись
                WalletOfFamily.Family? family = _mapper.Map<WalletOfFamily.Family>(newFamily);
                await _familyRepository.AddAsync(family, cancellation);
                await _familyRepository.SaveAsync(cancellation);
                return family;

            }
            catch (Exception ex)
            {
                throw new WalletAPIException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Получает все кошельки с пагинацией
        /// </summary>
        /// <param name="pageIndex">(обязательный): Индекс страницы (по умолчанию 1)</param>
        /// <param name="pageSize">(обязательный): Количество записей на странице (по умолчанию 10)</param>
        /// <param name="cancellation">Токен отмены операции</param>
        /// <returns>Список всех кошельков с пагинацией</returns>
        /// <exception cref="WalletAPIException">Выбрасывается, если произошла ошибка при получении списка кошельков</exception>
        public async Task<PaginatedItems<WalletReadModel>> GetWallets(int pageIndex, int pageSize, CancellationToken cancellation)
        {
            _logger.LogInformation("Fetching wallets with pagination. PageIndex: {pageIndex}, PageSize: {pageSize}.", pageIndex, pageSize);
            try
            {
                var wallets = _walletRepository.GetAll();

                var walletsPaginated = await wallets
                    .OrderBy(w => w.Id)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellation);

                bool hasMoreItems = await wallets.Skip(pageIndex * pageSize)
                    .AnyAsync(cancellation);

                var items = _mapper.Map<List<WalletReadModel>>(walletsPaginated);

                _logger.LogInformation("Successfully fetched {iCount} wallets for PageIndex: {pageIndex}, PageSize: {pageSize}.", items.Count, pageIndex, pageSize);
                return new PaginatedItems<WalletReadModel>(items, pageIndex, items.Count, hasMoreItems);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching wallets with pagination.");
                throw new WalletAPIException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Обновляет информацию по кошельку
        /// </summary>
        /// <param name="walletId">Id кошелька</param>
        /// <param name="updateWallet">Данные для обновления</param>
        /// <param name="cancellation">Токен отмены операции</param>
        /// <returns>Обновлённая информация по кошельку</returns>
        /// <exception cref="WalletAPIException">Выбрасывается, если произошла ошибка при обновлении кошелька</exception>
        public async Task<WalletReadModel> UpdateWallet(Guid walletId, WalletWriteModel updateWallet, CancellationToken cancellation)
        {
            try
            {
                var wallet = await _walletRepository.GetByIdAsync(walletId, cancellation);

                _logger.LogInformation("Changing description of wallet with Id {walletId} to '{Description}'.", walletId, updateWallet.Description);
                wallet.ChangeDescription(updateWallet.Description);

                _logger.LogInformation("Changing family name of wallet with Id {walletId} to '{Name}'.", walletId, updateWallet.Family.Name);
                wallet.Family.ChangeName(updateWallet.Family.Name);

                await _walletRepository.UpdateAsync(wallet);
                await _walletRepository.SaveAsync(cancellation);

                _logger.LogInformation("Successfully updated wallet with Id {walletId}.", walletId);

                await _publishService.PublishAsync(message: new WalletUpdated(wallet.Id, wallet.Description));

                return _mapper.Map<WalletReadModel>(wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating wallet with Id {walletId}.", walletId);
                throw new WalletAPIException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Получает кошелёк по Id
        /// </summary>
        /// <param name="walletId">(обязательный): Id кошелька</param>
        /// <param name="cancellation">Токен отмены операции</param>
        /// <returns>Кошелёк с указанным Id</returns>
        /// <exception cref="WalletAPIException">Выбрасывается, если произошла ошибка при получении кошелька</exception>
        public async Task<WalletReadModel> GetWallet(Guid walletId, CancellationToken cancellation)
        {
            try
            {
                var walletDb = await _walletRepository.GetByIdAsync(walletId, cancellation);
                _logger.LogInformation("Successfully fetched wallet with Id {walletId}.", walletId);
                return _mapper.Map<WalletReadModel>(walletDb);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching wallet with Id {walletId}.", walletId);
                throw new WalletAPIException(ex.Message, ex);
            }
        }

        /// <summary> 
        /// Удаляет кошелёк по указанному идентификатору. 
        /// </summary> 
        /// <param name="walletId">Идентификатор удаляемого кошелька.</param> 
        /// <param name="cancellation">Токен отмены операции.</param> 
        /// <returns>Задача выполнения операции удаления кошелька.</returns> 
        /// <exception cref="WalletAPIException"> 
        /// Выбрасывается, если кошелёк содержит подкошельки или баланс кошелька больше нуля. 
        /// Выбрасывается, если произошла ошибка при удалении кошелька. 
        /// </exception>
        public async Task DeleteWallet(Guid walletId, CancellationToken cancellation)
        {
            try
            {
                var wallet = await _walletRepository.GetByIdAsync(walletId, cancellation);

                if (wallet.IsSubWallets)
                {
                    _logger.LogWarning("Cannot delete wallet {walletId}. It has subwallets.", walletId);
                    throw new WalletAPIException($"Cannot be deleted. The wallet (Id = {walletId}) has the subwallets");
                }

                if (wallet.Balance > 0)
                {
                    _logger.LogWarning("Cannot delete wallet {walletId}. It has a balance of {Balance}.", walletId, wallet.Balance);
                    throw new WalletAPIException($"Cannot be deleted. The wallet (Id = {walletId}) has the money");
                }

                await _walletRepository.DeleteAsync(walletId, cancellation);
                await _walletRepository.SaveAsync(cancellation);

                await _publishService.PublishAsync(message: new WalletDeleted(wallet.Id));

                _logger.LogInformation("Successfully deleted wallet {walletId}.", walletId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting wallet {walletId}.", walletId);
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
        public async Task TransferFundsBetweenWalletAndSubWallet(WalletTransferFundsWriteModel transferFunds, CancellationToken cancellation)
        {
            try
            {
                var fromWallet = await _walletRepository.GetByIdAsync(transferFunds.FromWalletId, cancellation);
                var toWallet = await _walletRepository.GetByIdAsync(transferFunds.ToWalletId, cancellation);

                // Проверка на соответствие семей
                if (fromWallet.Family.Id != toWallet.Family.Id)
                {
                    _logger.LogWarning("Cannot transfer funds between wallets of different families. FromWallet's Family Id = {fromFamilyId}, ToWallet's Family Id = {toFamilyId}", fromWallet.Family.Id, toWallet.Family.Id);
                    throw new WalletAPIException($"Cannot transfer funds between wallets of different families. FromWallet's Family Id = {fromWallet.Family.Id}, ToWallet's Family Id = {toWallet.Family.Id}");
                }

                // Проверка валюты
                if (fromWallet.Currency != toWallet.Currency)
                {
                    _logger.LogWarning("Cannot transfer funds between wallets with different currencies.");
                    throw new WalletAPIException("Cannot transfer funds between wallets with different currencies.");
                }

                _logger.LogInformation("Writing off {amount} from wallet {fromWalletId}.", transferFunds.Amount, transferFunds.FromWalletId);
                fromWallet.WriteOffMoney(transferFunds.Amount);

                _logger.LogInformation("Adding {amount} to wallet {toWalletId}.", transferFunds.Amount, transferFunds.ToWalletId);
                toWallet.AddMoney(transferFunds.Amount);

                await _walletRepository.UpdateAsync(fromWallet);
                await _walletRepository.UpdateAsync(toWallet);
                await _walletRepository.SaveAsync(cancellation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while transferring funds between wallets.");
                throw new WalletAPIException(ex.Message, ex);
            }
        }
    }
}
