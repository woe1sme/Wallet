using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Wallet.API.Applications.Exceptions;
using Wallet.API.Models.Base;
using Wallet.API.Models.WalletOfFamily;
using Wallet.API.Services.Abstractions;
using WalletOfFamily = Wallet.API.Models.WalletOfFamily;

namespace Wallet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletsController : ControllerBase
    {
        private readonly ILogger<WalletsController> _logger;
        private readonly IWalletService _walletService;

        public WalletsController(IWalletService walletService,
            ILogger<WalletsController> logger)
        {
            _walletService = walletService ?? throw new WalletAPIException($"Property {nameof(walletService)} cannot be null");
            _logger = logger ?? throw new WalletAPIException($"Property {nameof(logger)} cannot be null");
        }


        /// <summary>
        /// Получает все кошельки.
        /// </summary>
        /// <returns>Список всех кошельков</returns>
        [HttpGet]
        [SwaggerOperation(Summary = "Получить все кошельки", Description = "Возвращает список всех кошельков из базы данных.")]
        [SwaggerResponse(200, "Возвращает список всех кошельков", typeof(PaginatedItems<WalletOfFamily.WalletReadModel>))]
        public async Task<ActionResult<PaginatedItems<WalletOfFamily.WalletReadModel>>> GetWallets(CancellationToken cancellation, [SwaggerParameter(Description = "Страница")] int pageIndex = 1, [SwaggerParameter(Description = "Количество записей на странице")] int pageSize = 10)
        {
            try
            {
                return await _walletService.GetWallets(pageIndex, pageSize, cancellation);
            }
            catch (WalletAPIException ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                return BadRequest(ex.Message);
                throw;
            }
        }


        /// <summary>
        /// Получить кошелёк по Id.
        /// </summary>
        /// <returns>Кошелёк с указанным Id</returns>
        [HttpGet("{id:long}")]
        [SwaggerOperation(Summary = "Получить кошелёк по Id", Description = "Кошелёк с указанным Id из базы данных.")]
        [SwaggerResponse(200, "Кошелёк с указанным Id из базы данных.", typeof(WalletOfFamily.WalletReadModel))]
        public async Task<ActionResult<WalletOfFamily.WalletReadModel>> GetWallet([SwaggerParameter(Description = "Id кошелька", Required = true)] Guid id, CancellationToken cancellation)
        {
            try
            {
                return await _walletService.GetWallet(id, cancellation);
            }
            catch (WalletAPIException ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                return BadRequest(ex.Message);
                throw;
            }
        }


        /// <summary>
        /// Создать кошелёк
        /// </summary>
        /// <returns>Созданный кошелёк.</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Создать кошелёк", Description = "Возвращает созданный кошелёк.")]
        [SwaggerResponse(200, "Возвращает созданный кошелёк", typeof(WalletOfFamily.WalletReadModel))]
        public async Task<ActionResult<WalletOfFamily.WalletReadModel>> Create([SwaggerRequestBody("Информация о новом кошельке семьи")] WalletWriteModel wallet, CancellationToken cancellation)
        {
            try
            {
                var resultWallet = await _walletService.CreateWallet(wallet, cancellation);

                return Ok(resultWallet);
            }
            catch (WalletAPIException ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                return BadRequest(ex.Message);
                throw;
            }
        }


        /// <summary>
        /// Обновляет информацию по кошельку
        /// </summary>
        /// <returns>Возвращает обновленную информацию по кошельку.</returns>
        [HttpPut("{id:long}")]
        [SwaggerOperation(Summary = "Обновить информацию по кошельку", Description = "Возвращает обновленную информацию по кошельку.")]
        [SwaggerResponse(200, "Возвращает обновленную информацию по кошельку", typeof(WalletOfFamily.WalletReadModel))]
        public async Task<ActionResult<WalletOfFamily.WalletReadModel>> Update([SwaggerParameter(Description = "Id кошелька", Required = true)] Guid id, [SwaggerRequestBody(Description = "Данные для обновления", Required = true)] WalletWriteModel updateWallet, CancellationToken cancellation)
        {
            try
            {
                return await _walletService.UpdateWallet(id, updateWallet, cancellation);
            }
            catch (WalletAPIException ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                return BadRequest(ex.Message);
                throw;
            }
        }


        /// <summary>
        /// Удалить кошелёк
        /// </summary>
        /// <param name="id">Id кошелька</param>
        /// <param name="cancellation">Токен отмены</param>
        [HttpDelete("{id:long}")]
        [SwaggerOperation(Summary = "Удалить кошелёк", Description = "Возвращает статус 204 при успешном выполнении.")]
        [SwaggerResponse(204, "Возвращает при успешном выполнении")]
        [SwaggerResponse(400, "Ошибка при удалении кошелька", typeof(string))]
        public async Task<IActionResult> Delete([SwaggerParameter(Description = "Id кошелька", Required = true)] Guid id, CancellationToken cancellation)
        {
            try
            {
                await _walletService.DeleteWallet(id, cancellation);
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
        /// Переводит указанную сумму денег между основным кошельком и подкошельком. 
        /// </summary> 
        /// <param name="transferFunds">Запрос с данными для перевода.</param> 
        /// <param name="cancellation">Токен отмены операции.</param> 
        /// <returns>Результат операции перевода средств.</returns> 
        /// <response code="200">Успешный перевод средств.</response> 
        /// <response code="400">Неверный запрос или логическая ошибка (например, несовпадение семей).</response>
        /// <response code="500">Внутренняя ошибка сервера.</response> 
        [HttpPost("TransferFunds")]
        [SwaggerOperation(Summary = "Переводит указанную сумму денег между основным кошельком и подкошельком.")]
        [SwaggerResponse(200, "Funds transferred successfully.")]
        [SwaggerResponse(400, "Invalid request or logic error.")]
        public async Task<IActionResult> TransferFunds([SwaggerRequestBody(Description = "Данные для перевода средств между кошельками", Required = true)] WalletTransferFundsWriteModel transferFunds, CancellationToken cancellation)
        {
            _logger.LogInformation("Initiating transfer of {Amount} from wallet {MainWalletId} to wallet {request.SubWalletId}.", transferFunds.Amount, transferFunds.FromWalletId, transferFunds.ToWalletId);
            try
            {
                await _walletService.TransferFundsBetweenWalletAndSubWallet(transferFunds, cancellation);
                _logger.LogInformation("Successfully transferred {Amount} from wallet {MainWalletId} to wallet {SubWalletId}.", transferFunds.Amount, transferFunds.FromWalletId, transferFunds.ToWalletId);
                return Ok(new { Message = "Funds transferred successfully." });
            }
            catch (WalletAPIException ex)
            {
                _logger.LogWarning(ex, "WalletAPIException: {Message}", ex.Message);
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while transferring funds.");
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
    }
}
