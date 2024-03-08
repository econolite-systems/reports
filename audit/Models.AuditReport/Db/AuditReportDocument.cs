// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Econolite.Ode.Models.AuditReport.Db;

public class AuditReportDocument
{
    [BsonElement("EventType")]
    public string EventType { get; set; } = string.Empty;

    [BsonElement("StartDate")]
    public DateTime StartDate { get; set; }

    [BsonElement("EndDate")]
    public DateTime EndDate { get; set; }

    [BsonElement("Username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("Target")]
    public BsonDocument? Target { get; set; }
}
