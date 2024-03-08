// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Auditing;
using Econolite.Ode.Authorization;
using Econolite.Ode.Models.AuditReport.Dto;
using Econolite.Ode.Services.AuditReport;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Econolite.Ode.Api.Reports.Controllers
{
    /// <summary>
    /// Controller to get audit reports
    /// </summary>
    [ApiController]
    [Route("audit-report")]
    [AuthorizeOde(MoundRoadRole.Administrator)]
    public class AuditReportController : ControllerBase
    {
        private readonly IAuditReportService _auditReportService;

        /// <summary>
        /// Constructs an audit report controller
        /// </summary>
        public AuditReportController(IAuditReportService auditReportService)
        {
            _auditReportService = auditReportService;
        }

        /// <summary>
        /// Find audit report entries
        /// </summary>
        /// <param name="startDate">Required start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <param name="eventTypes">Optional event types</param>
        /// <param name="usernames">Optional usernames</param>
        /// <param name="details">Optional details</param>
        /// <returns></returns>
        /// <response code="200">Returns a list of audit report entries</response>
        [HttpGet("find")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AuditReportDto>))]
        public async Task<IActionResult> FindAsync([FromQuery][BindRequired] DateTime startDate, [FromQuery] DateTime? endDate, [FromQuery] string[]? eventTypes, [FromQuery] string[]? usernames, [FromQuery] bool? details)
        {
            var auth = Request.Headers.Authorization[0]!.Split(" ");
            if (eventTypes?.Length > 0)
            {
                eventTypes = eventTypes[0].Split(",");
            }
            if (usernames?.Length > 0)
            {
                usernames = usernames[0].Split(",");
            }

            var results1 = await _auditReportService.FindNonIdentityAuditLogs(startDate, endDate, eventTypes, usernames, details);
            var results2 = await _auditReportService.FindIdentityAuditLogs(auth[0], auth[1], startDate, endDate, eventTypes, usernames, details);

            return Ok(results1.Concat(results2).OrderBy(x => x.StartDate));
        }

        /// <summary>
        /// Get audit event types
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Returns a set of supported audit event types</response>
        [HttpGet("getAuditEventTypes")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<AuditEventType, AuditEventTypeModel>))]
        public IActionResult GetAuditEventTypes()
        {
            return Ok(SupportedAuditEventTypes.AuditEventTypes);
        }
    }
}
