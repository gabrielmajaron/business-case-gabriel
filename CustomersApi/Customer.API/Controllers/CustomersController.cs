using Customer.Core.DataTransferObjects;
using Customer.Core.Interfaces.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace ParanaBanco.CustomerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerHandler _customerHandler;
    
    public CustomersController(ICustomerHandler customerHandler)
    {
        _customerHandler = customerHandler;
        
        ArgumentNullException.ThrowIfNull(nameof(customerHandler));
    }

    // Autenticação aqui
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateCustomerRequest createCustomerRequest)
    {
        await _customerHandler.CreateAsync(createCustomerRequest);
        
        return Ok();
    }
}