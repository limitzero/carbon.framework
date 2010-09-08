using System;
using System.Collections.Generic;
using System.Threading;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Builder;
using Carbon.Core.Channel;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Registries.For.MessageEndpoints;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Core.Stereotypes.For.MessageHandling;
using Carbon.Integration.Configuration;
using Carbon.Integration.Stereotypes.Router;
using Carbon.Integration.Stereotypes.Router.Impl;
using Carbon.Integration.Stereotypes.Router.Impl.Configuration.Rules;
using Castle.Windsor;
using Xunit;

namespace Carbon.Integration.Tests.Stereotypes.Router
{
    public class RouterTests
    {
        private ProductSimpleRouter _router = null;

        public RouterTests()
        {
            _router = new ProductSimpleRouter();
        }

        [Fact]
        public void can_redirect_widget_product_to_the_widget_route_for_simple_router()
        {
            var route = _router.Route(new Widget());
            Assert.Equal("widget", route);
        }

        [Fact]
        public void can_redirect_gadget_product_to_the_gadget_route_for_simple_router()
        {
            var route = _router.Route(new Gadget());
            Assert.Equal("gadget", route);
        }

        [Fact]
        public void can_evaluate_routing_rule_for_widgets_and_return_widget_channel()
        {
            var rule = new WidgetRoutingRule();
            var envelope = new Envelope(new Widget()); // this is the message coming in over the infrastructure;

            Assert.Equal(true, rule.IsMatch(envelope));   // must do this first to set the channel for delivery
            Assert.Equal("widget", rule.ChannelName);

        }

        [Fact]
        public void can_evaluate_routing_rule_for_gadgets_and_return_gadget_channel()
        {
            var rule = new GadgetRoutingRule();
            var envelope = new Envelope(new Gadget()); // this is the message coming in over the infrastructure;

            Assert.Equal(true, rule.IsMatch(envelope));   // must do this first to set the channel for delivery
            Assert.Equal("gadget", rule.ChannelName);
        }

        [Fact]
        public void can_invoke_simple_router_over_infrastructure_to_route_a_message_to_the_appropriate_channel_for_a_widget()
        {
            // need to initialize the infrastructure first before the test can be conducted:
            var container = new WindsorContainer(@"empty.config.xml");
            container.Kernel.AddFacility(CarbonIntegrationFacility.FACILITY_ID, new CarbonIntegrationFacility());

            var context = container.Resolve<IIntegrationContext>();

            // add the channels to the infrastructure:
            context.GetComponent<IChannelRegistry>().RegisterChannel("product_simple_router"); // this may be  picked up on container initialization..
            context.GetComponent<IChannelRegistry>().RegisterChannel("widget");
            context.GetComponent<IChannelRegistry>().RegisterChannel("gadget");

            // register the component and create the message endpoint for it:
            context.GetComponent<IObjectBuilder>().Register(typeof(ProductSimpleRouter).Name, typeof(ProductSimpleRouter));
            context.GetComponent<IMessageEndpointRegistry>().CreateEndpoint("product_simple_router", string.Empty, string.Empty, new ProductSimpleRouter());

            // send the message over the channel to the endpoint via the message endpoint activator:
            context.GetComponent<IChannelRegistry>().FindChannel("product_simple_router").Send(new Envelope(new Widget()));

            // wait for the message to appear on the "widget" channel:
            var wait = new ManualResetEvent(true);
            wait.WaitOne(TimeSpan.FromSeconds(5));
            wait.Set();

            // get the message from the channel:
            var channel = context.GetComponent<IChannelRegistry>().FindChannel("widget");
            Assert.NotEqual(typeof(NullChannel), channel.GetType());

            var message = channel.Receive();
            Assert.NotEqual(typeof(NullEnvelope), message.GetType());

            Assert.Equal(typeof(Widget), message.Body.GetPayload<object>().GetType());
        }

