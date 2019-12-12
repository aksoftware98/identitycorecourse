using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetIdentityDemo.Api.Services;
using AspNetIdentityDemo.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspNetIdentityDemo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private IUserService _userService; 

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        // /api/auth/register
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody]RegisterViewModel model)
        {
            if(ModelState.IsValid)
            {
                var result = await _userService.RegisterUserAsync(model);

                if (result.IsSuccess)
                    return Ok(result); // Status Code: 200 

                return BadRequest(result);
            }

            return BadRequest("Some properties are not valid"); // Status code: 400
        }

        // /api/auth/login
        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody]LoginViewModel model)
        {
            if(ModelState.IsValid)
            {
                var result = await _userService.LoginUserAsync(model);

                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }

            return BadRequest("Some properties are not valid");
        }

    }
}