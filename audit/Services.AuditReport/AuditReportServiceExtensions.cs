// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Microsoft.Extensions.DependencyInjection;

namespace Econolite.Ode.Services.AuditReport
{
    public static class AuditReportServiceExtensions
    {
        public static IServiceCollection AddAuditReportService(this IServiceCollection services)
        {
            services.AddScoped<IAuditReportService, AuditReportService>();
            return services;
        }
    }
}
