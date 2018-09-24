using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository repository;
        private readonly IMapper mapper;
        public UsersController(IDatingRepository repository, IMapper mapper)
        {
            this.mapper = mapper;
            this.repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var usersFromRepo = await repository.GetUser(currentUserId);

            userParams.UserId = currentUserId;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = usersFromRepo.Gender == "male" ? "female" : "male";
            }

            var users = await repository.GetUsers(userParams);
            var usersToReturn = mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalItemCount, users.TotalPages);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await repository.GetUser(id);
            var userToReturn = mapper.Map<UserForDetailedDto>(user);
            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) return Unauthorized();

            var userFromRepo = await repository.GetUser(id);
            mapper.Map(userForUpdateDto, userFromRepo);

            if (await repository.SaveAll()) return NoContent();

            throw new Exception($"Updating user with {id} failed on save");
        }
    }
}