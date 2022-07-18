using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApi.Configurations;
using WebApi.Models;
using WebApi.Models.DTOs;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        

        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto requestDto)
        {
            // Validate the incoming request
            if (ModelState.IsValid)
            {
                // We need to check if the email already exist
                var userExist = await _userManager.FindByEmailAsync(requestDto.Email);

                if(userExist != null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Result = true,
                        Errors = new List<string>()
                        {
                            "Email already exist"
                        }
                    });
                }

                // Create a user
                var newUser = new IdentityUser()
                {
                    Email = requestDto.Email,
                    UserName = requestDto.Name,
                };



                var isCreated = await _userManager.CreateAsync(newUser, requestDto.Password);

                if (isCreated.Succeeded)
                {
                    // Generate the token
                    var token = GenerateJwtToken(newUser);

                    return Ok(new AuthResult()
                    {
                        Result = true,
                        Token = token
                    });

                }

                return BadRequest(new AuthResult()
                {
                    Errors = new List<string>()
                    {
                        "Server Error"
                    },
                    Result = false
                });
            }

            return BadRequest();
        }

        [HttpPost]
        [Route("Register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] UserRegistrationRequestDto requestDto)
        {
            if (ModelState.IsValid)
            {
                var userExist = await _userManager.FindByEmailAsync(requestDto.Email);

                if (userExist != null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Result = true,
                        Errors = new List<string>()
                        {
                            "Email already exist"
                        }
                    });

                    var newUser = new IdentityUser()
                    {
                        Email = requestDto.Email,
                        UserName = requestDto.Name,
                    };

                    var isCreated = await _userManager.CreateAsync(newUser, requestDto.Password);

                    if (isCreated.Succeeded)
                    {
                        var token = GenerateJwtToken(newUser);

                        return Ok(new AuthResult()
                        {
                            Result = true,
                            Token = token
                        });
                    }

                    if(!await _roleManager.RoleExistsAsync(UserRoles.Admin.ToString())) // &&&
                    {
                        await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin.ToString()));
                    }
                    if(!await _roleManager.RoleExistsAsync(UserRoles.User.ToString()))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(UserRoles.User.ToString()));
                    }

                    if(await _roleManager.RoleExistsAsync(UserRoles.Admin.ToString()))
                    {
                        await _userManager.AddToRoleAsync(newUser, UserRoles.Admin.ToString());
                    }
                }
            }

            return BadRequest();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto loginRequest)
        {
            if (ModelState.IsValid)
            {
                // Check if the user exist
                var existingUser = await _userManager.FindByEmailAsync(loginRequest.Email);

                if(existingUser == null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>
                        {
                            "Invalid payload"
                        },
                        Result = false
                    });
                }

                var isCorrect = await _userManager.CheckPasswordAsync(existingUser, loginRequest.Password);

                if (!isCorrect)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>
                        {
                            "Invalid credentials"
                        },
                        Result = false
                    });
                }

                var userRoles = await _userManager.GetRolesAsync(existingUser);

                //var jwtToken = GenerateJwtToken(existingUser);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, existingUser.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach(var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtConfig:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JwtConfig:Issuer"],
                    audience: _configuration["JwtConfig:Audience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new AuthResult()
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Result = true
                });
            }

            //return Unauthorized();
            return BadRequest(new AuthResult()
            {
                Errors = new List<string>()
                {
                    "Invalid payload"
                },
                Result = false
            });

        }

        private string GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwtConfig:Secret").Value);

            // Token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, value:user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
                }),

                Expires = DateTime.Now.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }
    }
}

