using System;
using System.Collections;
using System.Collections.Generic;
using Carbon.Core;
using Carbon.Core.Configuration;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Integration.Dsl.Surface;
using Castle.Core.Configuration;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Builder;
using Carbon.Integration.Dsl.Surface.Registry;

namespace Carbon.Integration.Configuration.Surface
{
    public class SurfaceElementBuilder : AbstractElementBuilder
    {
        private const string m_element_name = "surface";

        public override bool IsMatchFor(string name)
        {
            return name.Trim().ToLower() == m_element_name.Trim().ToLower();
        }

        public override void Build(IConfiguration configuration)
        {
            // register the default surface for dyamically creating new surfaces from configuration:
            Kernel.AddComponent(typeof(DefaultSurface).Name, typeof(DefaultSurface));

            var reflection = Kernel.Resolve<IReflection>();
            var surfaceName = configuration.Attributes["name"];
            var available = configuration.Attributes["available"];

            var isAvailable = true; //optimistic do not skip this surface:
            if(bool.TryParse(available, out isAvailable))
                if(!isAvailable) return;

            var surfaceElements = new List<AbstractSubElementBuilder>(); 

            // inspect all of the children for the surface:
            for (var index = 0; index < configuration.Children.Count; index ++)
            {
                var element = configuration.Children[index];

                  var elementBuilderInstances =
                        reflection.FindConcreteTypesImplementingInterfaceAndBuild(typeof(AbstractSubElementBuilder),
                                                                          this.GetType().Assembly);

                  if (element == null)
                      continue;

                  foreach (var elementBuilderInstance in elementBuilderInstances)
                  {
                      var builder = elementBuilderInstance as AbstractSubElementBuilder;

                      if (!builder.IsMatchFor(element.Name)) continue;

                      builder.Kernel = Kernel;
                      builder.Build(element);

                      if(!surfaceElements.Contains(builder))
                          surfaceElements.Add(builder);
                      break;
                  }

            }
            
            // create and register the surface:
            var registry = Kernel.Resolve<ISurfaceRegistry>();

            var surface = this.CreateSurfanceImplementationFromElements(surfaceName, surfaceElements);

            if (surface != null)
            {
                surface.IsAvailable = isAvailable;
                surface.Configure();
                registry.Register(surface);
            }
        }

        private AbstractIntegrationComponentSurface CreateSurfanceImplementationFromElements(string name, 
            IEnumerable<AbstractSubElementBuilder> elements)
        {
 
            // create and register the surface:
            var surface = Kernel.Resolve<DefaultSurface>();
            var objectBuilder = Kernel.Resolve<IObjectBuilder>(); 

            surface.Name = name;

            foreach (var element in elements)
            {
                // receive ports:
                if (element is ReceivePort.ReceivePortElementBuilder)
                {
                    var elem = element as ReceivePort.ReceivePortElementBuilder;

                    elem.Port.ObjectBuilder = objectBuilder;
                    elem.Port.Build();

                    if (elem.Port.Port.Interval > 0)
                        surface.CreateReceivePort(elem.Port.Port.Pipeline,
                            elem.Port.Channel, elem.Port.Port.Uri, elem.Port.Port.Interval);
                    else
                    {
                        surface.CreateReceivePort(elem.Port.Port.Pipeline, elem.Port.Channel, elem.Port.Port.Uri,
                            elem.Port.Port.Concurrency, elem.Port.Port.Concurrency);
                    }
                }

                // send ports:
                if (element is SendPort.SendPortElementBuilder)
                {
                    var elem = element as SendPort.SendPortElementBuilder;

                    elem.Port.ObjectBuilder = objectBuilder;
                    elem.Port.Build();

                    if (elem.Port.Port.Interval > 0)
                        surface.CreateSendPort(elem.Port.Port.Pipeline,
                            elem.Port.Channel, elem.Port.Port.Uri, elem.Port.Port.Interval);
                    else
                    {
                        surface.CreateSendPort(elem.Port.Port.Pipeline, elem.Port.Channel, elem.Port.Port.Uri,
                            elem.Port.Port.Concurrency, elem.Port.Port.Concurrency);
                    }
                }

                // collaborations:
                 if (element is Collaborations.CollaborationsElementBuilder)
                 {
                     var elem = element as Collaborations.CollaborationsElementBuilder;

                     foreach (var definition in elem.Definitions)
                     {
                         object component = null;

                         try
                         {
                             component = Kernel.Resolve(definition.Reference, new Hashtable());
                         }
                         catch (Exception exception)
                         {
                             // component not found by reference
                             throw;
                         }

                         try
                         {
                             if (string.IsNullOrEmpty(definition.InputChannel))
                             {
                                 var channels = this.ExtractChannels(component.GetType());
                                 surface.AddComponent(component.GetType(), channels.Item1, channels.Item2);
                             }
                             else
                             {
                                 surface.AddComponent(component.GetType(), definition.InputChannel, definition.OutputChannel);
                             }
                         }
                         catch (Exception exception)
                         {
                             // incorrect configuration for channels on component:
                             throw;
                         }

                     }
                     
                 }

            }

            return surface;
        }

        private Tuple<string, string> ExtractChannels(Type component)
        {
            Tuple<string, string> retval = null;

            var attributes = component.GetCustomAttributes(typeof(MessageEndpointAttribute), true);
            if (attributes.Length == 0) return retval;

            var attr = attributes[0] as MessageEndpointAttribute;
            retval = new Tuple<string, string>(attr.InputChannel, attr.OutputChannel);

            return retval;
        }
    }
}