using System.Text;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Taxually.TechnicalTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VatRegistrationController : ControllerBase
    {
        private readonly TaxuallyHttpClient _taxuallyHttpClient;
        private readonly TaxuallyQueueClient _taxuallyQueueClient;

        public VatRegistrationController(
            TaxuallyHttpClient taxuallyHttpClient,
            TaxuallyQueueClient taxuallyQueueClient)
        {
            _taxuallyHttpClient = taxuallyHttpClient ?? throw new ArgumentNullException(nameof(taxuallyHttpClient));
            _taxuallyQueueClient = taxuallyQueueClient ?? throw new ArgumentNullException(nameof(taxuallyQueueClient));
        }

        /// <summary>
        /// Registers a company for a VAT number in a given country
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody] VatRegistrationRequest request)
        {
            switch (request.Country)
            {
                case "GB":
                    // UK has an API to register for a VAT number
                    _taxuallyHttpClient.PostAsync("https://api.uktax.gov.uk", request).Wait();
                    break;
                case "FR":
                    // France requires an excel spreadsheet to be uploaded to register for a VAT number
                    var csvBuilder = new StringBuilder();
                    csvBuilder.AppendLine("CompanyName,CompanyId");
                    csvBuilder.AppendLine($"{request.CompanyName}{request.CompanyId}");
                    var csv = Encoding.UTF8.GetBytes(csvBuilder.ToString());
                    // Queue file to be processed
                    _taxuallyQueueClient.EnqueueAsync("vat-registration-csv", csv).Wait();
                    break;
                case "DE":
                    // Germany requires an XML document to be uploaded to register for a VAT number
                    using (var stringwriter = new StringWriter())
                    {
                        var serializer = new XmlSerializer(typeof(VatRegistrationRequest));
                        serializer.Serialize(stringwriter, request);
                        var xml = stringwriter.ToString();
                        // Queue xml doc to be processed
                        _taxuallyQueueClient.EnqueueAsync("vat-registration-xml", xml).Wait();
                    }
                    break;
                default:
                    throw new Exception("Country not supported");

            }
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
