using AuthServer.Core.DTOs;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AuthServer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : CustomBaseController
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        //api/auth/CreateToken
        [HttpPost]
        public async Task<IActionResult> CreateToken(LoginDto loginDto)
        {
            var result = await _authenticationService.CreateTokenAsync(loginDto);
            return ActionResultInstance(result);

            // Yukarıda ActionResultInsance olarak tanımladığımız metot,
            //if(result.StatusCode == 200)
            //{
            //    return Ok(result);
            //}else if (result.StatusCode == 404)
            //{
            //    return NotFound();
            //}
        }

        [HttpPost]
        public IActionResult CreateTokenByClient(ClientLoginDto clientLoginDto)
        {
            var result = _authenticationService.CreateTokenByClient(clientLoginDto);
            return ActionResultInstance(result);
        }

        [HttpPost]
        public async Task<IActionResult> RevokeRefleshToken(RefleshTokenDto refleshTokenDto)
        {
            var result = await _authenticationService.RevokeRefleshToken(refleshTokenDto.RefleshToken);
            return ActionResultInstance(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTokenByRefleshToken(RefleshTokenDto refleshTokenDto)
        {
            var result = await _authenticationService.CreateTokenByRefleshTokenAsync(refleshTokenDto.RefleshToken);
            return ActionResultInstance(result);
        }

    }
}
