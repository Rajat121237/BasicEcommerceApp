using eCommerce.API.Simulation;
using eCommerce.Core.DTO;
using eCommerce.Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;
    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    //GET: api/users/{userID}
    [HttpGet("{userID}")]
    public async Task<IActionResult> GetUserByUserID(Guid userID)
    {
        // Simulate an error for demonstration purposes
        //await ErrorSimulator.GetError();
        //await ErrorSimulator.AddDelayInMilliseconds(10000);

        if (userID == Guid.Empty)
        {
            return BadRequest("Invalid user ID");
        }
        UserDTO? response = await _usersService.GetUserByUserID(userID);
        if(response is null)
        {
            return NotFound(response);
        }
        return Ok(response);
    }
}
