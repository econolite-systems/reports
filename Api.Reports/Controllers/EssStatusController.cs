// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Econolite.Ode.Services.Ess;
using Econolite.Ode.Models.Ess.Status;

namespace Econolite.Ode.Api.Reports.Controllers;

/// <summary>
/// Controller for querying environmental sensor status log entries
/// </summary>
[ApiController]
[Route("ess-status")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class EssStatusController : ControllerBase
{
    private readonly IEssStatusService _essStatusService;
    private readonly ILogger<EssStatusController> _logger;

    /// <summary>
    /// Constructs an ESS status controller
    /// </summary>
    /// <param name="logger">Injected logger</param>
    /// <param name="essStatusService">An ESS status service instance</param>
    public EssStatusController(IEssStatusService essStatusService, ILogger<EssStatusController> logger)
    {
        _essStatusService = essStatusService;
        _logger = logger;
    }

    /// <summary>
    /// Returns the latest environmental sensor status for the given device ID
    /// </summary>
    /// <param name="deviceId">A device ID</param>
    /// <response code="200">Returns the latest environmental status entry for the given device ID</response>
    /// <response code="404">Returns nothing if no status entries for the device ID exist</response>
    [HttpGet("latest")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EssStatusDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LatestAsync([BindRequired] Guid deviceId)
    {
        return await _essStatusService.FindLatest(deviceId) switch
        {
            { } result => Ok(result),
            null => NotFound()
        };
    }

    /// <summary>
    /// Returns the latest environmental sensor status entries for all devices
    /// </summary>
    /// <response code="200">Returns a list of the latest environmental sensor statuses for all devices</response>
    [HttpGet("latest/all")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<EssStatusDto>))]
    public async Task<IActionResult> FindAllLatestAsync()
    {
        return Ok(await _essStatusService.FindAllLatest());
    }

    /// <summary>
    /// Finds environmental sensor statuses matching the given query parameters
    /// </summary>
    /// <remarks>
    /// The start date is mandatory, but the end date is optional. If no end date is given, all status entries with a 
    /// timestamp from the start date up to the latest will be returned. If an end date is provided, only status entries
    /// within the date range will be returned.
    /// 
    /// If no device ID parameters are given, then the query will *not* filter on any device IDs, so statuses for
    /// any device will be returned. The device ID parameter may be provided multiple times to filter on multiple
    /// device IDs. If any device IDs are given, only statuses for the given device IDs will be returned.
    /// </remarks>
    /// <param name="deviceId">Optional device ID to filter on</param>
    /// <param name="startDate">Required start date</param>
    /// <param name="endDate">Optional end date</param>
    /// <response code="200">Returns a list of ESS status entries matching the given query parameters</response>
    [HttpGet("find")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<EssStatusDto>))]
    public async Task<IActionResult> FindAsync([FromQuery] string[]? deviceId, [BindRequired] DateTime startDate,
        DateTime? endDate)
    {
        var deviceGuids = new List<Guid>();
        if (deviceId?.Length > 0)
        {
            deviceGuids.AddRange(deviceId[0].Split(",").Select(d => Guid.Parse(d)));
        }

        return Ok(await _essStatusService.Find(deviceGuids, startDate, endDate));
    }
}
