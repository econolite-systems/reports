// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Api.Reports.Models
{
    /// <summary>
    /// A Jasper server resource who's type is "reportUnit".
    ///  https://community.jaspersoft.com/documentation/tibco-jasperreports-server-rest-api-reference/v64/working-resource
    /// </summary>
    public class JasperReportModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string Label { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string Uri { get; set; } = string.Empty;
    }
}
