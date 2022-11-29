using AutoMapper;
using Data;
using DTOs.RequestObject;
using DTOs.ResponseObject;
using Infrastructures.IdentityProviders;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
        private readonly JwtSettings _jwtSettings;

        public AccountController(
            IMediator mediatR,
            IOptions<JwtSettings> jwtSettings,
            UserManager<EnforcerUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper,
            IConfiguration configuration,
            IUserService userService)
        {
            _mediatR = mediatR;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _configuration = configuration;
            _userService = userService;
            _jwtSettings = jwtSettings.Value;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> RegisterATherapist([FromBody] AppUserDTO model)
        {

            try
            {
                var response = await CreateAppUser(model, "Physiotherapist");
                if (response.ReturnModel != null && response.ReturnModel.Status == false)
                {
                    return StatusCode(500, response.ReturnModel);
                }


                var physio = _mapper.Map<AppUserDTO, PhysiotherapistDTO>(model);
                physio.IsVerified = false;
                await _userService.CreatePhysiotherapist(physio);

                //generate token
                var jwtTokenResponse = await GenerateJwtToken(response.User);
                jwtTokenResponse.Response = "Successful";

                return Ok(jwtTokenResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel { Status = false, Errors = { ex.Message } });
            }
        }


        public async Task<IActionResult> RegisterAPatient([FromBody] AppUserDTO model)
        {

            try
            {
                var response = await CreateAppUser(model, "Patient");
                if (response.ReturnModel != null && response.ReturnModel.Status == false)
                {
                    return StatusCode(500, response.ReturnModel);
                }
                var patient = _mapper.Map<AppUserDTO, PatientDTO>(model);
                await _userService.CreatePatient(patient);



                //Generate token
                var jwtTokenResponse = await GenerateJwtToken(response.User);
                return Ok(jwtTokenResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel { Status = false, Errors = { ex.Message } });
            }
        }

        private async Task<(EnforcerUser User, ResponseModel ReturnModel)> CreateAppUser(AppUserDTO model, string roleName)
        {
            var userExists = await _userManager.FindByNameAsync(model.Email);

            if (userExists != null)
            {
                return (null, new ResponseModel { Status = false, Errors = new List<string>() { "User already exists!" } });
            }
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return (null, new ResponseModel { Status = false, Errors = new List<string>() { "Email is required!" } });
            }
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                return (null, new ResponseModel { Status = false, Errors = new List<string>() { "Password is required!" } });
            }

            if (userExists == null)
            {
                Guid userIdToUse = Guid.NewGuid();

                var newUser = new EnforcerUser
                {
                    Id = userIdToUse.ToString(),
                    UserName = model.Email,
                    Email = model.Email,
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
            catch (Exception)
            {
                return false;
            }
        }
        [HttpPost("SignIn")]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            var emailToUse = !string.IsNullOrWhiteSpace(model.Email) ? model.Email : !string.IsNullOrWhiteSpace(model.Phone) ? $"{model.Phone}@elo.com" : "";
            if (string.IsNullOrWhiteSpace(emailToUse))
            {
                return BadRequest("All fields are required");
            }

            return Ok(await LogUserIn(model));

        }

        private async Task<ResponseModel> LogUserIn(LoginDTO model)
        {
            var emailToUse = !string.IsNullOrWhiteSpace(model.Email) ? model.Email : !string.IsNullOrWhiteSpace(model.Phone) ? $"{model.Phone}@elo.com" : "";

            //get the user
            var user = await _userManager.FindByEmailAsync(emailToUse);

            if (user == null)
            {
                return new ResponseModel { Status = false, Response = "Account with this email does not exist!" };
            }

            //get the user's roles
            var roles = await _userManager.GetRolesAsync(user);

            //check that the user is not null and that his password is correct
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var jwtTokenResponse  = await GenerateJwtToken(user);
                return jwtTokenResponse;
            }
            //return an authorization error if the checks fail
            return new ResponseModel { Response = "Username or password invalid, please try again with correct details.", Status = false };
        }

    }
    
}
