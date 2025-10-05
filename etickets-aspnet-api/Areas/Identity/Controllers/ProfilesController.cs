using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace etickets_aspnet_api.Areas.Identity.Controllers
{
    [Area(SD.IdentityArea)]
    [Route("api/identity/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfilesController : ControllerBase
    {
        #region Fields
        private readonly UserManager<ApplicationUser> _userManager;
        #endregion

        #region Constructore
        public ProfilesController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        #endregion

        #region ProfileInfo
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();
            var personalInformation = user.Adapt<PersonalInformationResponse>();

            return Ok(personalInformation);
        }
        #endregion

        #region Update info
        [HttpPut]
        public async Task<IActionResult> UpdateInfo(UpdateInformationRequest request)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            user.Name = request.Name;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.Street = request.Street;
            user.City = request.City;
            user.State = request.State;
            user.ZipCode = request.ZipCode;

            await _userManager.UpdateAsync(user);

            return Ok(new {msg = "Update Info Successfully" });
        }
        #endregion

        #region Change Password
        [HttpPatch]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok("Update Password Successfully");
        }
        #endregion
    }
}

