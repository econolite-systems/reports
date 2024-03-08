// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Authorization;
using Econolite.Ode.Messaging.Elements;
using Econolite.Ode.Models.Tim.Db;
using Econolite.Ode.Models.Tim.Dto;
using Econolite.Ode.Repository.TimService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Econolite.Ode.Api.Reports.Controllers;

/// <summary>
/// A controller for querying current and historical TIM entries
/// </summary>
[Route("tim-status")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class TimStatusController : ControllerBase
{
    private readonly ITimRsuStatusRepository _timRepository;

    /// <summary>
    /// Constructs a TIM controller
    /// </summary>
    public TimStatusController(
        ITimRsuStatusRepository timRepository
    )
    {
        _timRepository = timRepository;
    }

    /// <summary>
    /// Returns active TIM entries
    /// </summary>
    /// <response code="200">Returns a list of active TIM entries</response>
    [HttpGet("find-active")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TimDocument>))]
    public async Task<IActionResult> FindAsync()
    {
        var messages = await _timRepository.FindActive();
        return Ok(messages);
    }

    /// <summary>
    /// Returns historical TIM entries with a creation date within the given time range
    /// </summary>
    /// <response code="200">Returns a list of TIM entries matching the given query parameters</response>
    [HttpGet("find")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TimDocument>))]
    public async Task<IActionResult> FindAsync(
        [FromQuery] [BindRequired] DateTime startDate,
        [FromQuery] DateTime? endDate
    )
    {
        var messages = await _timRepository.Find(startDate, endDate);
        
        return Ok(messages.ToTimDocument());
    }
}

public static class TimStatusControllerExtensions
{
    public static IEnumerable<TimDocument> ToTimDocument(this IEnumerable<TimRsuStatus> status)
    {
        return status.Select(s => s.ToTimDocument());
    }
    
    public static TimDocument ToTimDocument(this TimRsuStatus status)
    {
        return new TimDocument()
        {
            Id = status.Id,
            IntersectionId = status.IntersectionId,
            RsuId = status.RsuId,
            DeliveryStart = status.DeliveryStart,
            EndDate = status.EndDate,
            State = status.State,
            Source = status.Source,
            CreationDate = status.CreationDate,
            ItisCode = status.ItisCode,
        };
    }
}
