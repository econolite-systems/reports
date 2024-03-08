// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.AuditReport.Dto;
using Econolite.Ode.Models.Users.Dto;
using System.Text.Json;

namespace Econolite.Ode.Models.AuditReport.Api;

public class UserEvent
{
    public long Time { get; set; }
    
    public string? Type { get; set; } = string.Empty;
    
    public string? RealmId { get; set; } = string.Empty;

    public string? ClientId { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public Guid SessionId { get; set; }

    public string? IpAddress { get; set; } = string.Empty;

    public dynamic? Details { get; set; }

    public AuditReportDto ToDto(ICollection<UserDto>? users, bool? details)
    {
        var user = users?.FirstOrDefault(u => u.Id == UserId);
        return new AuditReportDto
        {
            StartDate = DateTime.UnixEpoch.AddMilliseconds(Time),
            EndDate = DateTime.UnixEpoch.AddMilliseconds(Time),
            EventType = Type ?? "User event",
            Username = user?.Username ?? user?.Email ?? $"Unknown user ({UserId})",
            Details = (details == true) ? (Details is JsonElement ? JsonSerializer.Serialize(Details) : string.Empty) : string.Empty,
        };
    }
}
