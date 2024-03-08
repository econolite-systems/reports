// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Models.AuditReport.Dto;

public class AuditReportDto
{
    public string EventType { get; set; } = string.Empty;
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public string Username { get; set; } = string.Empty;

    public string Details { get; set; } = string.Empty;
}
