// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Api.Reports.Models;
using Econolite.Ode.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Serialization;

namespace Econolite.Ode.Api.Reports.Controllers
{
    /// <summary>
    /// Controller to get Jasper reports
    /// </summary>
    [ApiController]
    [Route("jasper-report")]
    [AuthorizeOde(MoundRoadRole.ReadOnly)]
    public class JasperReportController : ControllerBase
    {
        private readonly ILogger<JasperReportController> _logger;

        private readonly string _reportServerUrl;
        private readonly string _reportServerUsername;
        private readonly string _reportServerPassword;

        /// <summary>
        /// Controller to get Jasper reports
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public JasperReportController(IConfiguration configuration, ILogger<JasperReportController> logger)
        {
            _logger = logger;

            _reportServerUrl = configuration["Jasper:ReportServerUrl"] ?? string.Empty;
            _reportServerUsername = configuration["Jasper:ReportServerUsername"] ?? string.Empty;
            _reportServerPassword = configuration["Jasper:ReportServerPassword"] ?? string.Empty;
        }

        /// <summary>
        /// Get Jasper reports
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Returns a list of Jasper reports</response>
        [HttpGet("getJasperReports")]
        public async Task<ActionResult<IEnumerable<JasperReportModel>>> GetJasperReportsAsync()
        {
            //Note:  for now our report server is Jasper Server

            //var url = "https://reports.cosysdev.com/jasperserver/rest-v2/login?j_username=reportReader&j_password=4LMK1YtaE";

            //query the jasper server for "reportUnits"
            var url = _reportServerUrl + "/jasperserver/rest_v2/resources?type=reportUnit";

            var httpClient = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var reports = new List<JasperReportModel>();

            var svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_reportServerUsername + ":" + _reportServerPassword));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", svcCredentials);

            try
            {
                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    using var responseStream = await response.Content.ReadAsStreamAsync();
                    //this is xml not json
                    var serializer = new XmlSerializer(typeof(ResourceCollection));
                    var xmlSerialized = (ResourceCollection)serializer.Deserialize(responseStream)!;

                    //output needs to to be json not xml and simplify the object to just the fields we need
                    reports = xmlSerialized.Resources.Select(o => new JasperReportModel
                    {
                        Label = o.Label,
                        Uri = o.Uri
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error: {0}", ex.Message);
            }

            return Ok(reports);
        }
    }
}
