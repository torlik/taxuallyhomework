using System.Text;
using System.Xml.Serialization;
using Taxually.TechnicalTest.Controllers;

namespace Taxually.TechnicalTest.Services
{
    public class VatRegistrationService : IVatRegistrationService
    {
        private readonly TaxuallyHttpClient _taxuallyHttpClient;
        private readonly TaxuallyQueueClient _taxuallyQueueClient;

        public VatRegistrationService(TaxuallyHttpClient taxuallyHttpClient, TaxuallyQueueClient taxuallyQueueClient)
        {
            _taxuallyHttpClient = taxuallyHttpClient ?? throw new ArgumentNullException(nameof(taxuallyHttpClient));
            _taxuallyQueueClient = taxuallyQueueClient ?? throw new ArgumentNullException(nameof(taxuallyQueueClient));
        }

        public async Task ProcessAsync(VatRegistrationRequest vatRegistrationRequest)
        {
            switch (vatRegistrationRequest.Country)
            {
                case "GB":
                    // UK has an API to register for a VAT number
                    await _taxuallyHttpClient.PostAsync("https://api.uktax.gov.uk", vatRegistrationRequest);
                    break;
                case "FR":
                    // France requires an excel spreadsheet to be uploaded to register for a VAT number
                    var csvBuilder = new StringBuilder();
                    csvBuilder.AppendLine("CompanyName,CompanyId");
                    csvBuilder.AppendLine($"{vatRegistrationRequest.CompanyName}{vatRegistrationRequest.CompanyId}");
                    var csv = Encoding.UTF8.GetBytes(csvBuilder.ToString());
                    // Queue file to be processed
                    await _taxuallyQueueClient.EnqueueAsync("vat-registration-csv", csv);
                    break;
                case "DE":
                    // Germany requires an XML document to be uploaded to register for a VAT number
                    using (var stringwriter = new StringWriter())
                    {
                        var serializer = new XmlSerializer(typeof(VatRegistrationRequest));
                        serializer.Serialize(stringwriter, vatRegistrationRequest);
                        var xml = stringwriter.ToString();
                        // Queue xml doc to be processed
                        await _taxuallyQueueClient.EnqueueAsync("vat-registration-xml", xml);
                    }
                    break;
                default:
                    throw new Exception("Country not supported");

            }
        }
    }
}
