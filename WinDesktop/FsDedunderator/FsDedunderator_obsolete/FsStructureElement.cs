using System;
using System.Collections.Generic;
using System.Text;

namespace FsDedunderator_obsolete
{
    public abstract class FsStructureElement : IFsStructureElement
    {
        #region Fields

        private readonly object _syncRoot = new object();

        #endregion

        #region Properties

        protected object SyncRoot => _syncRoot;

        public FsRootContainer Root { get; private set; }

        #endregion

        #region Constructors

        protected FsStructureElement(FsRootContainer root)
        {
            Root = root ?? throw new ArgumentNullException("root");
        }

        #endregion
    }
}
