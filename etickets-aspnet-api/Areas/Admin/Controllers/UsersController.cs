using etickets_aspnet_api.Models;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using etickets_aspnet_api.Areas.Admin.DTOs.Response;

namespace etickets_aspnet_api.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{SD.AdminRole}, {SD.SuperAdminRole}")]

    public class UsersController : ControllerBase
    {

        #region Fields
        private readonly UserManager<ApplicationUser> _userManager;
        #endregion

        #region Constructor
        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        #endregion

        #region Get All Users (GET: api/admin/user)
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userManager.Users.ToList();

            // Optional mapping using Mapster (convert ApplicationUser → UserDto)
            var userDtos = users.Adapt<List<UserResponse>>();

            return Ok(userDtos);
        }
        #endregion

    }
}
