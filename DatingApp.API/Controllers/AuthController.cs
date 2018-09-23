using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository repository;
        private readonly IConfiguration config;
        private readonly IMapper mapper;
        public AuthController(IAuthRepository repository, IConfiguration config, IMapper mapper)
        {
            this.mapper = mapper;
            this.config = config;
            this.repository = repository;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            if (await this.repository.UserExists(userForRegisterDto.Username))
            {
                return BadRequest("Username already exists!");
            }

            var userToCreate = mapper.Map<User>(userForRegisterDto);

            var createdUser = await this.repository.Register(userToCreate, userForRegisterDto.Password);

            var userToRturn = mapper.Map<UserForDetailedDto>(createdUser);

            return CreatedAtRoute("GetUser",new {controller = "Users", id = createdUser.Id}, userToRturn );
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepository = await this.repository.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
            if (userFromRepository == null) return Unauthorized();

            var claims = new[]{
                new Claim(ClaimTypes.NameIdentifier,userFromRepository.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepository.Username),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds,
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var user = mapper.Map<UserForListDto>(userFromRepository);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                user
            });
        }
    }
}