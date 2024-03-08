// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Authorization;
using Econolite.Ode.Models.Status.Db;
using Econolite.Ode.Services.PavementCondition;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Econolite.Ode.Api.Reports.Controllers;

/// <summary>
/// Controller for querying pavement condition status log entries
/// </summary>
[ApiController]
[Route("pavement-condition-status")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class PavementConditionStatusController : ControllerBase
{
    private readonly IPavementConditionStatusService _pavementConditionService;
    private readonly ILogger<PavementConditionStatusController> _logger;

    /// <summary>
    /// Constructs a Pavement Condition Status controller
    /// </summary>
    /// <param name="pavementConditionService">A pavement condition service instance</param>
    /// <param name="logger">Injected logger</param>
    public PavementConditionStatusController(IPavementConditionStatusService pavementConditionService, ILogger<PavementConditionStatusController> logger)
    {
        _pavementConditionService = pavementConditionService;
        _logger = logger;
    }

    /// <summary>
    /// Finds the unique active pavement condition statuses.  A status is considered active if the date is within the active number of days configured in the pavement condition configs.
    /// If no config has been defined then it will find statuses from the last 7 days.
    ///  If multiple statuses has been reported on the same lat/long it will be grouped into a single record.
    /// </summary>
    /// <response code="200">Returns a list of pavement condition status entries matching the given query parameters</response>
    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PavementConditionStatusMessageDocument>))]
    public async Task<IActionResult> ActiveAsync()
    {
        //get unique lat/long
        return Ok(await _pavementConditionService.FindAsync(true));
    }

    /// <summary>
    /// Finds pavement condition sensor statuses matching the given query parameters
    /// </summary>
    /// <remarks>
    /// The start date is mandatory, but the end date is optional. If no end date is given, all status entries with a 
    /// timestamp from the start date up to the latest will be returned. If an end date is provided, only status entries
    /// within the date range will be returned.
    /// </remarks>
    /// <param name="startDate">Required start date</param>
    /// <param name="endDate">Optional end date</param>
    /// <response code="200">Returns a list of pavement condition status entries matching the given query parameters</response>
    [HttpGet("find")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PavementConditionStatusMessageDocument>))]
    public async Task<IActionResult> FindAsync([FromQuery][BindRequired] DateTime startDate,
        DateTime? endDate)
    {
        return Ok(await _pavementConditionService.FindAsync(startDate, endDate));
    }
}
