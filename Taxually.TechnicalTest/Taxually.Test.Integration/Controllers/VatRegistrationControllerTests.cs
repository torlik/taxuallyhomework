using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Net;
using System.Text;
using Taxually.TechnicalTest;
using Taxually.TechnicalTest.Controllers;

namespace Taxually.Test.Integration.Controllers
{
    public class VatRegistrationControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public VatRegistrationControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task PostAsync_WhenCountryIsUk_PostsRequestWithTaxuallyHttpClient()
        {
            var taxuallyHttpClient = Substitute.For<TaxuallyHttpClient>();
            var client = CreateClient((typeof(TaxuallyHttpClient), taxuallyHttpClient));
            const string url = "api/vatregistration";
            var content = new StringContent(
                @"{""companyName"":""name"",""companyId"":""id"",""country"":""GB""}",
                Encoding.UTF8,
                "application/json");

            const string expectedUrl = "https://api.uktax.gov.uk";

            var response = await client.PostAsync(url, content);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            await taxuallyHttpClient
                .Received(1)
                .PostAsync(expectedUrl,
                    Arg.Is<VatRegistrationRequest>(x =>
                        x.CompanyId == "id" &&
                        x.CompanyName == "name" &&
                        x.Country == "GB"));
        }

        [Fact]
        public async Task PostAsync_WhenCountryIsFrance_PostsRequestWithTaxuallyQueueClient()
        {
            var taxuallyHttpClient = Substitute.For<TaxuallyQueueClient>();
            var client = CreateClient((typeof(TaxuallyQueueClient), taxuallyHttpClient));
            const string url = "api/vatregistration";
            var content = new StringContent(
                @"{""companyName"":""name"",""companyId"":""id"",""country"":""FR""}",
                Encoding.UTF8,
                "application/json");

            const string expectedQueueName = "vat-registration-csv";

            var response = await client.PostAsync(url, content);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            await taxuallyHttpClient.Received(1).EnqueueAsync(expectedQueueName, Arg.Any<byte[]>);
        }

        [Fact]
        public async Task PostAsync_WhenCountryIsGermany_PostsRequestWithTaxuallyQueueClient()
        {
            var taxuallyQueueClient = Substitute.For<TaxuallyQueueClient>();
            var client = CreateClient((typeof(TaxuallyQueueClient), taxuallyQueueClient));
            const string url = "api/vatregistration";
            var content = new StringContent(
                @"{""companyName"":""name"",""companyId"":""id"",""country"":""DE""}",
                Encoding.UTF8,
                "application/json");

            const string expectedQueueName = "vat-registration-xml";

            var response = await client.PostAsync(url, content);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            await taxuallyQueueClient.Received(1).EnqueueAsync(expectedQueueName, Arg.Any<string>);
        }

        [Fact]
        public async Task PostAsync_WhenCountryIsNotSupported_ReturnsInternalServerError()
        {
            var taxuallyHttpClient = Substitute.For<TaxuallyHttpClient>();
            var client = CreateClient((typeof(TaxuallyHttpClient), taxuallyHttpClient));
            const string url = "api/vatregistration";
            var content = new StringContent(
                @"{""companyName"":""name"",""companyId"":""id"",""country"":""HU""}",
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(url, content);

            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        private HttpClient CreateClient(params (Type serviceType, object serviceImplementation)[] serviceRegistrations)
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    foreach ((var serviceType, var serviceMock) in serviceRegistrations)
                    {
                        var registeredService = services.SingleOrDefault(d => d.ServiceType == serviceType);
                        if (registeredService is not null)
                        {
                            services.Remove(registeredService);
                        }

                        services.AddSingleton(serviceType, serviceMock);
                    }
                });
            })
            .CreateClient();

            return client;
        }
    }
}