using Microsoft.AspNetCore.Mvc;
using WelfareLinkApi.Interfaces;

namespace WelfareLinkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserApiController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserApiController(IUserService userService)
        {
            _userService = userService;
        }
    }
}
