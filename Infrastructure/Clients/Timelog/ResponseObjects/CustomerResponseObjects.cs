using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CoreChatbotApp.Infrastructure.Clients.Timelog.ResponseObjects
{
    public class CustomerResponseObjects
    {
        [XmlRoot("Customers", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
        public class CustomersResponse
        {
            [XmlElement("Customer")]
            public List<Customer> Customers { get; set; } = new List<Customer>();
        }

        public class Customer
        {
            [XmlAttribute("ID")]
            public int ID { get; set; }

            [XmlElement("Name", Namespace = "http://www.timelog.com/XML/Schema/tlp/v4_4")]
            public string Name { get; set; }
        }

    }
}
