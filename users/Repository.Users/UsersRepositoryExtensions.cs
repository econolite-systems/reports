// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Microsoft.Extensions.DependencyInjection;

namespace Econolite.Ode.Repository.Users;

public static class UsersRepositoryExtensions
{
    public static IServiceCollection AddUsersRepo(this IServiceCollection services)
    {
        services.AddHttpClient<UsersRepository>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        return services;
    }
}
