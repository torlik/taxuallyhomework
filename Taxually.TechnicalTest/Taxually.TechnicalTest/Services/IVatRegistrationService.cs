using Taxually.TechnicalTest.Controllers;

namespace Taxually.TechnicalTest.Services
{
    public interface IVatRegistrationService
    {
        Task ProcessAsync(VatRegistrationRequest vatRegistrationRequest);
    }
}
