using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Carbon.Core.Registries
{
    /// <summary>
    /// Contract for all registries that can store items
    /// retrieve items and build collections of items 
    /// from an associated assembly.
    /// </summary>
    /// <typeparam name="TItem">Type of the item to store</typeparam>
    /// <typeparam name="TKey">Type of the key to retreive items from storage.</typeparam>
    public interface IRegistry<TItem, TKey>
    {
        /// <summary>
        /// This will return the list of all items in the associated storage.
        /// </summary>
        ReadOnlyCollection<TItem> GetAllItems();

        /// <summary>
        /// This will register an item in the associated storage.
        /// </summary>
        /// <param name="item">Item to store.</param>
        void Register(TItem item);

        /// <summary>
        /// This will remove an item in the associated storage.
        /// </summary>
        /// <param name="item"></param>
        void Remove(TItem item);

        /// <summary>
        /// This will retrieve the item in the associated storage by key.
        /// </summary>
        /// <param name="id">Key to retrieve the item.</param>
        TItem Find(TKey id);

        /// <summary>
        /// This will scan an assembly by name for all items that can be registered in the associated storage.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly to scan.</param>
        void Scan(params string[] assemblyName);

        /// <summary>
        /// This will scan an assembly by name for all items that can be registered in the associated storage.
        /// </summary>
        /// <param name="assembly">Assembly to scan.</param>
        void Scan(params Assembly[] assembly);
    }
}