// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.Users.Dto;

namespace Econolite.Ode.Repository.Users;

public interface IUsersRepository
{
    Task<ICollection<UserDto>> GetUsers(string authScheme, string authToken, string[]? usernames, bool? locked);
}
