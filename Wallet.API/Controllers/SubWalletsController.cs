using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Wallet.API.Applications.Exceptions;
using Wallet.API.Models.Base;
using Wallet.API.Models.WalletOfFamily;
using Wallet.API.Services.Abstractions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Wallet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubWalletsController : ControllerBase
    {
        private readonly ISubWalletService _subWalletService; 
        private readonly ILogger<SubWalletsController> _logger;
        public SubWalletsController(
            ISubWalletService subWalletService, 
            ILogger<SubWalletsController> logger) 
        { 
            _subWalletService = subWalletService ?? throw new WalletAPIException($"Property {nameof(subWalletService)} cannot be null");
            _logger = logger ?? throw new WalletAPIException($"Property {nameof(logger)} cannot be null");
        }

        /// <summary>
        /// Создаёт новый подкошелёк.
        /// </summary>
        /// <param name="subWallet">Модель данных для создания нового подкошелька.</param>
        /// <param name="cancellation">Токен отмены операции.</param>
        /// <returns>Созданный подкошелёк.</returns>
        /// <response code="201">Успешное создание подкошелька.</response>
        /// <response code="400">Неверный запрос или ошибка в бизнес-логике.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Создаёт новый подкошелёк.", Description = "Возвращает cозданный подкошелёк")]
        [SwaggerResponse(201, "Sub-wallet created successfully.", typeof(SubWalletReadModel))]
        [SwaggerResponse(400, "Invalid request or business logic error.")]
        public async Task<ActionResult<SubWalletReadModel>> Create([SwaggerRequestBody("Модель данных для создания нового подкошелька.", Required = true)] SubWalletWriteModel subWallet, CancellationToken cancellation)
        {
            _logger.LogInformation("Received request to create a new sub-wallet.");

            try
            {
                var createdSubWallet = await _subWalletService.CreateSubWallet(subWallet, cancellation);

                _logger.LogInformation("Sub-wallet created successfully with ID {SubWalletId}.", createdSubWallet.Id);

                return Ok(createdSubWallet);
            }
            catch (WalletAPIException ex)
            {
                _logger.LogWarning(ex, "WalletAPIException: {Message}", ex.Message);
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating the sub-wallet.");
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Обновляет информацию подкошелька.
        /// </summary>
        /// <param name="id">Идентификатор подкошелька.</param>
        /// <param name="updateModel">Модель данных для обновления подкошелька.</param>
        /// <returns>Обновлённый подкошелёк.</returns>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Обновляет информацию подкошелька.", Description = "Обновлённый подкошелёк")]
        [SwaggerResponse(200, "Sub-wallet updated successfully.", typeof(SubWalletReadModel))]
        [SwaggerResponse(400, "Invalid request or business logic error.")]
        public async Task<ActionResult<SubWalletReadModel>> Update([SwaggerParameter(Description = "Идентификатор подкошелька", Required = true)] Guid id, [SwaggerRequestBody("Модель данных для обновления подкошелька.", Required = true)] SubWalletWriteModel subWalletUpdateModel, CancellationToken cancellation)
        {
            _logger.LogInformation("Request to update sub-wallet with ID {id} received.", id);

            try
            {
                var updatedSubWallet = await _subWalletService.UpdateSubWallet(id, subWalletUpdateModel, cancellation);
                return Ok(updatedSubWallet);
            }
            catch (WalletAPIException ex)
            {
                _logger.LogWarning(ex, "Error updating sub-wallet with ID {id}: {message}", id, ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating sub-wallet with ID {id}.", id);
                return StatusCode(500, new { Message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Удаляет подкошелёк по указанному идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор подкошелька.</param>
        /// <param name="cancellation">Токен отмены операции.</param>
        /// <returns>Результат операции удаления.</returns>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Удаляет подкошелёк по идентификатору.", Description = "Удаляет подкошелёк по указанному идентификатору. Операция невозможна, если баланс подкошелька не равен нулю или если у подкошелька есть подкошельки.")]
        [SwaggerResponse(204, "Sub-wallet was successfully deleted.")]
        [SwaggerResponse(400, "Invalid request or business logic error.")]
        public async Task<IActionResult> Delete([SwaggerParameter(Description = "Идентификатор подкошелька", Required = true)] Guid id, CancellationToken cancellation)
        {
            _logger.LogInformation("Request to delete sub-wallet with ID {id} received.", id);

            try
            {
                var result = await _subWalletService.DeleteSubWallet(id, cancellation);

                _logger.LogInformation("Deleted sub-wallet with ID {id} successfully.", id);
                return NoContent();
            }
            catch (WalletAPIException ex)
            {
                _logger.LogWarning(ex, "Error deleting sub-wallet with ID {id}: {message}", id, ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting sub-wallet with ID {id}.", id);
                return StatusCode(500, new { Message = "An unexpected error occurred." });
            }
        }

        /// <summary> 
        /// Переводит указанную сумму денег между подкошельком и кошельком. 
        /// </summary> 
        /// <param name="transferFunds">Запрос с данными для перевода.</param> 
        /// <param name="cancellation">Токен отмены операции.</param> 
        /// <returns>Результат операции перевода средств.</returns> 
        /// <response code="200">Успешный перевод средств.</response> 
        /// <response code="400">Неверный запрос или логическая ошибка (например, несовпадение семей).</response>
        [HttpPost("TransferFunds")]
        [SwaggerOperation(Summary = "Переводит указанную сумму денег между подкошельком и кошельком.")]
        [SwaggerResponse(200, "Funds transferred successfully.")]
        [SwaggerResponse(400, "Invalid request or logic error.")]
        public async Task<IActionResult> TransferFunds([SwaggerRequestBody(Description = "Данные для перевода средств между кошельками", Required = true)] SubWalletTransferFundsWriteModel transferFunds, CancellationToken cancellation)
        {
            _logger.LogInformation("Initiating transfer of {Amount} from wallet {MainWalletId} to wallet {WalletId}.", transferFunds.Amount, transferFunds.FromWalletId, transferFunds.ToWalletId);
            try
            {
                await _subWalletService.TransferFundsBetweenSubWalletAndWallet(transferFunds, cancellation);
                _logger.LogInformation("Successfully transferred {Amount} from wallet {MainWalletId} to wallet {WalletId}.", transferFunds.Amount, transferFunds.FromWalletId, transferFunds.ToWalletId);
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

        /// <summary>
        /// Получает все подкошельки.
        /// </summary>
        /// <returns>Список всех подкошельков</returns>
        [HttpGet]
        [SwaggerOperation(Summary = "Получить все подкошельки", Description = "Возвращает список всех подкошельков из базы данных.")]
        [SwaggerResponse(200, "Возвращает список всех подкошельков", typeof(PaginatedItems<SubWalletReadModel>))]
        public async Task<ActionResult<PaginatedItems<SubWalletReadModel>>> GetSubWallets(CancellationToken cancellation, [SwaggerParameter(Description = "Страница")] int pageIndex = 1, [SwaggerParameter(Description = "Количество записей на странице")] int pageSize = 10)
        {
            try
            {
                return Ok(await _subWalletService.GetSubWallets(pageIndex, pageSize, cancellation));
            }
            catch (WalletAPIException ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Получить подкошелёк по Id.
        /// </summary>
        /// <returns>Подкошелёк с указанным Id</returns>
        [HttpGet("{id:long}")]
        [SwaggerOperation(Summary = "Получить подкошелёк по Id", Description = "Подкошелёк с указанным Id из базы данных.")]
        [SwaggerResponse(200, "Подкошелёк с указанным Id из базы данных.", typeof(SubWalletReadModel))]
        public async Task<ActionResult<SubWalletReadModel>> GetSubWallet([SwaggerParameter(Description = "Id подкошелька", Required = true)] Guid id, CancellationToken cancellation)
        {
            try
            {
                return Ok(await _subWalletService.GetSubWallet(id, cancellation));
            }
            catch (WalletAPIException ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                return BadRequest(ex.Message);
            }
        }
    }
}
