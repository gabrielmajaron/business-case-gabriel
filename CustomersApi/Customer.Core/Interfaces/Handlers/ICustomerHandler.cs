using Customer.Core.DataTransferObjects;

namespace Customer.Core.Interfaces.Handlers;

public interface ICustomerHandler
{
    public Task CreateAsync(CreateCustomerRequest createCustomerRequest);
}