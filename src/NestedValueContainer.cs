using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace FsDedunderator
{
    public abstract class NestedValueContainer<TValue> : INestedContainer
    {
        public const string Encoded_FullName_Separator = "%2E";
        public const string Decoded_FullName_Separator = ".";
        public static readonly StringComparer NameComparer = StringComparer.OrdinalIgnoreCase;
        private bool _disposedValue;
        private LinkedList<Site> _sites = new LinkedList<Site>();
        private ComponentCollection _components;

        public IComponent Owner { get; }

        protected virtual string OwnerName
        {
            get
            {
                ISite ownerSite;
                Monitor.Enter(_sites);
                try
                {
                    if (_disposedValue)
                        throw new ObjectDisposedException(GetType().FullName);
                    ownerSite = Owner.Site;
                }
                finally { Monitor.Exit(_sites); }
                if (null == ownerSite)
                    return null;
                INestedSite nestedOwnerSite = ownerSite as INestedSite;
                return (ownerSite is INestedSite) ? ((INestedSite)ownerSite).FullName : ownerSite.Name;
            }
        }

        public ComponentCollection Components
        {
            get
            {
                ComponentCollection components;
                Monitor.Enter(_sites);
                try
                {
                    if (_disposedValue)
                        throw new ObjectDisposedException(GetType().FullName);
                    if (null == (components = _components))
                        _components = components = new ComponentCollection(_sites.Select(s => s.Component).ToArray());
                }
                finally { Monitor.Exit(_sites); }
                return components;
            }
        }

        public virtual IComponent this[TValue value]
        {
            get
            {
                Site site = EnumerateSites().OfType<ValueSite>().FirstOrDefault(s => AreValuesEqual(s.Value, value));
                return (null == site) ? null : site.Component;
            }
        }
        
        protected NestedValueContainer(IComponent owner)
        {
            if (null == owner)
                throw new ArgumentNullException("owner");
            Owner = owner;
            Owner.Disposed += new EventHandler(OnOwnerDisposed);
        }

        protected virtual string ValidateName(IComponent component, string name)
        {
            if (null == component)
                throw new ArgumentNullException("component");
            if (null == name)
                return null;
            if (!EncodeName(DecodeName(name)).Equals(name))
                name = EncodeName(name);
            IComponent c = GetAnyComponentByName(name);
            if (null != c && !Object.ReferenceEquals(c, component))
                throw new ArgumentException("Duplicate name not allowed", "name");
            return name;
        }

        protected abstract string ValidateName(IComponent component, string name, out TValue value);

        protected abstract TValue ValidateValue(IComponent component, TValue value, out string name);

        protected abstract bool AreValuesEqual(TValue x, TValue y);

        protected abstract Site CreateSite(IComponent component, string name);

        protected abstract ValueSite CreateValueSite(IComponent component, TValue value);

        public virtual void Add(IComponent component)
        {
            Add(component, null);
        }

        public virtual void Add(IComponent component, string name)
        {
            if (null == component)
                throw new ArgumentNullException("component");
            Monitor.Enter(_sites);
            try
            {
                if (_disposedValue)
                    throw new ObjectDisposedException(GetType().FullName);
                Monitor.Enter(component);
                try
                {
                    if (CheckComponent(component))
                        throw new InvalidOperationException();
                    ISite oldSite = component.Site;
                    
                    if (null != oldSite && Object.ReferenceEquals(oldSite.Container, this))
                        return;
                    Site newSite = CreateSite(component, name);
                    if (null != oldSite)
                        oldSite.Container.Remove(component);
                    component.Site = newSite;
                    _components = null;
                    _sites.AddLast(newSite);
                }
                finally { Monitor.Exit(component); }
            }
            finally { Monitor.Exit(_sites); }
        }

        public virtual void AddValue(IComponent component, TValue value)
        {
            if (null == component)
                throw new ArgumentNullException("component");
            Monitor.Enter(_sites);
            try
            {
                if (_disposedValue)
                    throw new ObjectDisposedException(GetType().FullName);
                Monitor.Enter(component);
                try
                {
                    if (CheckComponent(component))
                        throw new InvalidOperationException();
                    ISite oldSite = component.Site;
                    if (null != oldSite && Object.ReferenceEquals(oldSite.Container, this))
                        return;
                    Site newSite = CreateValueSite(component, value);
                    if (null != oldSite)
                        oldSite.Container.Remove(component);
                    component.Site = newSite;
                    _components = null;
                    _sites.AddLast(newSite);
                }
                finally { Monitor.Exit(component); }
            }
            finally { Monitor.Exit(_sites); }
        }

        public void Remove(IComponent component)
        {
            if (null == component)
                return;
            Monitor.Enter(_sites);
            try
            {
                if (_disposedValue)
                    throw new ObjectDisposedException(GetType().FullName);
                Monitor.Enter(component);
                try
                {
                    Site site = EnumerateSites().FirstOrDefault(s => Object.ReferenceEquals(component, s.Component));
                    if (null != site)
                    {
                        _sites.Remove(site);
                        ISite s = component.Site;
                        if (null != s && Object.ReferenceEquals(site, s))
                            component.Site = null;
                    }
                }
                finally { Monitor.Exit(component); }
            }
            finally { Monitor.Exit(_sites); }
        }

        protected virtual object GetService(Type service)
        {
            if (_disposedValue)
                throw new ObjectDisposedException(GetType().FullName);
            if (typeof(INestedContainer).IsAssignableFrom(service))
                return this;
            return (typeof(IContainer).IsAssignableFrom(service)) ? this : null;
        }

        public int Count()
        {
            return EnumerateSites().Count();
        }
        
        public int CountOfType<TComponent>()
            where TComponent : class, IComponent
        {
            return EnumerateSites().OfType<TComponent>().Count();
        }

        public bool AnyOfType<TComponent>()
            where TComponent : class, IComponent
        {
            return EnumerateSites().OfType<TComponent>().Any();
        }

        protected TComponent FindComponent<TComponent>(Func<TComponent, bool> matcher)
            where TComponent : class, IComponent
        {
            return EnumerateSites().Select(s => s.Component).OfType<TComponent>().FirstOrDefault(matcher);
        }

        public virtual TComponent GetComponentByValue<TComponent>(TValue value)
            where TComponent : class, IComponent
        {
            return EnumerateSites().OfType<ValueSite>().Where(s => AreValuesEqual(s.Value, value))
                .Select(s => s.Component).OfType<TComponent>().FirstOrDefault();
        }
        
        public IComponent GetAnyComponentByName(string name)
        {
            if (null == name)
                return null;
            Site site = EnumerateSites().FirstOrDefault(s =>
            {
                string n = s.Name;
                return null != n && NameComparer.Equals(name, n);
            });
            return (null == site) ? null : site.Component;
        }
        
        public TComponent GetComponentByName<TComponent>(string name)
            where TComponent : class, IComponent
        {
            if (null == name)
                return null;
            return EnumerateSites().Where(s =>
            {
                string n = s.Name;
                return null != n && NameComparer.Equals(name, n);
            }).Select(s => s.Component).OfType<TComponent>().FirstOrDefault();
        }
        
        public IEnumerable<IComponent> EnumerateComponents()
        {
            return EnumerateSites().Select(s => s.Component);
        }

        private IEnumerable<Site> EnumerateSites()
        {
            Func<LinkedListNode<Site>, LinkedListNode<Site>> moveNext = (node) =>
            {
                LinkedListNode<Site> result;
                Monitor.Enter(_sites);
                try
                {
                    result = node.Next;
                    if (null == result)
                        return null;
                    Site n = result.Value;
                    ISite s = n.Component.Site;
                    while (null == s || !Object.ReferenceEquals(n, s))
                    {
                        LinkedListNode<Site> next = result.Next;
                        _components = null;
                        _sites.Remove(result);
                        if (null == next)
                            return null;
                        result = next;
                        n = result.Value;
                        s = n.Component.Site;
                    }
                }
                finally { Monitor.Exit(_sites); }
                return result;
            };
            LinkedListNode<Site> node = _sites.First;
            if (null == node)
                yield break;
            Site nestedSite = node.Value;
            ISite site = nestedSite.Component.Site;
            for (LinkedListNode<Site> n = (null == site || !Object.ReferenceEquals(nestedSite, site)) ? moveNext(node) : node; null != n; n = n.Next)
                yield return n.Value;
        }

        private bool CheckComponent(IComponent component)
        {
            if (null != component)
            {
                ISite site = component.Site;
                if (null != site && site is Site && Object.ReferenceEquals(site.Container, this))
                    return true;
                Site s = _sites.FirstOrDefault(s => Object.ReferenceEquals(s.Component, component));
                if (null != s)
                {
                    _sites.Remove(s);
                    _components = null;
                }
            }
            return false;
        }

        private void OnOwnerDisposed(object sender, EventArgs e)
        {
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Owner.Disposed -= new EventHandler(OnOwnerDisposed);
                    Monitor.Enter(_sites);
                    try
                    {
                        LinkedListNode<Site> node = _sites.First;
                        while (null != node)
                        {
                            LinkedListNode<Site> next = node.Next;
                            Site site = node.Value;
                            IComponent c = site.Component;
                            ISite s = c.Site;
                            _sites.Remove(node);
                            _components = null;
                            if (null != s && Object.ReferenceEquals(site, s) && Object.ReferenceEquals(this, s.Container))
                                c.Dispose();
                        }
                    }
                    finally { Monitor.Exit(_sites); }
                }
                _disposedValue = true;
            }
        }

        ~NestedValueContainer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public static string EncodeName(string name)
        {
            return (String.IsNullOrEmpty(name)) ? name : Uri.EscapeUriString(name).Replace(Decoded_FullName_Separator, Encoded_FullName_Separator);
        }
    
        public static string DecodeName(string name)
        {
            return (String.IsNullOrEmpty(name)) ? name : Uri.UnescapeDataString(name);
        }

        protected abstract class ValueSite : Site, IValueSite<TValue>
        {
            private TValue _value;

            private string _name;

            protected ValueSite(IComponent component, NestedValueContainer<TValue> container, TValue value) : base(component, container)
            {
                _value = Container.ValidateValue(Component, value, out string name);
                _name = name;
            }

            public TValue Value
            {
                get => _value;
                set
                {
                    Monitor.Enter(Container._sites);
                    try
                    {
                        Monitor.Enter(Component);
                        try
                        {
                            if (Container.CheckComponent(Component))
                            {
                                _value = Container.ValidateValue(Component, value, out string name);
                                _name = name;
                            }
                        }
                        finally { Monitor.Exit(Component); }
                    }
                    finally { Monitor.Exit(Container._sites); }
                }
            }

            public override string Name
            {
                get => _name;
                set
                {
                    Monitor.Enter(Container._sites);
                    try
                    {
                        Monitor.Enter(Component);
                        try
                        {
                            if (Container.CheckComponent(Component))
                            {
                                _name = Container.ValidateName(Component, value, out TValue newValue);
                                _value = newValue;
                            }
                        }
                        finally { Monitor.Exit(Component); }
                    }
                    finally { Monitor.Exit(Container._sites); }
                }
            }

            public override string RawName { get => _name; }
        }

        protected class OtherSite : Site
        {
            private string _name;
            private string _rawName;

            protected internal OtherSite(IComponent component, NestedValueContainer<TValue> container, string name) : base(component, container)
            {
                if (null == (_name = container.ValidateName(component, name)))
                    _rawName = null;
                else
                    _rawName = ((null == name) ? ((_name.Length == 0) ? null : "") : ((name.Equals(_name)) ? null : name));
            }

            public override string Name
            {
                get => _name;
                set
                {
                    Monitor.Enter(Container._sites);
                    try
                    {
                        Monitor.Enter(Component);
                        try
                        {
                            if (Container.CheckComponent(Component))
                            {
                                if (null == (_name = Container.ValidateName(Component, value)))
                                    _rawName = null;
                                else
                                    _rawName = ((null == value) ? ((_name.Length == 0) ? null : "") : ((value.Equals(_name)) ? null : value));

                            }
                        }
                        finally { Monitor.Exit(Component); }
                    }
                    finally { Monitor.Exit(Container._sites); }
                }
            }

            public override string RawName { get => (null == _rawName) ? _name : _rawName; }
        }

        protected abstract class Site : INestedSite
        {
            public abstract string Name { get; set; }

            public abstract string RawName { get; }

            public string FullName
            {
                get
                {
                    string childName = Name;
                    string ownerName;
                    return (null != childName && null != (ownerName = Container.OwnerName)) ?  ownerName + "." + childName : childName;
                }
            }

            public IComponent Component { get; }

            public NestedValueContainer<TValue> Container { get; }

            IContainer ISite.Container => Container;

            public bool DesignMode
            {
                get
                {
                    IComponent owner = Container.Owner;
                    ISite site;
                    return null != owner && null != (site = owner.Site) && site.DesignMode;
                }
            }

            protected Site(IComponent component, NestedValueContainer<TValue> container)
            {
                if (null == component)
                    throw new ArgumentNullException("component");
                if (null == container)
                    throw new ArgumentNullException("container");
                Component = component;
                Container = container;
            }

            public object GetService(Type serviceType)
            {
                return((serviceType == typeof(ISite)) ? this : Container.GetService(serviceType));
            }
        }
    }
}