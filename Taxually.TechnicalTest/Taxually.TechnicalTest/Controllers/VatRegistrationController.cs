using System.Text;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using Taxually.TechnicalTest.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Taxually.TechnicalTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VatRegistrationController : ControllerBase
    {
        private readonly IVatRegistrationService _vatRegistrationService;

        public VatRegistrationController(
            IVatRegistrationService vatRegistrationService)
        {
            _vatRegistrationService = vatRegistrationService ?? throw new ArgumentNullException(nameof(vatRegistrationService));
        }

        /// <summary>
        /// Registers a company for a VAT number in a given country
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody] VatRegistrationRequest request)
        {
            await _vatRegistrationService.ProcessAsync(request);
            return Ok();
        }
    }

    public class VatRegistrationRequest
    {
        public string CompanyName { get; set; }
        public string CompanyId { get; set; }
        public string Country { get; set; }
    }
}
