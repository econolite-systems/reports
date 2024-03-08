// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Authorization;
using Econolite.Ode.Models.WrongWayDriver.Status;
using Econolite.Ode.Models.WrongWayDriver.Status.Db;
using Econolite.Ode.Services.WrongWayDriver;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Econolite.Ode.Api.Reports.Controllers
{
    /// <summary>
    /// Controller for querying wrong way driver incidents log entries.
    /// </summary>
    [ApiController]
    [Route("wrong-way-driver-status")]
    [AuthorizeOde(MoundRoadRole.ReadOnly)]
    public class WrongWayDriverStatusController : ControllerBase
    {
        private readonly IWrongWayDriverStatusService _wrongWayDriverService;
        private readonly ILogger<WrongWayDriverStatusController> _logger;

        /// <summary>
        /// Constructs a Wrong Way Driver Incidents controller
        /// </summary>
        /// <param name="wrongWayDriverService">A Wrong Way Driver Incidents log service instance</param>
        /// <param name="logger">Injected logger</param>
        public WrongWayDriverStatusController(IWrongWayDriverStatusService wrongWayDriverService,
            ILogger<WrongWayDriverStatusController> logger)
        {
            _wrongWayDriverService = wrongWayDriverService;
            _logger = logger;
        }

        /// <summary>
        /// Finds wrong way driver incidents matching the given query parameters
        /// </summary>
        /// <remarks>
        /// The start date is mandatory, but the end date is optional. If no end date is given, all incident entries with a 
        /// timestamp from the start date up to the latest will be returned. If an end date is provided, only incident entries
        /// within the date range will be returned.
        /// </remarks>
        /// <param name="startDate">Required start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <response code="200">Returns a list of wrong way driver incidents matching the given query parameters</response>
        [HttpGet("find")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<WrongWayDriversStatusDto>))]
        public async Task<IActionResult> FindAsync([BindRequired] DateTime startDate, DateTime? endDate)
        {
            return Ok(await _wrongWayDriverService.Find(startDate, endDate));
        }

        /// <summary>
        /// Finds the unique active wrong way driver statuses.  A status is considered active if the date is within the active number of days configured in the wrong way driver configs.
        /// If no config has been defined then it will find statuses from the last 7 days.
        ///  If multiple statuses has been reported on the same lat/long it will be grouped into a single record.
        /// </summary>
        /// <response code="200">Returns a list of wrong way driver status entries matching the given query parameters</response>
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<WrongWayDriverStatusMessageDocument>))]
        public async Task<IActionResult> ActiveAsync()
        {
            return Ok(await _wrongWayDriverService.FindActive());
        }
    }
}
