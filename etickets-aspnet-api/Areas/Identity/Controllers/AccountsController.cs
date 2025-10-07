using etickets_aspnet_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace etickets_aspnet_api.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Route("api/Identity/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        #region Fields 

        private readonly IEmailSender emailSender;
        private readonly SignInManager<ApplicationUser> signInManager;
        public UserManager<ApplicationUser> _userManager { get; }
        private readonly IRepository<UserOTP> _userOTP;

        #endregion

        #region Constructor
        // Inject UserManager and EmailSender
        public AccountsController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, SignInManager<ApplicationUser> signInManager, IRepository<UserOTP> userOTP)
        {
            _userManager = userManager;
            this.emailSender = emailSender;
            this.signInManager = signInManager;
            this._userOTP = userOTP;
        }

        #endregion

        #region Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // Create application user from request
            ApplicationUser applicationUser = new()
            {
                UserName = request.Username,
                Email = request.Email,
            };

            // Save user with password
            var result = await _userManager.CreateAsync(applicationUser, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await _userManager.AddToRoleAsync(applicationUser, SD.CustomerRole);

            // Generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
            token = WebUtility.UrlEncode(token);

            var link = $"{Request.Scheme}://{Request.Host}/api/Identity/Accounts/ConfirmEmail?userId={applicationUser.Id}&token={token}";

            // Send confirmation email
            await emailSender.SendEmailAsync(
                applicationUser.Email,
                "Confirm Your Account!",
                $"<h1>Confirm your account by clicking <a href='{link}'>here</a></h1>");

            return Ok(new
            {
                msg = "Create User successfully, Confirm Your Email!",
                link = $"{link}"
            });
        }

        #endregion

        #region Confirm Email

        // Confirm user email using token
        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var result = await _userManager.ConfirmEmailAsync(user, WebUtility.UrlDecode(token));

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    msg = "Invalid token, please resend confirmation email"
                });
            }

            return Ok(new
            {
                msg = "Confirm Email successfully"
            });
        }

        #endregion

        #region Login

        [HttpGet("Login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {


            // Try to find user by username or email
            var user = await _userManager.FindByNameAsync(request.EmailORUserName) ?? await _userManager.FindByEmailAsync(request.EmailORUserName);

            if (user is null)
            {
                return NotFound(new { msg = "Invalid User Name/Email" });
            }

            var result = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!result)
            {
                return BadRequest(new { msg = "Invalid User Password" });
            }

            if (!user.EmailConfirmed)
            {
                return BadRequest(new { msg = "Confirm Your Account!" });
            }

            if (!user.LockoutEnabled)
            {
                return BadRequest(new { msg = $"You have a block till {user.LockoutEnd}" });
            }

            //await signInManager.SignInAsync(user, request.RememberMe);

            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim> {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Role, String.Join(",", userRoles)),
                    };

            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("EraaSoft515##EraaSoft515##EraaSoft515##EraaSoft515##")), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                        issuer: "https://localhost:7177",
                        audience: "https://localhost:5000,https://localhost:5500,https://localhost:4200",
                        claims: claims,
                        expires: DateTime.UtcNow.AddDays(1),
                        signingCredentials: signingCredentials
                    );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expires = token.ValidTo
            });
            //return Ok(new { msg = "Login successfully" });
        }
        #endregion 

        #region Logout

        [HttpGet("Logout")]
        public async Task<IActionResult> Logout()
        {
            // Sign out the current user and clear authentication cookies
            await signInManager.SignOutAsync();
            return Ok(new { msg = "Logout Successfully" });
        }

        #endregion

        #region ResendEmailConfirmation


        [HttpPost("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(ForgetPasswordRequest request)
        {

            var user = await _userManager.FindByNameAsync(request.EmailORUserName) ?? await _userManager.FindByEmailAsync(request.EmailORUserName);

            if (user is null)
            {
                return Ok(new { msg = "Invalid User Name/Email" });

            }

            // Send Email confirmation
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var link = $"{Request.Scheme}://{Request.Host}/api/Identity/Accounts/ConfirmEmail?userId={user.Id}&token={token}";

            await emailSender.SendEmailAsync(user.Email!, "Confirm Your Account!", $"<h1>Confirm Your Account By Clicking <a href='{link}'>here</a></h1>");

            return Ok(new { msg = "Send Email successfully, Please Confirm Your Account" });



        }

        #endregion

        #region Forget Password


        [HttpPost("forgetpassword")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.EmailORUserName) ?? await _userManager.FindByEmailAsync(request.EmailORUserName);

            if (user is null)
            {
                return NotFound(new { msg = "Invalid User Name/Email" });
            }

            // Send Email confirmation
            //var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var OTPNumber = new Random().Next(1000, 9999);

            await emailSender.SendEmailAsync(user.Email!, "Reset Your Account!", $"Use this OTP Number: <b>{OTPNumber}</b> to reset your account. Don't share it.");

            await _userOTP.AddAsync(new UserOTP()
            {
                ApplicationUserId = user.Id,
                OTPNumber = OTPNumber.ToString(),
                ValidTo = DateTime.UtcNow.AddDays(1)
            });
            await _userOTP.CommitAsync();

            return Ok(new { msg = "Send OTP to your Email successfully, Please check Your Email" });
        }

        [HttpPost("newpassword")]
        public async Task<IActionResult> NewPassword(NewPasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.ApplicationUserId);

            if (user is null)
                return NotFound();

            var lstOTP = (await _userOTP.GetAsync(e => e.ApplicationUserId == request.ApplicationUserId)).OrderBy(e => e.Id).LastOrDefault();
            if (lstOTP is null)
                return NotFound();

            if (lstOTP.OTPNumber != request.OTPNumber)
            {
                return BadRequest(new
                {
                    msg = "Invalid OTP"
                });
            }
            if (lstOTP.ValidTo > DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    msg = "Expired OTP"
                });
            }
            else
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, request.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
                return RedirectToAction("Login", "Account", new { area = "Identity", userId = user.Id });
            }

        }


        #endregion
    }
}
