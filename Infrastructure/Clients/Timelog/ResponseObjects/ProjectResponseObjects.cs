using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CoreChatbotApp.Infrastructure.Clients.Timelog.ResponseObjects
{
    public class ProjectResponseObjects
    {
        [XmlRoot("Projects", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
        public class ProjectsResponse
        {
            [XmlElement("Project")]
            public List<Project> Projects { get; set; } = new List<Project>();
        }

        public class Project
        {
            [XmlAttribute("GUID")]
            public string GUID { get; set; }

            [XmlElement("Name", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
            public string Name { get; set; }

            [XmlElement("No", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
            public string No { get; set; }

            [XmlElement("Status", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
            public int Status { get; set; }

            [XmlElement("Department", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
            public int Department { get; set; }

            [XmlElement("ProjectStartDate", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
            public DateTime StartDate { get; set; }

            [XmlElement("ProjectEndDate", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
            public DateTime EndDate { get; set; }

            [XmlElement("CustomerID", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
            public int CustomerID { get; set; }

            [XmlElement("CustomerName", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
            public string CustomerName { get; set; }

            [XmlElement("CustomerNo", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
            public string CustomerNo { get; set; }

            [XmlElement("PMID", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
            public int PMID { get; set; }

            [XmlElement("ProjectTypeName", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
            public string ProjectTypeName { get; set; }
        }
    }
}
