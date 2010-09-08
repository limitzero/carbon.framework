using System;

namespace Carbon.Core.Stereotypes.For.Components.Message
{
    /// <summary>
    /// Attribute for a indicating the object should be used for messaging. In using this 
    /// attribute, the class should have one parameter-less constructor for the serialization
    /// instance to create for type inspection for concrete implementations.
    /// </summary>
    /// <example>
    /// [Message]
    /// public class OrderItem
    /// {
    ///     public string Id {get;set;}
    /// }
    /// 
    /// or 
    /// 
    /// [Message]
    /// public interface IOrderItem
    /// {
    ///    string Id {get; set;}
    /// }
    /// 
    /// the later can be created by the bus or the reflection engine by 
    /// 
    /// 1. bus.CreateComponent{IOrderItem}();
    /// 
    /// which delegates to
    /// 
    /// 2. IContainer.Resolve{IReflection}().BuildProxyFor{IOrderItem}();
    /// 
    /// </example>
    [AttributeUsage(AttributeTargets.Class ^ AttributeTargets.Enum ^ AttributeTargets.Struct ^ AttributeTargets.Interface,
        AllowMultiple = false, Inherited = true)]
    public class MessageAttribute : Attribute
    {
        
    }
}