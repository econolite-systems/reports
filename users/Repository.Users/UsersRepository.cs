// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.Users.Api;
using Econolite.Ode.Models.Users.Dto;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;

namespace Econolite.Ode.Repository.Users;

public class UsersRepository : IUsersRepository
{
    private readonly string _identityApiPath;
    private readonly HttpClient _httpClient;

    public UsersRepository(IConfiguration config, HttpClient httpClient)
    {
        _identityApiPath = config["Authentication:Api"] ?? throw new NullReferenceException("Authentication:Api missing from config");
        _httpClient = httpClient;
    }

    public async Task<ICollection<UserDto>> GetUsers(string authScheme, string authToken, string[]? usernames, bool? locked)
    {
        var result = new List<UserDto>();

        
        if (usernames?.Length > 0)
        {
            foreach (var username in usernames)
            {
                var args = new List<(string key, string value)>();
                if (locked != null)
                    args.Add(("enabled", (!locked.Value).ToString()));
                if (!string.IsNullOrWhiteSpace(username))
                    args.Add(("username", HttpUtility.UrlEncode(username)));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
                var users = await _httpClient.GetFromJsonAsync<UserModel[]>($"{_identityApiPath}/users?{string.Join("&", args.Select(a => $"{a.key}={a.value}"))}");
                if (users != null)
                {
                    result.AddRange(users.Select(u => u.ToDto()));
                }
            }
        }
        else
        {
            var args = new List<(string key, string value)>();
            if (locked != null)
                args.Add(("enabled", (!locked.Value).ToString()));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
            var users = await _httpClient.GetFromJsonAsync<UserModel[]>($"{_identityApiPath}/users?{string.Join("&", args.Select(a => $"{a.key}={a.value}"))}");
            if (users != null)
            {
                result.AddRange(users.Select(u => u.ToDto()));
            }
        }

        return result;
    }
}
