using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Wallet.API.Applications.Exceptions;
using Wallet.API.Models.Base;
using Wallet.API.Models.AccountOfPerson;
using Wallet.API.Services.Abstractions;

namespace Wallet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly ILogger<AccountsController> _logger;
    private readonly IAccountService _accountService;

    /// <summary>
    /// Инициализирует новый экземпляр контроллера AccountsController
    /// </summary>
    /// <param name="accountService">Сервис для работы с персональными счетами (обязательный)</param>
    /// <param name="logger">Сервис для логирования (обязательный)</param>
    /// <exception cref="WalletAPIException">Выбрасывается, если одно из свойств имеет NULL</exception>
    public AccountsController(IAccountService accountService, ILogger<AccountsController> logger)
    {
        _accountService = accountService ?? throw new WalletAPIException($"Property {nameof(accountService)} cannot be null");
        _logger = logger ?? throw new WalletAPIException($"Property {nameof(logger)} cannot be null");
    }

    /// <summary>
    /// Создать персональный счёт
    /// </summary>
    /// <returns>Созданный персональный счёт.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Создать персональный счёт", Description = "Возвращает созданный персональный счёт.")]
    [SwaggerResponse(200, "Возвращает созданный персональный счёт", typeof(AccountReadModel))]
    public async Task<ActionResult<AccountReadModel>> Create([SwaggerRequestBody("Информация о новом персональном счёте")] AccountWriteModel account, CancellationToken cancellation)
    {
        try
        {
            var resultAccount = await _accountService.CreatePersonalAccount(account, cancellation);

            return Ok(resultAccount);
        }
        catch (WalletAPIException ex)
        {
            _logger.LogError(ex.Message, ex.StackTrace);
            return BadRequest(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Получает все персональные счета с пагинацией.
    /// </summary>
    /// <returns>Список всех персональных счетов с пагинацией</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Получить все персональные счета", Description = "Возвращает список всех персональных счетов из базы данных.")]
    [SwaggerResponse(200, "Возвращает список всех персональных счетов", typeof(PaginatedItems<AccountReadModel>))]
    public async Task<ActionResult<PaginatedItems<AccountReadModel>>> GetAccounts(CancellationToken cancellation, [SwaggerParameter(Description = "Страница")] int pageIndex = 1, [SwaggerParameter(Description = "Количество записей на странице")] int pageSize = 10)
    {
        try
        {
            return await _accountService.GetAccounts(pageIndex, pageSize, cancellation);
        }
        catch (WalletAPIException ex)
        {
            _logger.LogError(ex.Message, ex.StackTrace);
            return BadRequest(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Получает персональный счёт по Id.
    /// </summary>
    /// <returns>Персональный счёт с указанным Id</returns>
    [HttpGet("{id:long}")]
    [SwaggerOperation(Summary = "Получить персональный счёт по Id", Description = "Персональный счёт с указанным Id из базы данных.")]
    [SwaggerResponse(200, "Персональный счёт с указанным Id из базы данных.", typeof(AccountReadModel))]
    public async Task<ActionResult<AccountReadModel>> GetAccount([SwaggerParameter(Description = "Id счёта", Required = true)] Guid id, CancellationToken cancellation)
    {
        try
        {
            return await _accountService.GetAccount(id, cancellation);
        }
        catch (WalletAPIException ex)
        {
            _logger.LogError(ex.Message, ex.StackTrace);
            return BadRequest(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Обновляет информацию по персональному счёту
    /// </summary>
    /// <returns>Возвращает обновленную информацию по персональному счёту</returns>
    [HttpPut("{id:long}")]
    [SwaggerOperation(Summary = "Обновить информацию по персональному счёту", Description = "Возвращает обновленную информацию по персональному счёту.")]
    [SwaggerResponse(200, "Возвращает обновленную информацию по персональному счёту", typeof(AccountReadModel))]
    public async Task<ActionResult<AccountReadModel>> Update([SwaggerParameter(Description = "Id счёта", Required = true)] Guid id, [SwaggerRequestBody(Description = "Данные для обновления", Required = true)] AccountWriteModel updateAccount, CancellationToken cancellation)
    {
        try
        {
            var updatedAccount = await _accountService.UpdateAccount(id, updateAccount, cancellation);

            return Ok(updatedAccount);
        }
        catch (WalletAPIException ex)
        {
            _logger.LogError(ex.Message, ex.StackTrace);
            return BadRequest(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Удаляет персональный счёт по указанному идентификатору
    /// </summary>
    /// <param name="id">Id персонального счёта</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Задача выполнения операции удаления персонального счёта</returns>
    /// <exception cref="WalletAPIException">
    /// Выбрасывается, если баланс персонального счёта больше нуля.
    /// Выбрасывается, если произошла ошибка при удалении персонального счёта.
    /// </exception>
    [HttpDelete("{id:long}")]
    [SwaggerOperation(Summary = "Удалить персональный счёт", Description = "Возвращает статус 204 при успешном выполнении.")]
    [SwaggerResponse(204, "Возвращает при успешном выполнении")]
    [SwaggerResponse(400, "Ошибка при удалении счёта", typeof(string))]
    public async Task<IActionResult> Delete([SwaggerParameter(Description = "Id персонального счёта", Required = true)] Guid id, CancellationToken cancellation)
    {
        try
        {
            await _accountService.DeleteAccount(id, cancellation);
            return NoContent();
        }
        catch (WalletAPIException ex)
        {
            _logger.LogError(ex.Message, ex.StackTrace);
            return BadRequest(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Переводит указанную сумму денег между персональными счетами.
    /// </summary>
    /// <param name="transferFunds">Модель данных для перевода средств между счетами</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Результат операции перевода средств</returns>
    /// <exception cref="WalletAPIException">
    /// Выбрасывается, если персональные счета принадлежат разным пользователям.
    /// Выбрасывается, если на счету-отправителе недостаточно средств.
    /// Выбрасывается, если у счетов разные валюты.
    /// Выбрасывается, если произошла ошибка при сохранении изменений.
    /// </exception>
    [HttpPost("TransferFunds")]
    [SwaggerOperation(Summary = "Переводит указанную сумму денег между персональными счетами", Description = "Переводит указанную сумму денег между персональными счетами.")]
    [SwaggerResponse(200, "Средства успешно переведены.")]
    [SwaggerResponse(400, "Неверный запрос или логическая ошибка.")]
    public async Task<IActionResult> TransferFunds([SwaggerRequestBody(Description = "Данные для перевода средств между счетами", Required = true)] AccountTransferFundsWriteModel transferFunds, CancellationToken cancellation)
    {
        _logger.LogInformation("Initiating transfer of {Amount} from account {fromAccountId} to account {toAccountId}.", transferFunds.Amount, transferFunds.FromAccountId, transferFunds.ToWalletId);
        try
        {
            await _accountService.TransferFundsFromAccountToWallet(transferFunds, cancellation);
            _logger.LogInformation("Successfully transferred {Amount} from account {fromAccountId} to account {toAccountId}.", transferFunds.Amount, transferFunds.FromAccountId, transferFunds.ToWalletId);
            return Ok(new { Message = "Средства успешно переведены." });
        }
        catch (WalletAPIException ex)
        {
            _logger.LogWarning(ex, "WalletAPIException: {Message}", ex.Message);
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Добавляет деньги на личный счёт
    /// </summary>
    /// <param name="addFundsToAccount">Запрос с данными для добавления средств</param>
    /// <param name="cancellation">Токен отмены операции</param>
    /// <returns>Результат операции добавления средств</returns>
    /// <exception cref="WalletAPIException">Выбрасывается, если произошла ошибка при добавлении денег или если профиль не соответствует владельцу счёта</exception>
    [HttpPost("AddFunds")]
    [SwaggerOperation(Summary = "Добавляет деньги на личный счёт", Description = "Добавляет деньги на личный счёт.")]
    [SwaggerResponse(200, "Средства успешно добавлены.")]
    [SwaggerResponse(400, "Неверный запрос.")]
    public async Task<IActionResult> AddFunds([SwaggerRequestBody(Description = "Данные для добавления средств на счёт", Required = true)] AddFundsToAccountWriteModel addFundsToAccount, CancellationToken cancellation)
    {
        _logger.LogInformation("Adding {Amount} to account {accountId}.", addFundsToAccount.Amount, addFundsToAccount.AccountId);
        try
        {
            await _accountService.AddFundsToAccount(addFundsToAccount, cancellation);
            _logger.LogInformation("Successfully added {Amount} to account {accountId}.", addFundsToAccount.Amount, addFundsToAccount.AccountId);
            return Ok(new { Message = "Счет успешно пополнен." });
        }
        catch (WalletAPIException ex)
        {
            _logger.LogWarning(ex, "WalletAPIException: {Message}", ex.Message);
            return BadRequest(new { Error = ex.Message });
        }
    }

}
