// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.AuditReport.Db;
using Econolite.Ode.Models.AuditReport.Dto;
using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace Econolite.Ode.Models.AuditReport;

public static class AuditReportModelsExtensions
{
    public static AuditReportDto ToDto(this AuditReportDocument document, bool? details)
    {
        JsonWriterSettings settings = new JsonWriterSettings
        {
            OutputMode = JsonOutputMode.CanonicalExtendedJson
        };

        return new AuditReportDto
        {
            EventType = document.EventType,
            StartDate = document.StartDate,
            EndDate = document.EndDate,
            Username = document.Username,
            Details = (details == true) ? (document.Target?.ToJson(settings) ?? string.Empty): string.Empty
        };
    }
}
