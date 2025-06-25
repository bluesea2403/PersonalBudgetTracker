using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BudgetTracker.Services;

namespace BudgetTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OcrController : ControllerBase
    {
        private readonly IOcrService _ocrService;

        public OcrController(IOcrService ocrService)
        {
            _ocrService = ocrService;
        }

        [HttpPost("UploadInvoice")]
        public async Task<IActionResult> UploadInvoice(IFormFile invoiceFile)
        {
            if (invoiceFile == null || invoiceFile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var extractedData = await _ocrService.ExtractTransactionDataFromInvoice(invoiceFile);

            if (extractedData != null)
            {
                return Ok(extractedData);
            }

            return StatusCode(500, "Could not process the image.");
        }
    }
}