using FluentAssertions;
using NSubstitute;
using System.Text;
using Taxually.TechnicalTest;
using Taxually.TechnicalTest.Controllers;
using Taxually.TechnicalTest.Services;

namespace Taxually.Test.Unit.Services
{
    public class VatRegistrationServiceTests
    {
        private TaxuallyHttpClient _taxuallyHttpClient;
        private TaxuallyQueueClient _taxuallyQueueClient;

        private VatRegistrationService _vatRegistrationService;

        public VatRegistrationServiceTests()
        {
            _taxuallyHttpClient = Substitute.For<TaxuallyHttpClient>();
            _taxuallyQueueClient = Substitute.For<TaxuallyQueueClient>();

            _vatRegistrationService = new VatRegistrationService(_taxuallyHttpClient, _taxuallyQueueClient);
        }

        [Fact]
        public async Task ProcessAsync_WhenCountryIsFrance_EnqueuesCsvInByteArray()
        {
            var csvBytes = Array.Empty<byte>();
            await _taxuallyQueueClient.EnqueueAsync(Arg.Any<string>(), Arg.Do<byte[]>(x => csvBytes = x));

            var request = new VatRegistrationRequest
            {
                Country = "FR",
                CompanyId = "id",
                CompanyName = "name"
            };

            await _vatRegistrationService.ProcessAsync(request);
            var csv = Encoding.UTF8.GetString(csvBytes);

            const string expectedCsv = "CompanyName,CompanyId\r\nnameid\r\n";

            csv.Should().Be(expectedCsv);
        }
    }
}