        [Fact]
        public void can_invoke_simple_router_over_infrastructure_to_route_a_message_to_the_appropriate_output_channel_when_rule_does_not_return_a_channel()
        {
            // need to initialize the infrastructure first before the test can be conducted:
            var container = new WindsorContainer(@"empty.config.xml");
            container.Kernel.AddFacility(CarbonIntegrationFacility.FACILITY_ID, new CarbonIntegrationFacility());

            var context = container.Resolve<IIntegrationContext>();

            // add the channels to the infrastructure:
            context.GetComponent<IChannelRegistry>().RegisterChannel("product_simple_router"); 
            context.GetComponent<IChannelRegistry>().RegisterChannel("product_simple_router_error"); // here is where the message should go when routing rules fail
            context.GetComponent<IChannelRegistry>().RegisterChannel("widget");
            context.GetComponent<IChannelRegistry>().RegisterChannel("gadget");

            // register the component and create the message endpoint for it:
            context.GetComponent<IObjectBuilder>().Register(typeof(ProductSimpleRouter).Name, typeof(ProductSimpleRouter));
            context.GetComponent<IMessageEndpointRegistry>().CreateEndpoint("product_simple_router", string.Empty, string.Empty, new ProductSimpleRouter());

            // send the message over the channel to the endpoint via the message endpoint activator:
            context.GetComponent<IChannelRegistry>().FindChannel("product_simple_router").Send(new Envelope(new Thingy()));

            // wait for the message to appear on the "widget" channel:
            var wait = new ManualResetEvent(true);
            wait.WaitOne(TimeSpan.FromSeconds(5));
            wait.Set();

            // get the message from the channels (should be empty):
            var widgetChannel = context.GetComponent<IChannelRegistry>().FindChannel("widget");
            Assert.Equal(typeof(NullEnvelope), widgetChannel.Receive().GetType());

            var gadgetChannel = context.GetComponent<IChannelRegistry>().FindChannel("gadget");
            Assert.Equal(typeof(NullEnvelope), gadgetChannel.Receive().GetType());

            // here is where the message is going...no rule for routing met:
            var errorChannel = context.GetComponent<IChannelRegistry>().FindChannel("product_simple_router_error");
            Assert.Equal(typeof(Thingy), errorChannel.Receive().Body.GetPayload<object>().GetType());
        }

        [Fact]
        public void can_invoke_complex_router_over_infrastructure_to_route_a_message_to_the_appropriate_channel_for_a_widget()
        {
            // need to initialize the infrastructure first before the test can be conducted:
            var container = new WindsorContainer(@"empty.config.xml");
            container.Kernel.AddFacility(CarbonIntegrationFacility.FACILITY_ID, new CarbonIntegrationFacility());

            var context = container.Resolve<IIntegrationContext>();

            // add the channels to the infrastructure:
            context.GetComponent<IChannelRegistry>().RegisterChannel("product_complex_router"); // this may be  picked up on container initialization..
            context.GetComponent<IChannelRegistry>().RegisterChannel("widget");
            context.GetComponent<IChannelRegistry>().RegisterChannel("gadget");

            // register the component and create the message endpoint for it:
            context.GetComponent<IObjectBuilder>().Register(typeof(ProductComplexRouter).Name, typeof(ProductComplexRouter));
            context.GetComponent<IMessageEndpointRegistry>().CreateEndpoint("product_complex_router", string.Empty, string.Empty, new ProductComplexRouter());

            // send the message over the channel to the endpoint via the message endpoint activator:
            context.GetComponent<IChannelRegistry>().FindChannel("product_complex_router").Send(new Envelope(new Widget()));

            // wait for the message to appear on the "widget" channel:
            var wait = new ManualResetEvent(true);
            wait.WaitOne(TimeSpan.FromSeconds(5));
            wait.Set();

            // get the message from the channel:
            var channel = context.GetComponent<IChannelRegistry>().FindChannel("widget");
            Assert.NotEqual(typeof(NullChannel), channel.GetType());

            var message = channel.Receive();
            Assert.NotEqual(typeof(NullEnvelope), message.GetType());

            Assert.Equal(typeof(Widget), message.Body.GetPayload<object>().GetType());
        }

