using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CoreChatbotApp.Infrastructure.Clients.Timelog.ResponseObjects
{
    public class EmployeeResponseObjects
    {

        [XmlRoot("Employees", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
        public class EmployeesResponse
        {
            [XmlElement("Employee")]
            public List<Employee> Employees { get; set; } = new List<Employee>();
        }

        public class Employee
        {
            [XmlAttribute("ID")]
            public int ID { get; set; }

            [XmlElement("FirstName", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
            public string FirstName { get; set; }

            [XmlElement("LastName", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
            public string LastName { get; set; }

            [XmlElement("FullName", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
            public string FullName { get; set; }

            [XmlElement("Email", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
            public string Email { get; set; }
        }
    }
}
