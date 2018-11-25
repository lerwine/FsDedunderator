using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator_obsolete
{
    public abstract class FsStructureChild : FsStructureElement, IFsStructureChild
    {
        #region Fields

        private IFsContainer _parent = null;

        #endregion

        #region Properties

        public string Name { get; private set; }

        #region Data

        IFsData IFsStructureChild.Data => BaseData;

        protected abstract IFsData BaseData { get; }

        #endregion

        #region Parent

        IFsContainer IFsStructureChild.Parent { get => BaseParent; set => BaseParent = value; }

        protected virtual IFsContainer BaseParent
        {
            get => _parent;
            set
            {
                Monitor.Enter(SyncRoot);
                IFsContainer oldParent = _parent;

                if ((oldParent == null) ? value == null : value != null && ReferenceEquals(oldParent, value))
                    return;

                try
                {
                    try
                    {
                        if (value == null)
                        {
                            if (oldParent == null)
                                return;
                            if (oldParent.Items.Contains(this))
                            {
                                oldParent.Items.Remove(this);
                                if (_parent == null)
                                    return;
                            }
                            _parent = null;
                        }
                        else
                        {
                            if (!ReferenceEquals(value.Root, Root))
                                throw new InvalidOperationException("Child belongs to another root");

                            if (oldParent == null)
                            {
                                if (!value.Items.Contains(this))
                                {
                                    value.Items.Add(this);
                                    if (_parent != null && ReferenceEquals(_parent, value))
                                        return;
                                }
                                _parent = value;
                            }
                            else
                            {
                                if (ReferenceEquals(oldParent, value))
                                    return;
                                if (value.Items.Contains(this))
                                {
                                    if (oldParent.Items.Contains(this))
                                        oldParent.Items.Remove(this);
                                    if (!value.Items.Contains(this))
                                        return;
                                    _parent = value;
                                }
                                else
                                {
                                    value.Items.Add(this);
                                    if (_parent != null && ReferenceEquals(_parent, value))
                                    {
                                        if (_parent != null && ReferenceEquals(_parent, value))
                                            return;
                                    }
                                    if (oldParent.Items.Contains(this))
                                        oldParent.Items.Remove(this);
                                    if (!value.Items.Contains(this))
                                        return;
                                    _parent = value;
                                }
                            }
                        }
                    }
                    catch
                    {
                        if (oldParent != null && oldParent.Items.Contains(this))
                        {
                            if (value.Items.Contains(this))
                                value.Items.Remove(this);
                            _parent = oldParent;
                        }
                        else if (value != null && value.Items.Contains(this) && _parent != null && !ReferenceEquals(_parent, value))
                        {
                            if (_parent.Items.Contains(this))
                                value.Items.Remove(this);
                            else
                                _parent = value;
                        }
                    }
                    finally
                    {
                        if (((oldParent == null) ? _parent != null : _parent == null || !ReferenceEquals(_parent, oldParent)) && ((value == null) ? _parent == null : _parent != null && ReferenceEquals(_parent, oldParent)))
                            RaiseParentChanged(oldParent);
                    }
                }
                finally { Monitor.Exit(SyncRoot); }
            }
        }

        #endregion

        #endregion

        #region Constructors

        protected FsStructureChild(string name, FsRootContainer root)
            : base(root)
        {
            Name = name ?? throw new ArgumentNullException("name");
        }

        #endregion

        #region Methods

        private void RaiseParentChanged(IFsContainer oldParent)
        {
            OnParentChanged(oldParent);
        }

        protected virtual void OnParentChanged(IFsContainer oldParent) { }

        #region Equals

        public bool Equals(IFsStructureChild other) => other != null && other is FsStructureChild && ReferenceEquals(this, other);

        public override bool Equals(object obj) => obj != null && obj is FsStructureChild && ReferenceEquals(obj, this);

        #endregion

        public override int GetHashCode() => Name.GetHashCode();

        public override string ToString() => Name;

        #endregion
    }
}