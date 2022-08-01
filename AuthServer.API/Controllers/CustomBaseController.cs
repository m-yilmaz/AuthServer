using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Dtos;

namespace AuthServer.API.Controllers
{

    public class CustomBaseController : ControllerBase
    {
        public IActionResult ActionResultInstance<T>(Response<T> response) where T : class
        {
            //Ok
            // BadRequest
            // Bi tık üstünde base class
            return new ObjectResult(response)
            {
                StatusCode = response.StatusCode
            };
        }
    }
}
