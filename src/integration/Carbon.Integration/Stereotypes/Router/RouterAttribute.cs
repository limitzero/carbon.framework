using System;
using System.Collections.Generic;
using System.Text;
using Carbon.Core.Stereotypes.For.MessageHandling;
using Carbon.Integration.Stereotypes.Router.Impl;
using Carbon.Integration.Stereotypes.Router.Impl.Configuration;

namespace Carbon.Integration.Stereotypes.Router
{
    /// <summary>
    /// Attribute for a indicating that a message will have rules applied for routing to the appropriate channel for processing.
    /// </summary>
    /// <example>
    ///  
    ///  public interface IProduct
    ///  {
    ///       Name {get; set:}
    ///   }
    /// 
    ///  public interface IWidget : IProduct
    ///  {}
    ///  
    ///  public interface IGadget : IProduct
    ///  {}
    ///  
    /// [MessagingEndpoint("product_router", "invalid_product")]
    /// public class ProductRouter
    /// {
    ///   
    ///     [Router(typeof(ProductContentBasedRouter)]    
    ///     public void RouteProductForDistribution(object product)
    ///     {
    ///         // the incoming message will be evaluated against 
    ///         // the rules, if a rule can not be satisfied, it
    ///         // will be sent to the configured output channel (invalid_product)
    ///         // for analysis: 
    ///     }
    /// 
    ///     or use simple routing with common inheritance chain...
    /// 
    ///     [Router]
    ///     public string ProductDistributionRouter([MatchAll] IProduct product)
    ///     {
    ///         // the [MatchAll] method attribute will look at the 
    ///        // incoming message and match all payloads that inherit from IProduct
    ///        // to the method for processing (effective for routing):
    /// 
    ///         var route = string.Empty;
    /// 
    ///         if(product.GetType == typeof(Widget))
    ///             route = "widget";
    ///         
    ///         if(product.GetType == typeof(Gadget))
    ///             route = "gadget";
    ///         
    ///         // if no route can be calculated, it will 
    ///         // forward the message to the "invalid_product" 
    ///         // channel for analysis: 
    ///    
    ///         return route;
    ///     }
    /// }
    ///  
    ///  // create the class to implement the content based router based on the rules:
    ///  public class ProductContentBasedRouter : AbstractRouterMessageHandlingStrategy
    ///  {
    ///     public ProductContentBasedRouter()
    ///     {
    ///           LoadRule{WidgetProductRule}();
    ///           LoadRule{GadgetRoutingRule}();
    /// 
    ///           // or
    /// 
    ///           base.LoadRules(new WidgetProductRule(), new GadgetProductRule());
    ///         
    ///           // or 
    ///           base.LoadRules(new ProductRoutingRule());
    ///     }  
    ///  }
    /// 
    ///  // create the rules:
    ///  public class WidgetProductRule : IRoutingRule
    ///  {
    ///     public string ChannelName { get; private set;}
    ///     public AbstractChannel Channel {get; private set; }
    ///    
    ///     public bool IsMatch(IEnvelope message)
    ///     {
    ///         var isMatch = false;
    /// 
    ///         if(message.Body.GetPayload{object}.GetType == typeof(Widget))
    ///         {
    ///             isMatch = true;
    ///             ChannelName = "widget";
    ///         }
    /// 
    ///         return isMatch;
    ///     }
    ///  }
    /// 
    ///  public class GadgetProductRule : IRoutingRule
    ///  {
    ///     public string ChannelName { get; private set;}
    ///     public AbstractChannel Channel {get; private set; }
    ///    
    ///     public bool IsMatch(IEnvelope message)
    ///     {
    ///         var isMatch = false;
    /// 
    ///         if(message.Body.GetPayload{object}.GetType == typeof(Gadget))
    ///         {
    ///             isMatch = true;
    ///             ChannelName = "gadget";
    ///         }
    ///         
    ///         return isMatch;
    ///     }
    ///  }
    /// 
    ///  or you can combine, if neccessary:
    /// 
    ///  public class ProductRoutingRule : IRoutingRule
    ///  {
    ///     public string ChannelName { get; private set;}
    ///     public AbstractChannel Channel {get; private set; }
    ///    
    ///     public bool IsMatch(IEnvelope message)  
    ///     {
    ///         var isMatch = false;
    /// 
    ///         if(message.Body.GetPayload{object}.GetType == typeof(Widget))
    ///         {
    ///             isMatch = true;
    ///             ChannelName = "widget";
    ///         }
    /// 
    ///         if(message.Body.GetPayload{object}.GetType == typeof(Gadget))
    ///         {
    ///             isMatch = true;
    ///             ChannelName = "gadget";
    ///         }
    ///         
    ///         return isMatch;
    ///     } 
    ///  
    ///  }   
    /// </example>
    [System.AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RouterAttribute : Attribute, IMessageHandlingStrategyAttribute
    {
        public Type Strategy { get; set; }

        /// <summary>
        /// Default implementation of a simple router where the return value is a string
        /// indicating the channel that the message will be sent to as a result of user-defined
        /// rules.
        /// </summary>
        public RouterAttribute()
            :this(new DefaultRouterMessageHandlingStrategy().GetType())
        {
        }

        /// <summary>
        /// Implementation of a complex router where multiple rules are defined in a custom 
        /// component using a strategy to implemention routing.
        /// </summary>
        /// <param name="strategy"></param>
        public RouterAttribute(Type strategy)
        {
            if (!typeof(AbstractRouterMessageHandlingStrategy).IsAssignableFrom(strategy))
                throw new Exception("The strategy used for content based routing must derive from " + typeof(AbstractRouterMessageHandlingStrategy).Name);

            Strategy = strategy;
        }

    }
}