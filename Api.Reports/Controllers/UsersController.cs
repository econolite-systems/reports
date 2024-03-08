// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Microsoft.AspNetCore.Mvc;
using Econolite.Ode.Authorization;
using Econolite.Ode.Models.Users.Dto;
using Econolite.Ode.Repository.Users;

namespace Econolite.Ode.Api.Reports.Controllers
{
    /// <summary>
    /// Controller to get locked users reports.
    /// </summary>
    [ApiController]
    [Route("users")]
    [AuthorizeOde(MoundRoadRole.ReadOnly)]
    public class UsersController : ControllerBase
    {
        private readonly IUsersRepository _usersRepository;

        /// <summary>
        /// Constructs a locked users report controller.
        /// </summary>
        public UsersController(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        /// <summary>
        /// Find locked users
        /// </summary>
        /// <param name="usernames">Optional usernames</param>
        /// <param name="locked">Optional filter for locked or unlocked users</param>
        /// <returns></returns>
        /// <response code="200">Returns a list of locked users</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDto>))]
        public async Task<IActionResult> FindAsync([FromQuery] string[]? usernames, [FromQuery] bool? locked)
        {
            var auth = Request.Headers.Authorization[0]!.Split(" ");
            if (usernames?.Length > 0)
            {
                usernames = usernames[0].Split(",");
            }
            return Ok(await _usersRepository.GetUsers(auth[0], auth[1], usernames, locked));
        }
    }
}