        [Fact]
        public void can_invoke_complex_router_over_infrastructure_to_route_a_message_to_the_appropriate_output_channel_when_rule_does_not_return_a_channel()
        {
            // need to initialize the infrastructure first before the test can be conducted:
            var container = new WindsorContainer(@"empty.config.xml");
            container.Kernel.AddFacility(CarbonIntegrationFacility.FACILITY_ID, new CarbonIntegrationFacility());

            var context = container.Resolve<IIntegrationContext>();

            // add the channels to the infrastructure:
            context.GetComponent<IChannelRegistry>().RegisterChannel("product_complex_router");
            context.GetComponent<IChannelRegistry>().RegisterChannel("product_complex_router_error"); // here is where the message should go when routing rules fail
            context.GetComponent<IChannelRegistry>().RegisterChannel("widget");
            context.GetComponent<IChannelRegistry>().RegisterChannel("gadget");

            // register the component and create the message endpoint for it:
            context.GetComponent<IObjectBuilder>().Register(typeof(ProductComplexRouter).Name, typeof(ProductComplexRouter));
            context.GetComponent<IMessageEndpointRegistry>().CreateEndpoint("product_complex_router", string.Empty, string.Empty, new ProductComplexRouter());

            // send the message over the channel to the endpoint via the message endpoint activator:
            context.GetComponent<IChannelRegistry>().FindChannel("product_complex_router").Send(new Envelope(new Thingy()));

            // wait for the message to appear on the channel:
            var wait = new ManualResetEvent(true);
            wait.WaitOne(TimeSpan.FromSeconds(5));
            wait.Set();

            // get the message from the channels (should be empty):
            var widgetChannel = context.GetComponent<IChannelRegistry>().FindChannel("widget");
            Assert.Equal(typeof(NullEnvelope), widgetChannel.Receive().GetType());

            var gadgetChannel = context.GetComponent<IChannelRegistry>().FindChannel("gadget");
            Assert.Equal(typeof(NullEnvelope), gadgetChannel.Receive().GetType());

            // here is where the message is going...no rule for routing met:
            var errorChannel = context.GetComponent<IChannelRegistry>().FindChannel("product_complex_router_error");
            Assert.Equal(typeof(Thingy), errorChannel.Receive().Body.GetPayload<object>().GetType());
        }

    }

    public interface IProduct
    {
        string Name { get; set; }
    }

    public class Widget : IProduct
    {
        public string Name { get; set; }
    }

    public class Gadget : IProduct
    {
        public string Name { get; set; }
    }

    public class Thingy : IProduct
    {
        public string Name { get; set; }
    }

    [MessageEndpoint("product_simple_router","product_simple_router_error")]
    public class ProductSimpleRouter
    {
        [Router]
        public string Route([MatchAll]IProduct product)
        {
            // the resultant string for the routing 
            // result should be mapped to a constructed
            // channel for receiving the message. if the routing rule
            // can not come up with a channel, the message 
            // will be sent to the output channel on the endpoint
            // (i.e. "product_simple_router_error")

            var route = string.Empty;

            if (product is Widget)
                route = "widget";

            if (product is Gadget)
                route = "gadget";

            return route;
        }
    }

    // let's add some routing rules for routing products:
    public class WidgetRoutingRule : IRoutingRule
    {
        public string ChannelName { get; private set; }
        public AbstractChannel Channel { get; private set; }

        public bool IsMatch(IEnvelope message)
        {
            ChannelName = "widget";
            var payload = message.Body.GetPayload<object>();
            var isMatch = (payload.GetType() == typeof(Widget));
            return isMatch;
        }
    }

    public class GadgetRoutingRule : IRoutingRule
    {
        public string ChannelName { get; private set; }
        public AbstractChannel Channel { get; private set; }

        public bool IsMatch(IEnvelope message)
        {
            ChannelName = "gadget";
            var payload = message.Body.GetPayload<object>();
            var isMatch = (payload.GetType() == typeof(Gadget));
            return isMatch;
        }
    }

    // let's create the content based router that will hold the rules for evaluation:
    public class ProductContentBasedRouter : AbstractRouterMessageHandlingStrategy
    {
        public ProductContentBasedRouter()
        {
            LoadRule<WidgetRoutingRule>();
            LoadRule<GadgetRoutingRule>();
        }
    }

    // need to create a router component that listens for messages on the "product" channel:
    [MessageEndpoint("product_complex_router","product_complex_router_error")]
    public class ProductComplexRouter
    {
        [Router(typeof(ProductContentBasedRouter))]
        public void RouteProduct([MatchAll] IProduct product)
        {
            // at this point the routing strategy should take 
            // over and deliver the message to the appropriate
            // channel via the infrastructure, if the routing rule
            // can not come up with a channel, the message 
            // will be sent to the output channel on the endpoint
            // (i.e. "product_complex_router_error")
        }
    }

}