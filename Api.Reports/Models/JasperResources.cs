// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using System.Xml.Serialization;

namespace Econolite.Ode.Api.Reports.Models
{
    /// <summary>
    /// A collection of resources on the Jasper reports server. The REST response object is in xml and camel case.
    /// https://community.jaspersoft.com/documentation/tibco-jasperreports-server-rest-api-reference/v64/working-resource
    /// </summary>

    [XmlRoot("resources")]
    public class ResourceCollection
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlElement("resourceLookup")]
        public ResourceLookup[] Resources { get; set; } = Array.Empty<ResourceLookup>();
    }

    /// <summary>
    /// The individual resources on the Jasper reports server.  These properties are the resource descriptors common attributes. 
    /// https://community.jaspersoft.com/documentation/tibco-jasperreports-server-rest-api-reference/v64/resource-descriptors#top
    /// </summary>
    public class ResourceLookup
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlElement("creationDate")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("label")]
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("permissionMask")]
        public int PermissionMask { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("updateDate")]
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("uri")]
        public string Uri { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("version")]
        public int Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("resourceType")]
        public string ResourceType { get; set; } = string.Empty;
    }
}
