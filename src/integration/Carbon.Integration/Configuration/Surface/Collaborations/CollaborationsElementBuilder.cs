using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Carbon.Core.Configuration;
using Castle.Core.Configuration;

namespace Carbon.Integration.Configuration.Surface.Collaborations
{
    public class CollaborationsElementBuilder : AbstractSubElementBuilder
    {
        private const string m_element_name = "collaborations";
        private List<CollaborationDefinition> m_references = null;

        public ReadOnlyCollection<CollaborationDefinition> Definitions { get; private set; }

        public CollaborationsElementBuilder()
        {
            this.m_references = new List<CollaborationDefinition>();
        }

        public override bool IsMatchFor(string name)
        {
            return name.Trim().ToLower() == m_element_name.Trim().ToLower();
        }

        public override void Build(IConfiguration configuration)
        {
            for (var index = 0; index < configuration.Children.Count; index++)
            {
                var collaboration = configuration.Children[index];
                if (collaboration == null) continue;
                if (collaboration.Name.Trim().ToLower() != "add") continue;

                var reference = collaboration.Attributes["ref"];
                var inputChannel = collaboration.Attributes["input-channel"];
                var outputChannel = collaboration.Attributes["output-channel"];

                var definition = new CollaborationDefinition()
                                     {InputChannel = inputChannel, OutputChannel = outputChannel, Reference = reference};

                if(!m_references.Contains(definition))
                    m_references.Add(definition);
            }

            this.Definitions = m_references.AsReadOnly();
        }
    }
}