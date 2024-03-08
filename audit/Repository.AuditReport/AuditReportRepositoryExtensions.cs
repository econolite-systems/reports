// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Microsoft.Extensions.DependencyInjection;

namespace Econolite.Ode.Repository.AuditReport
{
    public static class AuditReportRepositoryExtensions
    {
        public static IServiceCollection AddAuditReportRepository(this IServiceCollection services)
        {
            services.AddHttpClient<AuditReportRepository>();
            services.AddScoped<IAuditReportRepository, AuditReportRepository>();
            return services;
        }
    }
}
