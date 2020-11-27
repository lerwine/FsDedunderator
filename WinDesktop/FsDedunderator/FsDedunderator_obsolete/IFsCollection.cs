using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator_obsolete
{
    public interface IFsCollection : IDictionary<string, IFsData>, IList<IFsStructureChild>, IList<IFsData>, IDictionary, IList
    {
        IFsContainer Owner { get; }
        IFsStructureChild GetItemByName(string name);

        /// <summary>
        /// Get existing item or add/replace new item.
        /// </summary>
        /// <param name="name">Name of item</param>
        /// <param name="createNew">Callback for creating new child item.</param>
        /// <param name="item">Item matching name.</param>
        /// <returns>Item matching the given name or the item upserted.</returns>
        bool Upsert(string name, Func<IFsStructureChild> createNew, out IFsStructureChild item);
    }
}