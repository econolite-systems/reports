// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Persistence.Mongo.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Econolite.Ode.Models.AuditReport;
using Econolite.Ode.Models.AuditReport.Api;
using Econolite.Ode.Models.AuditReport.Db;
using Econolite.Ode.Models.AuditReport.Dto;
using Econolite.Ode.Models.Users.Dto;
using Econolite.Ode.Repository.Users;
using System.Web;
using Econolite.Ode.Auditing;

namespace Econolite.Ode.Repository.AuditReport
{
    public class AuditReportRepository : IAuditReportRepository
    {
        private readonly IMongoCollection<AuditReportDocument> _auditCollection;
        private readonly string? _identityApiPath;
        private readonly HttpClient _httpClient;
        private readonly IUsersRepository _usersRepository;
        private readonly ILogger<AuditReportRepository> _logger;

        public AuditReportRepository(IConfiguration configuration, IMongoContext mongoContext, HttpClient httpClient, IUsersRepository usersRepository, ILogger<AuditReportRepository> logger)
        {
            _auditCollection = mongoContext.GetCollection<AuditReportDocument>(configuration["Collections:Audit"] ?? throw new NullReferenceException("Collections:Audit missing from configuration."));
            _identityApiPath = configuration["Authentication:Api"] ?? throw new NullReferenceException("Authentication:Api missing from config");
            _httpClient = httpClient;
            _usersRepository = usersRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<AuditReportDto>> FindNonIdentityAuditLogs(DateTime startDate, DateTime? endDate, string[]? eventTypes, string[]? usernames, bool? details)
        {
            var result = new List<AuditReportDto>();

            var auditEventTypeKeys = eventTypes?.Select(x => (AuditEventType)Enum.Parse(typeof(AuditEventType), x)).ToList();
            var auditEventTypeModels = SupportedAuditEventTypes.AuditEventTypes
                        .Where(x => auditEventTypeKeys == null || auditEventTypeKeys?.Count == 0 || auditEventTypeKeys?.Contains(x.Key) == true)
                        .Select(x => x.Value)
                        .ToList();

            if (auditEventTypeModels.Any(x => x.Group != SupportedAuditEventTypes.KeycloakAdminGroup && x.Group != SupportedAuditEventTypes.KeycloakUserGroup))
            {
                try
                {
                    var filter = Builders<AuditReportDocument>.Filter.Where(x => x.StartDate >= startDate);
                    if (endDate != null)
                        filter &= Builders<AuditReportDocument>.Filter.Where(x => x.EndDate <= endDate.Value);
                    if (eventTypes?.Length > 0)
                    {
                        var events = auditEventTypeModels
                                .Where(x => x.Group != SupportedAuditEventTypes.KeycloakAdminGroup && x.Group != SupportedAuditEventTypes.KeycloakUserGroup)
                                .Select(x => x.Event)
                                .ToList();
                        if (events.Count > 0)
                        {
                            var eventFilter = Builders<AuditReportDocument>.Filter.Regex(x => x.EventType, new BsonRegularExpression($"^{events.ElementAt(0)}:.*"));
                            foreach (var ev in events.Select((value, index) => new { Value = value, Index = index }))
                            {
                                if (ev.Index > 0)
                                    eventFilter |= Builders<AuditReportDocument>.Filter.Regex(x => x.EventType, new BsonRegularExpression($"^{ev.Value}:.*"));
                            }
                            filter &= eventFilter;
                        }
                    }
                    if (usernames?.Length > 0)
                        filter &= Builders<AuditReportDocument>.Filter.Where(x => usernames.Contains(x.Username));
                    var cursor = await _auditCollection.FindAsync(filter);
                    result.AddRange((await cursor.ToListAsync()).Select(a => a.ToDto(details)));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to retrieve audit logs from mongo");
                }
            }

            return result.OrderBy(x => x.StartDate);
        }

        public async Task<IEnumerable<AuditReportDto>> FindIdentityAuditLogs(string authScheme, string authToken, DateTime startDate, DateTime? endDate, string[]? eventTypes, string[]? usernames, bool? details)
        {
            var result = new List<AuditReportDto>();

            var auditEventTypeKeys = eventTypes?.Select(x => (AuditEventType)Enum.Parse(typeof(AuditEventType), x)).ToList();
            var auditEventTypeModels = SupportedAuditEventTypes.AuditEventTypes
                        .Where(x => auditEventTypeKeys == null || auditEventTypeKeys?.Count == 0 || auditEventTypeKeys?.Contains(x.Key) == true)
                        .Select(x => x.Value)
                        .ToList();

            ICollection<UserDto>? users = null;
            try
            {
                users = await _usersRepository.GetUsers(authScheme, authToken, usernames, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve users for id lookup");
            }

            if (auditEventTypeModels.Any(x => x.Group == SupportedAuditEventTypes.KeycloakUserGroup))
            {
                // User events
                try
                {
                    // Example URLs
                    // https://keycloak.cosysdev.com/auth/admin/realms/moundroad/events?first=0&max=5
                    // https://keycloak.cosysdev.com/auth/admin/realms/moundroad/events?client=t&dateFrom=2023-05-01&dateTo=2023-05-20&first=0&max=5&user=u
                    // https://keycloak.cosysdev.com/auth/admin/realms/moundroad/events?first=0&max=5&type=IMPERSONATE&type=LOGIN

                    if (usernames?.Length > 0)
                    {
                        foreach (var user in users)
                        {
                            var args = new List<(string key, string value)> { ("first", "0"), ("max", "1000"), ("dateFrom", startDate.ToString("yyyy-MM-dd")) };
                            if (endDate.HasValue)
                                args.Add(("dateTo", endDate.Value.ToString("yyyy-MM-dd")));
                            if (eventTypes?.Length > 0)
                            {
                                var events = auditEventTypeModels
                                    .Where(x => x.Group == SupportedAuditEventTypes.KeycloakUserGroup)
                                    .Select(x => x.Event)
                                    .ToList();
                                if (events?.Count > 0)
                                {
                                    args.AddRange(events.Select(e => ("type", HttpUtility.UrlEncode(e))));
                                }
                            }
                            args.Add(("user", HttpUtility.UrlEncode(user.Id.ToString())));
                            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
                            var response = await _httpClient.GetFromJsonAsync<UserEvent[]>($"{_identityApiPath}/events?{string.Join("&", args.Select(a => $"{a.key}={a.value}"))}");
                            if (response != null)
                            {
                                result.AddRange(response.Select(u => u.ToDto(users, details)));
                            }
                        }
                    }
                    else
                    {
                        var args = new List<(string key, string value)> { ("first", "0"), ("max", "1000"), ("dateFrom", startDate.ToString("yyyy-MM-dd")) };
                        if (endDate.HasValue)
                            args.Add(("dateTo", endDate.Value.ToString("yyyy-MM-dd")));
                        if (eventTypes?.Length > 0)
                        {
                            var events = auditEventTypeModels
                                .Where(x => x.Group == SupportedAuditEventTypes.KeycloakUserGroup)
                                .Select(x => x.Event)
                                .ToList();
                            if (events?.Count > 0)
                            {
                                args.AddRange(events.Select(e => ("type", HttpUtility.UrlEncode(e))));
                            }
                        }
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
                        var response = await _httpClient.GetFromJsonAsync<UserEvent[]>($"{_identityApiPath}/events?{string.Join("&", args.Select(a => $"{a.key}={a.value}"))}");
                        if (response != null)
                        {
                            result.AddRange(response.Select(u => u.ToDto(users, details)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to retrieve user audit logs from identity server");
                }
            }

            if (auditEventTypeModels.Any(x => x.Group == SupportedAuditEventTypes.KeycloakAdminGroup))
            {
                // Admin events
                try
                {
                    // Example URLs
                    // https://keycloak.cosysdev.com/auth/admin/realms/moundroad/admin-events?first=0&max=5
                    // https://keycloak.cosysdev.com/auth/admin/realms/moundroad/admin-events?dateFrom=2023-05-01&dateTo=2023-05-20&first=0&max=5&operationTypes=ACTION&operationTypes=CREATE&operationTypes=DELETE&operationTypes=UPDATE&resourcePath=*e*
                    // https://keycloak.cosysdev.com/auth/admin/realms/moundroad/admin-events?first=0&max=5&resourceTypes=AUTHENTICATOR_CONFIG&resourceTypes=AUTHORIZATION_POLICY
                    // https://keycloak.cosysdev.com/auth/admin/realms/moundroad/admin-events?authClient=b&authIpAddress=d&authRealm=a&authUser=c&dateFrom=2023-05-01&dateTo=2023-05-20&first=0&max=5&operationTypes=ACTION&operationTypes=CREATE&operationTypes=DELETE&operationTypes=UPDATE&resourcePath=*e*

                    if (usernames?.Length > 0)
                    {
                        foreach (var user in users)
                        {
                            var args = new List<(string key, string value)> { ("first", "0"), ("max", "1000"), ("dateFrom", startDate.ToString("yyyy-MM-dd")) };
                            if (endDate.HasValue)
                                args.Add(("dateTo", endDate.Value.ToString("yyyy-MM-dd")));
                            if (eventTypes?.Length > 0)
                            {
                                var events = auditEventTypeModels
                                    .Where(x => x.Group == SupportedAuditEventTypes.KeycloakAdminGroup)
                                    .Select(x => x.Event)
                                    .ToList();
                                if (events?.Count > 0)
                                {
                                    args.AddRange(events.Select(e => ("type", HttpUtility.UrlEncode(e))));
                                }
                            }
                            args.Add(("authUser", HttpUtility.UrlEncode(user.Id.ToString())));
                            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
                            var response = await _httpClient.GetFromJsonAsync<AdminEvent[]>($"{_identityApiPath}/admin-events?{string.Join("&", args.Select(a => $"{a.key}={a.value}"))}");
                            if (response != null)
                            {
                                result.AddRange(response.Select(u => u.ToDto(users, details)));
                            }
                        }
                    }
                    else
                    {
                        var args = new List<(string key, string value)> { ("first", "0"), ("max", "1000"), ("dateFrom", startDate.ToString("yyyy-MM-dd")) };
                        if (endDate.HasValue)
                            args.Add(("dateTo", endDate.Value.ToString("yyyy-MM-dd")));
                        if (eventTypes?.Length > 0)
                        {
                            var events = auditEventTypeModels
                                .Where(x => x.Group == SupportedAuditEventTypes.KeycloakAdminGroup)
                                .Select(x => x.Event)
                                .ToList();
                            if (events?.Count > 0)
                            {
                                args.AddRange(events.Select(e => ("type", HttpUtility.UrlEncode(e))));
                            }
                        }
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
                        var response = await _httpClient.GetFromJsonAsync<AdminEvent[]>($"{_identityApiPath}/admin-events?{string.Join("&", args.Select(a => $"{a.key}={a.value}"))}");
                        if (response != null)
                        {
                            result.AddRange(response.Select(u => u.ToDto(users, details)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to retrieve admin audit logs from identity server");
                }
            }

            return result.OrderBy(x => x.StartDate);
        }
    }
}
