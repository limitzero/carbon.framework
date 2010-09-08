using System;
using System.Text;
using System.Xml;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Internals.Serialization;
using Xunit;
using Carbon.Test.Domain.Messages;

namespace Carbon.Core.Tests.Internals.Serialization
{
    public class when_the_serialization_provider_is_used
    {
        private ISerializationProvider m_serialization_provider = null;
        public when_the_serialization_provider_is_used()
        {
            m_serialization_provider = new XmlSerializationProvider(new DefaultReflection());
        }  
      
        [Fact]
        public void it_will_add_a_custom_type_to_the_internal_registry_of_types_for_serialization()
        {
            m_serialization_provider.AddCustomType(typeof(Invoice));
            Assert.Equal(1, m_serialization_provider.GetTypes().Length);
        }

        [Fact]
        public void it_will_serialize_a_message_from_type_instances_loaded_to_the_internal_registry_into_an_xml_format_after_initialization()
        {
            m_serialization_provider.AddCustomType(typeof(Invoice));
            m_serialization_provider.Initialize();

            var lm = new Invoice() { Id = "1" ,Total = 3.50M};
            var content = m_serialization_provider.Serialize(lm);
            Assert.NotNull(content);
            
            var doc = new XmlDocument();
            doc.LoadXml(content);
            Assert.True(doc.DocumentElement.ChildNodes.Count > 0);
        }

        [Fact(Skip="Not valid test")]
        public void it_will_deserialize_a_xml_format_of_a_message_into_the_concrete_instance_type()
        {
            m_serialization_provider.AddCustomType(typeof(Invoice));
            m_serialization_provider.Initialize();

            var payload = ASCIIEncoding.ASCII.GetBytes(this.GetConcreteInstanceAsXml());
            var instance = m_serialization_provider.Deserialize(payload);

            Assert.Equal(typeof(Invoice), instance.GetType());
        }

        private string GetConcreteInstanceAsXml()
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version='1.0'?>").Append(Environment.NewLine)
                .Append(
                "<anyType xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xsi:type='LoginRequestMessage'>")
                .Append(Environment.NewLine)
                .Append("<CorrelationId>d6624ea5-0327-48c5-a2d8-1d3a7bbfaec1</CorrelationId>").Append(
                Environment.NewLine)
                .Append("<Username>me</Username>").Append(Environment.NewLine)
                .Append("<Email>me</Email>").Append(Environment.NewLine)
                .Append("</anyType>");
            return sb.ToString();
        }

    }
}