// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.AuditReport.Dto;
using Econolite.Ode.Repository.AuditReport;

namespace Econolite.Ode.Services.AuditReport
{
    public class AuditReportService : IAuditReportService
    {
        private readonly IAuditReportRepository _auditReportRepository;

        public AuditReportService(IAuditReportRepository auditReportRepository)
        {
            _auditReportRepository = auditReportRepository;
        }

        public async Task<IEnumerable<AuditReportDto>> FindNonIdentityAuditLogs(DateTime startDate, DateTime? endDate, string[]? eventTypes, string[]? usernames, bool? details)
        {
            return await _auditReportRepository.FindNonIdentityAuditLogs(startDate, endDate, eventTypes, usernames, details);
        }

        public async Task<IEnumerable<AuditReportDto>> FindIdentityAuditLogs(string authScheme, string authToken, DateTime startDate, DateTime? endDate, string[]? eventTypes, string[]? usernames, bool? details)
        {
            return await _auditReportRepository.FindIdentityAuditLogs(authScheme, authToken, startDate, endDate, eventTypes, usernames, details);
        }
    }
}
