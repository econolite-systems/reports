// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.AuditReport.Dto;

namespace Econolite.Ode.Repository.AuditReport
{
    public interface IAuditReportRepository
    {
        Task<IEnumerable<AuditReportDto>> FindNonIdentityAuditLogs(DateTime startDate, DateTime? endDate, string[]? eventTypes, string[]? usernames, bool? details);
        Task<IEnumerable<AuditReportDto>> FindIdentityAuditLogs(string authScheme, string authToken, DateTime startDate, DateTime? endDate, string[]? eventTypes, string[]? usernames, bool? details);
    }
}
