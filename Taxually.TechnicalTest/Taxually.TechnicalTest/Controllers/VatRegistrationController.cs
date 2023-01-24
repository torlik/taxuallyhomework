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
        /// <summary>
        /// Registers a company for a VAT number in a given country
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] VatRegistrationRequest request)
        {
            switch (request.Country)
            {
                case "GB":
                    // UK has an API to register for a VAT number
                    var httpClient = new TaxuallyHttpClient();
                    httpClient.PostAsync("https://api.uktax.gov.uk", request).Wait();
                    break;
                case "FR":
                    // France requires an excel spreadsheet to be uploaded to register for a VAT number
                    var csvBuilder = new StringBuilder();
                    csvBuilder.AppendLine("CompanyName,CompanyId");
                    csvBuilder.AppendLine($"{request.CompanyName}{request.CompanyId}");
                    var csv = Encoding.UTF8.GetBytes(csvBuilder.ToString());
                    var excelQueueClient = new TaxuallyQueueClient();
                    // Queue file to be processed
                    excelQueueClient.EnqueueAsync("vat-registration-csv", csv).Wait();
                    break;
                case "DE":
                    // Germany requires an XML document to be uploaded to register for a VAT number
                    using (var stringwriter = new StringWriter())
                    {
                        var serializer = new XmlSerializer(typeof(VatRegistrationRequest));
                        serializer.Serialize(stringwriter, this);
                        var xml = stringwriter.ToString();
                        var xmlQueueClient = new TaxuallyQueueClient();
                        // Queue xml doc to be processed
                        xmlQueueClient.EnqueueAsync("vat-registration-xml", xml).Wait();
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
