﻿using AutoMapper;
using Data;
using DTOs.RequestObject;
using DTOs.ResponseObject;
using enforcerWeb.Helper;
using Infrastructures.EmailServices;
using Infrastructures.IdentityProviders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace enforcerWeb.Controllers
{
    [Route("api/[controller]")]    //api/Accounts
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediatR;
        private readonly UserManager<EnforcerUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IGeneralService _generalService;
        private readonly JwtSettings _jwtSettings;

        #region Constructor
        public AccountController(
            IMediator mediatR,
            IOptions<JwtSettings> jwtSettings,
            UserManager<EnforcerUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper,
            IConfiguration configuration,
            IUserService userService,
            IGeneralService generalService)
        {
            _mediatR = mediatR;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _configuration = configuration;
            _userService = userService;
            _generalService = generalService;
            _jwtSettings = jwtSettings.Value;
        }
        #endregion

        [HttpPost("Therapist")]
        public async Task<IActionResult> RegisterATherapist([FromBody] AppUserDTO model)
        {

            try
            {
                var response = await CreateAppUser(model, "Physiotherapist");
                if (response.ReturnModel != null && response.ReturnModel.Status == false)
                {
                    Log.Fatal($"{response.ReturnModel.Response}>>>>>{JsonConvert.SerializeObject(model)}");
                    return StatusCode(200, response.ReturnModel);
                }


                var physio = _mapper.Map<PhysiotherapistDTO>(model);
                physio.IsVerified = false;
                physio.ID = Guid.Parse(response.User.Id);
                physio.DateCreated = DateTime.Now;
                await _userService.CreatePhysiotherapist(physio);

                //generate token
                var jwtTokenResponse = await GenerateJwtToken(response.User);
                jwtTokenResponse.Response = "Successful";

                return Ok(jwtTokenResponse);
            }
            catch (Exception ex)
            {
                Log.Fatal($"{ex.Message ?? ex.InnerException.Message}>>>>>{JsonConvert.SerializeObject(model)}");
                return StatusCode(200, new ResponseModel { Status = false, Errors = { ex.Message } });
            }
        }

        [HttpPost("Patient")]
        public async Task<IActionResult> RegisterAPatient([FromBody] AppUserDTO model)
        {

            try
            {
                var response = await CreateAppUser(model, "Patient");
                if (response.ReturnModel != null && response.ReturnModel.Status == false)
                {
                    Log.Fatal($"{response.ReturnModel.Response}>>>>>{JsonConvert.SerializeObject(model)}");
                    return StatusCode(200, response.ReturnModel);
                }
                var patient = _mapper.Map<PatientDTO>(model);
                patient.ID = Guid.Parse(response.User.Id);
                patient.DateCreated = DateTime.Now;
                await _userService.CreatePatient(patient);



                //Generate token
                var jwtTokenResponse = await GenerateJwtToken(response.User);
                return Ok(jwtTokenResponse);
            }
            catch (Exception ex)
            {
                Log.Fatal($"{ex.Message ?? ex.InnerException.Message}>>>>>{JsonConvert.SerializeObject(model)}");
                return StatusCode(200, new ResponseModel { Status = false, Errors = { ex.Message } });
            }
        }

        private async Task<(EnforcerUser User, ResponseModel ReturnModel)> CreateAppUser(AppUserDTO model, string roleName)
        {
            var emailToUse = !string.IsNullOrWhiteSpace(model.Email) ? model.Email : !string.IsNullOrWhiteSpace(model.PhoneNumber) ? $"{model.PhoneNumber}@productdrive.com" : "";
            if (string.IsNullOrWhiteSpace(emailToUse))
            {
                return (null, new ResponseModel { Status = false, Errors = new List<string>() { "Email Or Phone number is required!" }, Response = "Email is required!" });
            }
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                return (null, new ResponseModel { Status = false, Errors = new List<string>() { "Password is required!" }, Response= "Password is required!" });
            }

            model.PhoneNumber = FormatPhoneNumber(model.PhoneNumber);
            var userExists = await _userManager.FindByNameAsync(emailToUse);

            if (userExists != null)
            {
                return (null, new ResponseModel { Status = false, Errors = new List<string>() { "User already exists!" }, Response= "User already exists!" });
            }
            

            if (userExists == null)
            {
                Guid userIdToUse = Guid.NewGuid();

                var newUser = new EnforcerUser
                {
                    Id = userIdToUse.ToString(),
                    UserName = emailToUse,
                    Email = emailToUse,
                    PhoneNumber = model.PhoneNumber
                };

                var result = await _userManager.CreateAsync(newUser, model.Password);

                if (result.Succeeded)
                {
                    await AssignUserToRole(newUser, roleName);
                    return (newUser, null);
                }
                else
                {
                    return (null, new ResponseModel { Status = false, Errors = result.Errors.Select(x => x.Description).ToList() });
                }

            }
            return (null, new ResponseModel { Status = false, Errors = new List<string>() { "Invalid payload!" } });
        }
        // TODO: format phone number internationally using phone util
        private string FormatPhoneNumber(string phone)
        {
            //check null or empty OR check if less than 11 save it like that OR check if 14
            if (string.IsNullOrWhiteSpace(phone) || phone.Length < 11 || phone.Length == 14)
            {
                return phone;
            }

            //check if its 11 remove first 0 add +234
            if (phone.Length == 11)
            {
                if (phone.First() == '0')
                {
                    var trimmed = phone.Remove(0,1);
                    phone = string.Concat("+234", trimmed);
                }
            }

            return phone;
        }

        private async Task<ResponseModel> GenerateJwtToken(EnforcerUser user) //IdentityUser user
        {
            //get the user's roles
            var roles = await _userManager.GetRolesAsync(user);
            DateTime expirationDay; // NEED TO SPECIFY EXPIRATION TIME
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token;

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JwtSettings:Secret"]));
            var configExpTime = Encoding.ASCII.GetBytes(_configuration["JwtSettings:ExpirationTime"]);
            var correctExp = Encoding.ASCII.GetChars(configExpTime);
            expirationDay = DateTime.UtcNow.AddHours(double.Parse(correctExp));

            List<Claim> subjectClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();
            subjectClaims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            subjectClaims.Add(new Claim(ClaimTypes.Email, user.Email));
            subjectClaims.Add(new Claim(ClaimTypes.Sid, user.Id.ToString()));
            subjectClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            subjectClaims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Email));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(subjectClaims.AsEnumerable()),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                Expires = expirationDay

            };

            token = tokenHandler.CreateToken(tokenDescriptor);
            //convert token to string
            var jwtToken = tokenHandler.WriteToken(token);

            var response = new
            {
                UserID = Guid.Parse(user.Id),
                Token = jwtToken,
                Email = user.Email,
                ExpiryTime = expirationDay,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList()
            };

            return new ResponseModel { Status = true, ReturnObj = response, Response="Successful" };

        }

        private async Task<bool> AssignUserToRole(EnforcerUser user, string roleName)
        {
            try
            {
                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                    return true;
                }
                else
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                    var roleResult = await _userManager.AddToRoleAsync(user, roleName);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"{ex.Message ?? ex.InnerException.Message}>>>>> RoleName:{roleName}");
                return false;
            }
        }

        [HttpPost("uploadCredentials")]
        public async Task<ResponseModel> UploadVerificationDocs([FromForm] FileDTO stuff) => await _userService.PhysiotherapistVerificationFilesUpload(stuff); 


        [HttpPost("SignIn")]
        public async Task<ResponseModel> Login(LoginDTO model)
        {
            var emailToUse = !string.IsNullOrWhiteSpace(model.Email) ? model.Email : !string.IsNullOrWhiteSpace(model.Phone) ? $"{model.Phone}@productdrive.com" : "";
            if (string.IsNullOrWhiteSpace(emailToUse))
            {
                return new ResponseModel {  Status = false, Response = "All fields are required" };
            }
            model.Email = emailToUse;
            return await LogUserIn(model);

        }

        private async Task<ResponseModel> LogUserIn(LoginDTO model)
        {
            //get the user
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return new ResponseModel { Status = false, Response = "Account with this email does not exist!" };
            }

            //get the user's roles
            var roles = await _userManager.GetRolesAsync(user);

            //check that the user is not null and that his password is correct
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var jwtTokenResponse = await GenerateJwtToken(user);
                return jwtTokenResponse;
            }
            //return an authorization error if the checks fail
            Log.Fatal($"Username or password invalid, please try again with correct details.>>>>>{JsonConvert.SerializeObject(model)}");
            return new ResponseModel { Response = "Username or password invalid, please try again with correct details.", Status = false };
        }

        [HttpGet("dashboard")]
        [Authorize(Policy = "AppUser")]
        public async Task<ResponseModel> GetDashboard()
        {
            var res = new ResponseModel();
            var myClaims = User.Claims;
            try
            {
                var userID = myClaims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                var userRoles = myClaims.First(x => x.Type == ClaimTypes.Role).Value;

                res = userRoles == "Patient" ? await _generalService.GetPatientDashboardData(Guid.Parse(userID)) : await _generalService.GetTherapistDashboardData(Guid.Parse(userID));
            }
            catch (Exception)
            {
                return new ResponseModel { Status = false, Response = "Failed: Session expired. Kindly login again" };
            }

            return res;
        }

       [HttpPost("ForgotPassword/{email}")]
        public async Task<ActionResult<ResponseModel>> ForgotPassword([FromRoute] string email)
        {
            //check that the email is not empty
            if (string.IsNullOrEmpty(email))
                //return an error if it is
                return BadRequest(new ResponseModel { Response = "Supply user email", Status = false });

            try
            {
                //check if the user exists
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                    //if not return an error
                    return BadRequest(new ResponseModel { Response = "Oops seems you do not have an account with us. You can easily register now", Status = false });

                //generate a reset token from OTPGenerator class
                 var Token = OTPGenerator.GetOTPHash("eagi");
                 
                //send  the user an email template containing a reset link and OTP
                await _mediatR.Send(new EmailSenderCommand
                {
                    Contacts = new List<ContactsModel>()
                {
                    new ContactsModel
                    {
                        Email = "afeexclusive@gmail.com",
                    },
                    new ContactsModel
                    {
                        Email = email,
                    }
                },
                    EmailDisplayName = "Health Enforcer",
                    Message = $"You attempted to reset your password on Health Enforcer app. \n Here is your OTP: <b>{Token.otp}</b> which will expire soon. {Environment.NewLine} If this token expires kindly request another one by using the forgot password feature on the app again. {Environment.NewLine} If this action is not initiated by you, kindly contact Health Enforcer Admin @ henforce@gamil.com. {Environment.NewLine} Thanks",
                    Subject = "Password Reset OTP"
                }) ;

                return Ok(new ResponseModel { Response = "One Time Password sent successfully", Status = true, ReturnObj = Token.hash});
            }
            catch (Exception ex)
            {

                return Ok(new ResponseModel { Response = ex.InnerException.Message ?? ex.Message, Status = true });
            }
        }

        [HttpPost("passwordreset")]
        public async Task<ResponseModel> ResetPassword(PasswordResetDTO model)
        {
            try
            {
                // Find the user by email
                var user = await _userManager.FindByEmailAsync(model.Email);


                if (user == null)
                {
                    return new ResponseModel {  Status = false, Response="User not found"};
                }

                if (user.UserName != user.Email)
                {
                    user.UserName = user.Email;
                    user.NormalizedUserName = user.Email.ToUpper();
                    await _userManager.UpdateAsync(user);
                }



                // reset the user password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
                if (result.Succeeded)
                {

                    // Login the user
                    var loginmodel = new LoginDTO() { Email = user.Email, Password = model.Password };
                    var isLoginSuccessfull = await LogUserIn(loginmodel);
                    return isLoginSuccessfull;
                }

            }
            catch (Exception ex)
            {

                return new ResponseModel { Status = false, Response = ex.InnerException.Message };
            }
            return new ResponseModel { Status = false, Response = "something went wrong" }; ;
        }

        
        [HttpGet]
        [Authorize(Policy = "AppUser")]
        public async Task<ResponseModel> GetMyProfile()
        {
            var res = new ResponseModel();
            var myClaims = User.Claims;
            try
            {
                var userID = myClaims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                var userRoles = myClaims.First(x => x.Type == ClaimTypes.Role).Value;

                res = userRoles == "Patient" ? await _userService.GetAPatient(Guid.Parse(userID)) : await _userService.GetAPhysioTherapist(Guid.Parse(userID));
            }
            catch (Exception ex)
            {
                Log.Fatal($"{ex.Message ?? ex.InnerException.Message}>>>>> Trying to get App user");
                return new ResponseModel { Status = false, Response = "Failed: Session expired. Kindly login again" };
            }

            return res;
        }

        [HttpPost]
        [Route("edit/therapist/{Id}")]
        [Authorize(Policy = "Therapist")]
        public async Task<ResponseModel> EditPhysiotherapist(Guid Id, PhysiotherapistDTO request) => await _userService.UpdatePhysiotherapist(Id, request);

        [HttpPut("Patient")]
        [Authorize(Policy = "Patient")]
        public async Task<ResponseModel> EditPatient(Guid Id, PatientDTO request) => await _userService.UpdatePatient(Id, request);


        //TODO: write verification endpoint here
    }

}
