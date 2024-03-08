// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Authorization;
using Econolite.Ode.Models.ConnectedVehicle.Status;
using Econolite.Ode.Models.ConnectedVehicle.Status.Db;
using Econolite.Ode.Services.ConnectedVehicle;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Econolite.Ode.Api.Reports.Controllers;

/// <summary>
/// Controller for querying connected vehicle log entries
/// </summary>
[ApiController]
[Route("connected-vehicle-status")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class ConnectedVehicleStatusController : ControllerBase
{
    private readonly IConnectedVehicleStatusService _connectedVehicleService;

    /// <summary>
    /// Constructs a Connected Vehicle Status controller
    /// </summary>
    /// <param name="connectedVehicleService">A connected vehicle service instance</param>
    public ConnectedVehicleStatusController(IConnectedVehicleStatusService connectedVehicleService)
    {
        _connectedVehicleService = connectedVehicleService;
    }

    /// <summary>
    /// Finds connected vehicle messages matching the given query parameters
    /// </summary>
    /// <remarks>
    /// The start date is mandatory, but the end date is optional. If no end date is given, all log entries with a 
    /// timestamp from the start date up to the latest will be returned. If an end date is provided, only entries
    /// within the date range will be returned.
    /// </remarks>
    /// <param name="startDate">Required start date</param>
    /// <param name="endDate">Optional end date</param>
    /// <response code="200">Returns a list of connected vehicle log messages matching the given query parameters</response>
    [HttpGet("find")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ConnectedVehicleMessageDocument>))]
    public async Task<IActionResult> FindAsync([FromQuery] [BindRequired] DateTime startDate,
        DateTime? endDate)
    {
        return Ok(await _connectedVehicleService.FindAsync(startDate, endDate));
    }

    /// <summary>
    /// Gets the connected vehicle log data totals by message type
    /// </summary>
    /// <response code="200">Returns a list of connected vehicle log entries grouped by the type</response>
    [HttpGet("getTotalsByMessageType")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ConnectedVehicleMessageTypeCountAndSize>))]
    public async Task<IActionResult> GetTotalsByMessageTypeAsync()
    {
        return Ok(await _connectedVehicleService.GetTotalsByMessageTypeAsync());
    }

    /// <summary>
    /// Gets the connected vehicle log data totals by repository type
    /// </summary>
    /// <response code="200">Returns a list of connected vehicle log entries grouped by the type</response>
    [HttpGet("getTotalsByRepositoryType")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ConnectedVehicleRepositoryTypeCountAndSize>))]
    public async Task<IActionResult> GetTotalsByRepositoryTypeAsync()
    {
        return Ok(await _connectedVehicleService.GetTotalsByRepositoryTypeAsync());
    }

    /// <summary>
    /// Gets the last 60 mins of connected vehicle log data totals by message type
    /// </summary>
    /// <response code="200">Returns a list of connected vehicle log entries totals grouped by the type</response>
    [HttpGet("getLastHourTotalsByMessageType")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ConnectedVehicleMessageCount>))]
    public async Task<IActionResult> GetLastHourTotalsByMessageTypeAsync()
    {
        return Ok(await _connectedVehicleService.GetLastHourTotalsByMessageTypeAsync());
    }

    /// <summary>
    /// Gets the total count of the connected vehicle log messages
    /// </summary>
    /// <response code="200">Returns a count of all the messages in the connected vehicle log</response>
    [HttpGet("getTotalMessageCount")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
    public async Task<IActionResult> GetTotalMessageCountAsync()
    {
        return Ok(await _connectedVehicleService.GetTotalMessageCountAsync());
    }

    /// <summary>
    /// Gets the intersection totals by message type and intersection
    /// </summary>
    /// <response code="200">Returns a list of connected vehicle log intersection message totals by the type</response>
    [HttpGet("getIntersectionTotalsByMessageType")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ConnectedVehicleIntersectionTypeCountAndSize>))]
    public async Task<IActionResult> GetIntersectionTotalsByMessageTypeAsync()
    {
        return Ok(await _connectedVehicleService.GetIntersectionTotalsByMessageTypeAsync());
    }

}
