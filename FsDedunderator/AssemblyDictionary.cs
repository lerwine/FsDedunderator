using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace FsDedunderator
{
    public class AssemblyEntry : IEquatable<AssemblyEntry>, IEquatable<Assembly>, IEquatable<AssemblyName>, IEquatable<Version>
    {
        private static ProcessorArchitecture? _processorArchitecture = null;
        private static Version _frameworkVersion;

        public static ProcessorArchitecture ProcessorArchitecture
        {
            get
            {
                ProcessorArchitecture? processorArchitecture = _processorArchitecture;
                if (!processorArchitecture.HasValue)
                {
                    switch (RuntimeInformation.ProcessArchitecture)
                    {
                        case Architecture.X64:
                            processorArchitecture = ProcessorArchitecture.Amd64;
                            break;
                        case Architecture.Arm64:
                        case Architecture.Arm:
                            processorArchitecture = ProcessorArchitecture.Arm;
                            break;
                        default:
                            processorArchitecture = ProcessorArchitecture.X86;
                            break;
                    }
                    _processorArchitecture = processorArchitecture;
                }
                return processorArchitecture.Value;
            }
        }
        
        public static Version FrameworkVersion
        {
            get
            {
                Version version = _frameworkVersion;
                if (version == null)
                {
                    string s = Regex.Split(RuntimeInformation.FrameworkDescription, "\\s+").TakeLast(1).FirstOrDefault();
                    if (String.IsNullOrEmpty(s) || !Version.TryParse(s, out version))
                        version = GetImageRuntimeVersion(RuntimeEnvironment.GetSystemVersion());
                    _frameworkVersion = version;
                }
                return version;
            }
        }

        private static readonly Regex RuntimeVersionParseRegex = new Regex(@"^v?(\d+(\.\d+)*$)", RegexOptions.Compiled);
        private static Version GetImageRuntimeVersion(string version)
        {
            if (version == null || (version = version.Trim()).Length == 0)
                return null;
            Match match = RuntimeVersionParseRegex.Match(version);
            Version result;
            return (match.Success && Version.TryParse(match.Groups[1].Value, out result)) ? result : null;
        }
        
        private AssemblyName _name = null;
        private string _publicKeyToken = null;
        private Version _imageRuntimeVersion = null;

        private AssemblyEntry(Assembly assembly) { Assembly = assembly; }

        public Assembly Assembly { get; private set; }

        public AssemblyName Name
        {
            get
            {
                AssemblyName name = _name;
                if (name == null)
                    name = _name = Assembly.GetName();
                return name;
            }
        }

        public Version ImageRuntimeVersion
        {
            get
            {
                Version version = _imageRuntimeVersion;
                if (version == null)
                    version = _imageRuntimeVersion = GetImageRuntimeVersion(Assembly.ImageRuntimeVersion);
                return version;
            }
        }

        public string PublicKeyToken
        {
            get
            {
                string publicKeyToken = _publicKeyToken;
                if (publicKeyToken == null)
                {
                    byte[] pk = Name.GetPublicKeyToken();
                    publicKeyToken = _publicKeyToken = (pk == null || pk.Length == 0) ? "" : String.Join("", pk.Select(b => ((int)b).ToString("x2")));
                }
                return publicKeyToken;
            }
        }

        public bool Equals(AssemblyEntry other) => other != null && ReferenceEquals(this, other);

        public bool Equals(Assembly other) => other != null && (ReferenceEquals(other, Assembly) || Assembly.FullName == other.FullName || Equals(other.GetName()));

        public bool Equals(AssemblyName other)
        {
            if (other == null)
                return false;
            AssemblyName current = Name;
            if (ReferenceEquals(other, current) || current.FullName == other.FullName)
                return true;
            if (current.Name != other.Name || current.ProcessorArchitecture != other.ProcessorArchitecture || current.CultureName != other.CultureName || !Equals(other.Version))
                return false;
            byte[] buffer = other.GetPublicKeyToken();
            if (buffer == null || buffer.Length == 0)
                return PublicKeyToken.Length == 0;
            return PublicKeyToken.Length > 0 && PublicKeyToken == String.Join("", buffer.Select(b => ((int)b).ToString("X2")));
        }

        public bool Equals(Version other)
        {
            if (other == null)
                return false;

            Version current = Name.Version;
            return (current.Major == other.Major && current.Minor == other.Minor && ((current.Revision < 1) ? other.Revision < 1 : current.Revision == other.Revision) && ((current.Build < 1) ? other.Build < 1 : current.Build == other.Build));
        }

        public override bool Equals(object obj) => obj != null && ((obj is AssemblyEntry) ? ReferenceEquals(obj, this) : ((obj is Assembly) ? Equals((Assembly)obj) : (obj is AssemblyName && Equals((AssemblyName)obj))));

        public override int GetHashCode() => Assembly.FullName.GetHashCode();

        public override string ToString() => Assembly.FullName;

        public static AssemblyEntry Create(Assembly assembly, bool force)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");
            AssemblyEntry entry = new AssemblyEntry(assembly);
            return (force || (entry.Name.ProcessorArchitecture == ProcessorArchitecture.MSIL || entry.Name.ProcessorArchitecture == AssemblyEntry.ProcessorArchitecture) && entry.ImageRuntimeVersion >= AssemblyEntry.FrameworkVersion) ? entry : null;
        }
    }

    public class AssemblyDictionary : IList<Assembly>, IDictionary<string, VersionDictionary>, IList, IDictionary
    {
        private readonly object _syncRoot;
        private readonly List<AssemblyEntry> _innerList = new List<AssemblyEntry>();
        private readonly Dictionary<string, VersionDictionary> _innerDictionary = new Dictionary<string, VersionDictionary>(StringComparer.InvariantCulture);

        public Assembly this[int index] => _innerList[index].Assembly;

        public VersionDictionary this[string key]
        {
            get
            {
                Monitor.Enter(_syncRoot);
                try { return (key != null && _innerDictionary.ContainsKey(key)) ? _innerDictionary[key] : null; }
                finally { Monitor.Exit(_syncRoot); }
            }
        }

        object IList.this[int index] { get => _innerList[index]; set => throw new NotSupportedException(); }

        object IDictionary.this[object key] { get => (key != null && key is string) ? this[(string)key] : null; set => throw new NotSupportedException(); }

        bool ICollection<Assembly>.IsReadOnly => true;

        bool ICollection<KeyValuePair<string, VersionDictionary>>.IsReadOnly => true;

        bool IList.IsReadOnly => true;

        bool IDictionary.IsReadOnly => true;

        bool IList.IsFixedSize => false;

        bool IDictionary.IsFixedSize => false;

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot => _syncRoot;
        
        public ICollection<string> Keys => _innerDictionary.Keys;

        public ICollection<VersionDictionary> Values => _innerDictionary.Values;

        ICollection IDictionary.Keys => _innerDictionary.Keys;

        ICollection IDictionary.Values => _innerDictionary.Values;

        VersionDictionary IDictionary<string, VersionDictionary>.this[string key] { get => this[key]; set => throw new NotImplementedException(); }

        Assembly IList<Assembly>.this[int index] { get => _innerList[index].Assembly; set => throw new NotImplementedException(); }

        public AssemblyDictionary()
        {
            _syncRoot = (_innerList is ICollection) ? (((ICollection)_innerList).SyncRoot ?? new object()) : new object();
        }

        void ICollection<Assembly>.Add(Assembly item) => throw new NotSupportedException();

        void IDictionary<string, VersionDictionary>.Add(string key, VersionDictionary value) => throw new NotSupportedException();

        void ICollection<KeyValuePair<string, VersionDictionary>>.Add(KeyValuePair<string, VersionDictionary> item) => throw new NotSupportedException();

        int IList.Add(object value) => throw new NotSupportedException();

        void IDictionary.Add(object key, object value) => throw new NotSupportedException();

        public int Count { get; private set; }

        int ICollection<KeyValuePair<string, VersionDictionary>>.Count => _innerDictionary.Count;
        
        public void Clear()
        {
            Monitor.Enter(_syncRoot);
            try { _innerList.Clear(); }
            finally { Monitor.Exit(_syncRoot); }
        }

        public bool Contains(Assembly item)
        {
            if (item == null)
                return false;
            Monitor.Enter(_syncRoot);
            try
            {
                foreach (AssemblyEntry assembly in _innerList)
                {
                    if (assembly.Equals(item))
                        return true;
                }
            }
            finally { Monitor.Exit(_syncRoot); }
            return false;
        }

        bool ICollection<KeyValuePair<string, VersionDictionary>>.Contains(KeyValuePair<string, VersionDictionary> item) => item.Key != null && item.Value != null && ((ICollection<KeyValuePair<string, VersionDictionary>>)_innerDictionary).Contains(item);

        bool IList.Contains(object value) => value != null && value is Assembly && Contains((Assembly)value);

        public bool ContainsKey(string key) => key != null && _innerDictionary.ContainsKey(key);

        bool IDictionary.Contains(object key) => key != null && key is string && _innerDictionary.ContainsKey((string)key);

        public void CopyTo(Assembly[] array, int arrayIndex) => _innerList.Select(a => a.Assembly).ToArray().CopyTo(array, arrayIndex);

        void ICollection<KeyValuePair<string, VersionDictionary>>.CopyTo(KeyValuePair<string, VersionDictionary>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, VersionDictionary>>)_innerDictionary).CopyTo(array, arrayIndex);

        void ICollection.CopyTo(Array array, int index) => _innerList.Select(a => a.Assembly).ToArray().CopyTo(array, index);

        public IEnumerator<Assembly> GetEnumerator() => _innerList.Select(a => a.Assembly).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _innerList.Select(a => a.Assembly).GetEnumerator();

        IEnumerator<KeyValuePair<string, VersionDictionary>> IEnumerable<KeyValuePair<string, VersionDictionary>>.GetEnumerator() => _innerDictionary.GetEnumerator();

        IDictionaryEnumerator IDictionary.GetEnumerator() => _innerDictionary.GetEnumerator();

        public int IndexOf(Assembly item)
        {
            if (item != null)
            {
                Monitor.Enter(_syncRoot);
                try
                {
                    for (int i = 0; i < _innerList.Count; i++)
                    {
                        if (_innerList[i].Equals(item))
                            return i;
                    }
                }
                finally { Monitor.Exit(_syncRoot); }
            }
            return -1;
        }

        public int IndexOf(string item)
        {
            if (item != null)
            {
                Monitor.Enter(_syncRoot);
                try
                {
                    if (_innerDictionary.ContainsKey(item))
                        return IndexOf(_innerDictionary[item].GetOptimalVersion());
                }
                finally { Monitor.Exit(_syncRoot); }
            }
            return -1;
        }

        int IList.IndexOf(object value) => (value != null && value is Assembly) ? IndexOf((Assembly)value) : -1;

        void IList<Assembly>.Insert(int index, Assembly item) => throw new NotSupportedException();

        void IList.Insert(int index, object value) => throw new NotSupportedException();

        bool ICollection<Assembly>.Remove(Assembly item) => throw new NotSupportedException();

        bool IDictionary<string, VersionDictionary>.Remove(string key) => throw new NotSupportedException();

        bool ICollection<KeyValuePair<string, VersionDictionary>>.Remove(KeyValuePair<string, VersionDictionary> item) => throw new NotSupportedException();

        void IList.Remove(object value) => throw new NotSupportedException();

        void IDictionary.Remove(object key) => throw new NotSupportedException();

        void IList<Assembly>.RemoveAt(int index) => throw new NotSupportedException();

        void IList.RemoveAt(int index) => throw new NotSupportedException();

        public bool TryGetValue(string key, out VersionDictionary value)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                if (key != null && _innerDictionary.ContainsKey(key))
                {
                    value = _innerDictionary[key];
                    return true;
                }
            }
            finally { Monitor.Exit(_syncRoot); }
            value = null;
            return false;
        }

        public void Import(IEnumerable<Assembly> collection, bool force)
        {
            if (collection == null)
                return;

            Monitor.Enter(_syncRoot);
            try
            {
                foreach (AssemblyEntry assembly in collection.Select(a => AssemblyEntry.Create(a, force)).Where(a => a != null))
                {
                    string name = assembly.Name.Name;
                    VersionDictionary vd;
                    if (_innerDictionary.ContainsKey(name))
                        _innerDictionary[name].Import(assembly, force);
                    else
                    {
                        vd = new VersionDictionary(name);
                        if (vd.Import(assembly, force))
                            _innerDictionary.Add(name, vd);
                    }
                }
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        public void Import(bool force, params Assembly[] collection) { Import(collection, force); }

        public void Import(IEnumerable<Assembly> collection) { Import(collection, false); }

        public void Import(Assembly assembly1, Assembly assembly2, params Assembly[] collection) { Import(((collection == null || collection.Length == 0) ? new Assembly[] { assembly1, assembly2 } : new Assembly[] { assembly1, assembly2 }).Concat(collection), false); }

        public bool Import(Assembly assembly, bool force)
        {
            AssemblyEntry an = AssemblyEntry.Create(assembly, force);
            if (an == null)
                return false;
            string name = an.Name.Name;
            Monitor.Enter(_syncRoot);
            try
            {
                VersionDictionary vd;
                if (_innerDictionary.ContainsKey(name))
                    return _innerDictionary[name].Import(an, force);
                vd = new VersionDictionary(name);
                if (vd.Import(an, force))
                {
                    _innerDictionary.Add(name, vd);
                    return true;
                }
            }
            finally { Monitor.Exit(_syncRoot); }
            return false;
        }
    }

    public class VersionDictionary : IList<Assembly>, IDictionary<Version, Assembly>, IList, IDictionary
    {
        private readonly object _syncRoot;
        private readonly string _name;
        private readonly List<AssemblyEntry> _innerList = new List<AssemblyEntry>();
        private KeyCollection<AssemblyEntry, Version, Assembly> _keys;
        private ValueCollection<AssemblyEntry, Assembly> _values;

        public VersionDictionary(string name)
        {
            _syncRoot = (_innerList is ICollection) ? (((ICollection)_innerList).SyncRoot ?? new object()) : new object();
            _name = name;
            _keys = new KeyCollection<AssemblyEntry, Version, Assembly>(_innerList, a => (a == null) ? null : a.Name.Version, a => (a == null) ? null : a.Assembly);
            _values = new ValueCollection<AssemblyEntry, Assembly>(_innerList, a => (a == null) ? null : a.Assembly);
        }

        public Assembly this[int index] { get => _innerList[index].Assembly; }

        Assembly IList<Assembly>.this[int index] { get => _innerList[index].Assembly; set => throw new NotSupportedException(); }

        public Assembly this[Version key]
        {
            get
            {
                if (key == null)
                    return null;
                Monitor.Enter(_syncRoot);
                try { return _innerList.Where(i => i.Equals(key)).Select(i => i.Assembly).FirstOrDefault(); }
                finally { Monitor.Exit(_syncRoot); }
            }
        }

        Assembly IDictionary<Version, Assembly>.this[Version key] { get => this[key]; set => throw new NotSupportedException(); }

        object IList.this[int index] { get => _innerList[index]; set => throw new NotSupportedException(); }

        object IDictionary.this[object key] { get => (key != null && key is Version) ? this[(Version)key] : null; set => throw new NotSupportedException(); }

        public int Count => _innerList.Count;

        bool ICollection<Assembly>.IsReadOnly => true;

        bool ICollection<KeyValuePair<Version, Assembly>>.IsReadOnly => true;

        bool IList.IsReadOnly => true;

        bool IDictionary.IsReadOnly => true;

        public ICollection<Version> Keys => _keys;

        ICollection IDictionary.Keys => _keys;

        public ICollection<Assembly> Values => _values;

        ICollection IDictionary.Values => _values;

        bool IList.IsFixedSize => false;

        bool IDictionary.IsFixedSize => false;

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot => _syncRoot;

        internal Assembly GetOptimalVersion()
        {

            IEnumerable<AssemblyEntry> ordered = _innerList.OrderByDescending(a => a.Name.Version);
            IEnumerable<AssemblyEntry> versionCompatible = ordered.Where(a => a.ImageRuntimeVersion <= AssemblyEntry.FrameworkVersion);
            IEnumerable<AssemblyEntry> architectureCompatible = versionCompatible.Where(a => a.Name.ProcessorArchitecture == ProcessorArchitecture.MSIL || a.Name.ProcessorArchitecture == AssemblyEntry.ProcessorArchitecture);
            AssemblyEntry result = (architectureCompatible.FirstOrDefault() ?? versionCompatible.FirstOrDefault()) ?? ordered.FirstOrDefault();
            return (result == null) ? null : result.Assembly;
        }

        void ICollection<Assembly>.Add(Assembly item) => throw new NotSupportedException();

        void IDictionary<Version, Assembly>.Add(Version key, Assembly value) => throw new NotSupportedException();

        void ICollection<KeyValuePair<Version, Assembly>>.Add(KeyValuePair<Version, Assembly> item) => throw new NotSupportedException();

        int IList.Add(object value) => throw new NotSupportedException();

        void IDictionary.Add(object key, object value) => throw new NotSupportedException();

        void ICollection<Assembly>.Clear() => throw new NotSupportedException();

        void ICollection<KeyValuePair<Version, Assembly>>.Clear() => throw new NotSupportedException();

        void IList.Clear() => throw new NotSupportedException();

        void IDictionary.Clear() => throw new NotSupportedException();

        public bool Contains(Assembly item)
        {
            if (item == null)
                return false;
            Monitor.Enter(_syncRoot);
            try{ return _innerList.Any(i => i.Equals(item)); }
            finally { Monitor.Exit(_syncRoot); }
        }

        bool ICollection<KeyValuePair<Version, Assembly>>.Contains(KeyValuePair<Version, Assembly> item) => item.Value != null && item.Value.Equals(item.Key) && ContainsKey(item.Key);

        bool IList.Contains(object value) => value != null && value is Assembly && Contains((Assembly)value);

        public bool ContainsKey(Version key)
        {
            if (key == null)
                return false;
            Monitor.Enter(_syncRoot);
            try { return _innerList.Any(i => i.Equals(key)); }
            finally { Monitor.Exit(_syncRoot); }
        }

        bool IDictionary.Contains(object key) => key != null && key is Version && ContainsKey((Version)key);

        public void CopyTo(Assembly[] array, int arrayIndex) => _innerList.Select(i => i.Assembly).ToArray().CopyTo(array, arrayIndex);

        void ICollection<KeyValuePair<Version, Assembly>>.CopyTo(KeyValuePair<Version, Assembly>[] array, int arrayIndex) => _innerList.Select(i =>new KeyValuePair<Version, Assembly>(i.Name.Version, i.Assembly)).ToArray().CopyTo(array, arrayIndex);

        void ICollection.CopyTo(Array array, int index) => _innerList.Select(i => i.Assembly).ToArray().CopyTo(array, index);

        public IEnumerator<Assembly> GetEnumerator() => _innerList.Select(i => i.Assembly).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<KeyValuePair<Version, Assembly>> IEnumerable<KeyValuePair<Version, Assembly>>.GetEnumerator() => new GDictionaryEnumerator<AssemblyEntry, Version, Assembly>(_innerList, a => (a == null) ? null : a.Name.Version, a => (a == null) ? null : a.Assembly);

        IDictionaryEnumerator IDictionary.GetEnumerator() => new GDictionaryEnumerator<AssemblyEntry, Version, Assembly>(_innerList, a => (a == null) ? null : a.Name.Version, a => (a == null) ? null : a.Assembly);

        public int IndexOf(Assembly item)
        {
            if (item != null)
            {
                Monitor.Enter(_syncRoot);
                try
                {
                    for (int i = 0; i < _innerList.Count; i++)
                    {
                        if (_innerList[i].Equals(item))
                            return i;
                    }
                }
                finally { Monitor.Exit(_syncRoot); }
            }
            return -1;
        }

        public int IndexOf(Version item)
        {
            if (item != null)
            {
                Monitor.Enter(_syncRoot);
                try
                {
                    for (int i = 0; i < _innerList.Count; i++)
                    {
                        if (_innerList[i].Equals(item))
                            return i;
                    }
                }
                finally { Monitor.Exit(_syncRoot); }
            }
            return -1;
        }

        int IList.IndexOf(object value) => (value != null && value is Assembly) ? IndexOf((Assembly)value) : -1;

        void IList<Assembly>.Insert(int index, Assembly item) => throw new NotSupportedException();

        void IList.Insert(int index, object value) => throw new NotSupportedException();

        bool ICollection<Assembly>.Remove(Assembly item) => throw new NotSupportedException();

        bool IDictionary<Version, Assembly>.Remove(Version key) => throw new NotSupportedException();

        bool ICollection<KeyValuePair<Version, Assembly>>.Remove(KeyValuePair<Version, Assembly> item) => throw new NotSupportedException();

        void IList.Remove(object value) => throw new NotSupportedException();

        void IDictionary.Remove(object key) => throw new NotSupportedException();

        void IList<Assembly>.RemoveAt(int index) => throw new NotSupportedException();

        void IList.RemoveAt(int index) => throw new NotSupportedException();

        public bool TryGetValue(Version key, out Assembly value)
        {
            throw new NotImplementedException();
        }

        internal bool Import(AssemblyEntry assembly, bool force)
        {
            if (assembly == null || assembly.Name.Name != _name)
                return false;

            Monitor.Enter(_syncRoot);
            try
            {
                Version version = assembly.Name.Version;
                AssemblyEntry current = _innerList.FirstOrDefault(a => a.Equals(version));
                if (current != null)
                {
                    if (current.ImageRuntimeVersion > AssemblyEntry.FrameworkVersion)
                    {
                        if (assembly.ImageRuntimeVersion < current.ImageRuntimeVersion)
                        {
                            _innerList[_innerList.IndexOf(current)] = assembly;
                            return true;
                        }
                    }
                    else if (assembly.ImageRuntimeVersion < current.ImageRuntimeVersion && assembly.ImageRuntimeVersion > current.ImageRuntimeVersion)
                    {
                        _innerList[_innerList.IndexOf(current)] = assembly;
                        return true;
                    }
                }
                else
                {
                    _innerList.Add(assembly);
                    return true;
                }
            }
            finally { Monitor.Exit(_syncRoot); }
            return false;
        }
    }
    public class NamespaceEntry : IEquatable<NamespaceEntry>, IEquatable<string>
    {
        public ReadOnlyCollection<Type> Types { get; private set; }
        public ReadOnlyDictionary<string, NamespaceDictionary> Namespaces { get; private set; }

        public bool Equals(NamespaceEntry other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(string other)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    public class NamespaceDictionary : IList<NamespaceEntry>, IDictionary<string, NamespaceEntry>, IList, IDictionary
    {
        private object _syncRoot;
        private List<NamespaceEntry> _innerList = new List<NamespaceEntry>();

        public NamespaceDictionary Parent { get; private set; }

        public string Name { get; private set; }

        public NamespaceEntry this[int index] { get => throw new NotImplementedException(); }

        NamespaceEntry IList<NamespaceEntry>.this[int index] { get => throw new NotImplementedException(); set => throw new NotSupportedException(); }

        public NamespaceEntry this[string key] { get => throw new NotImplementedException(); }

        NamespaceEntry IDictionary<string, NamespaceEntry>.this[string key] { get => throw new NotImplementedException(); set => throw new NotSupportedException(); }

        object IList.this[int index] { get => throw new NotImplementedException(); set => throw new NotSupportedException(); }

        object IDictionary.this[object key] { get => throw new NotImplementedException(); set => throw new NotSupportedException(); }

        public int Count => throw new NotImplementedException();
        
        bool ICollection<NamespaceEntry>.IsReadOnly => true;

        bool ICollection<KeyValuePair<string, NamespaceEntry>>.IsReadOnly => true;

        bool IList.IsReadOnly => true;

        bool IDictionary.IsReadOnly => true;

        ICollection<string> IDictionary<string, NamespaceEntry>.Keys => throw new NotImplementedException();

        ICollection IDictionary.Keys => throw new NotImplementedException();

        ICollection<NamespaceEntry> IDictionary<string, NamespaceEntry>.Values => throw new NotImplementedException();

        ICollection IDictionary.Values => throw new NotImplementedException();

        bool IList.IsFixedSize => false;

        bool IDictionary.IsFixedSize => false;

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot => _syncRoot;

        void ICollection<NamespaceEntry>.Add(NamespaceEntry item) => throw new NotSupportedException();

        void IDictionary<string, NamespaceEntry>.Add(string key, NamespaceEntry value) => throw new NotSupportedException();

        void ICollection<KeyValuePair<string, NamespaceEntry>>.Add(KeyValuePair<string, NamespaceEntry> item) => throw new NotSupportedException();

        int IList.Add(object value) => throw new NotSupportedException();

        void IDictionary.Add(object key, object value) => throw new NotSupportedException();

        void ICollection<NamespaceEntry>.Clear() => throw new NotSupportedException();

        void ICollection<KeyValuePair<string, NamespaceEntry>>.Clear() => throw new NotSupportedException();

        void IList.Clear() => throw new NotSupportedException();

        void IDictionary.Clear() => throw new NotSupportedException();

        bool ICollection<NamespaceEntry>.Contains(NamespaceEntry item)
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, NamespaceEntry>>.Contains(KeyValuePair<string, NamespaceEntry> item)
        {
            throw new NotImplementedException();
        }

        bool IList.Contains(object value)
        {
            throw new NotImplementedException();
        }

        bool IDictionary.Contains(object key)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, NamespaceEntry>.ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        void ICollection<NamespaceEntry>.CopyTo(NamespaceEntry[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, NamespaceEntry>>.CopyTo(KeyValuePair<string, NamespaceEntry>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator<NamespaceEntry> IEnumerable<NamespaceEntry>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<string, NamespaceEntry>> IEnumerable<KeyValuePair<string, NamespaceEntry>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        int IList<NamespaceEntry>.IndexOf(NamespaceEntry item)
        {
            throw new NotImplementedException();
        }

        int IList.IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        void IList<NamespaceEntry>.Insert(int index, NamespaceEntry item) => throw new NotSupportedException();

        void IList.Insert(int index, object value) => throw new NotSupportedException();

        bool ICollection<NamespaceEntry>.Remove(NamespaceEntry item) => throw new NotSupportedException();

        bool IDictionary<string, NamespaceEntry>.Remove(string key) => throw new NotSupportedException();

        bool ICollection<KeyValuePair<string, NamespaceEntry>>.Remove(KeyValuePair<string, NamespaceEntry> item) => throw new NotSupportedException();

        void IList.Remove(object value) => throw new NotSupportedException();

        void IDictionary.Remove(object key) => throw new NotSupportedException();

        void IList<NamespaceEntry>.RemoveAt(int index) => throw new NotSupportedException();

        void IList.RemoveAt(int index) => throw new NotSupportedException();

        bool IDictionary<string, NamespaceEntry>.TryGetValue(string key, out NamespaceEntry value)
        {
            throw new NotImplementedException();
        }
    }
    public class RootNamespaceDictionary : IList<NamespaceEntry>, IDictionary<string, NamespaceEntry>, IList, IDictionary
    {
        private object _syncRoot;
        private List<NamespaceEntry> _allNamespaces = new List<NamespaceEntry>();
        private Dictionary<string, NamespaceEntry> _rootNamespaces = new Dictionary<string, NamespaceEntry>();

        public NamespaceEntry this[int index] { get => throw new NotImplementedException();  }

        NamespaceEntry IList<NamespaceEntry>.this[int index] { get => throw new NotImplementedException(); set => throw new NotSupportedException(); }

        object IList.this[int index] { get => throw new NotImplementedException(); set => throw new NotSupportedException(); }

        public NamespaceEntry this[string key] { get => throw new NotImplementedException(); }

        NamespaceEntry IDictionary<string, NamespaceEntry>.this[string key] { get => throw new NotImplementedException(); set => throw new NotSupportedException(); }

        object IDictionary.this[object key] { get => throw new NotImplementedException(); set => throw new NotSupportedException(); }

        public int Count => _allNamespaces.Count;
        
        int ICollection<KeyValuePair<string, NamespaceEntry>>.Count => _rootNamespaces.Count;

        bool ICollection<NamespaceEntry>.IsReadOnly => true;

        bool ICollection<KeyValuePair<string, NamespaceEntry>>.IsReadOnly => true;

        bool IList.IsReadOnly => true;

        bool IDictionary.IsReadOnly => true;

        public ICollection<string> Keys => _rootNamespaces.Keys;

        ICollection IDictionary.Keys => _rootNamespaces.Keys;

        public ICollection<NamespaceEntry> Values => _rootNamespaces.Values;

        ICollection IDictionary.Values => _rootNamespaces.Values;

        bool IList.IsFixedSize => false;

        bool IDictionary.IsFixedSize => false;

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot => _syncRoot;

        public RootNamespaceDictionary(AssemblyDictionary assemblies)
        {
            foreach (Type type in assemblies.OfType<Assembly>().SelectMany(a => a.GetTypes()))
            {
                string fullNs = type.Namespace ?? "";
                int i = fullNs.IndexOf(".");
                string n, sn;
                if (i < 0)
                {
                    n = fullNs;
                    sn = null;
                }
                else
                {
                    n = fullNs.Substring(0, 1);
                    sn = fullNs.Substring(i + 1);
                }
                
                // TODO: upsert into _allNamespaces
                // TODO: upsert into _rootNamespaces;
            }
        }

        void ICollection<NamespaceEntry>.Add(NamespaceEntry item) => throw new NotSupportedException();

        void IDictionary<string, NamespaceEntry>.Add(string key, NamespaceEntry value) => throw new NotSupportedException();

        void ICollection<KeyValuePair<string, NamespaceEntry>>.Add(KeyValuePair<string, NamespaceEntry> item) => throw new NotSupportedException();

        int IList.Add(object value) => throw new NotSupportedException();

        void IDictionary.Add(object key, object value) => throw new NotSupportedException();
        
        public void Clear()
        {
            Monitor.Enter(_syncRoot);
            try
            {
                _allNamespaces.Clear();
                _rootNamespaces.Clear();
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        public bool Contains(NamespaceEntry item)
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, NamespaceEntry>>.Contains(KeyValuePair<string, NamespaceEntry> item)
        {
            throw new NotImplementedException();
        }

        bool IList.Contains(object value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        bool IDictionary.Contains(object key)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(NamespaceEntry[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, NamespaceEntry>>.CopyTo(KeyValuePair<string, NamespaceEntry>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<NamespaceEntry> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<string, NamespaceEntry>> IEnumerable<KeyValuePair<string, NamespaceEntry>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(NamespaceEntry item)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(string item)
        {
            throw new NotImplementedException();
        }

        int IList.IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        void IList<NamespaceEntry>.Insert(int index, NamespaceEntry item) => throw new NotSupportedException();

        void IList.Insert(int index, object value) => throw new NotSupportedException();

        bool ICollection<NamespaceEntry>.Remove(NamespaceEntry item) => throw new NotSupportedException();

        bool IDictionary<string, NamespaceEntry>.Remove(string key) => throw new NotSupportedException();

        bool ICollection<KeyValuePair<string, NamespaceEntry>>.Remove(KeyValuePair<string, NamespaceEntry> item) => throw new NotSupportedException();

        void IList.Remove(object value) => throw new NotSupportedException();

        void IDictionary.Remove(object key) => throw new NotSupportedException();

        void IList<NamespaceEntry>.RemoveAt(int index) => throw new NotSupportedException();

        void IList.RemoveAt(int index) => throw new NotSupportedException();

        public bool TryGetValue(string key, out NamespaceEntry value)
        {
            throw new NotImplementedException();
        }
    }

    internal class GDictionaryEnumerator<TSource, TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
    {
        private IEnumerator<TSource> _innerEnumerator;
        private Func<TSource, TKey> _getKey;
        private Func<TSource, TValue> _getValue;
        
        internal GDictionaryEnumerator(IEnumerable<TSource> collection, Func<TSource, TKey> getKey, Func<TSource, TValue> getValue)
        {
            if (getKey == null)
                throw new ArgumentNullException("getKey");

            if (getValue == null)
                throw new ArgumentNullException("getValue");

            _innerEnumerator = (collection ?? new TSource[0]).GetEnumerator();
            _getKey = getKey;
            _getValue = getValue;
        }

        public KeyValuePair<TKey, TValue> Current
        {
            get
            {
                TSource current = _innerEnumerator.Current;
                if ((object)current == null)
                    return new KeyValuePair<TKey, TValue>();
                return new KeyValuePair<TKey, TValue>(_getKey(current), _getValue(current));
            }
        }

        public DictionaryEntry Entry
        {
            get
            {
                TSource current = _innerEnumerator.Current;
                if ((object)current == null)
                    return new DictionaryEntry();
                return new DictionaryEntry(_getKey(current), _getValue(current));
            }
        }

        public TKey Key
        {
            get
            {
                TSource current = _innerEnumerator.Current;
                return ((object)current == null) ? default(TKey) : _getKey(current);
            }
        }

        object IDictionaryEnumerator.Key => Key;

        public TValue Value
        {
            get
            {
                TSource current = _innerEnumerator.Current;
                return ((object)current == null) ? default(TValue) : _getValue(current);
            }
        }

        object IDictionaryEnumerator.Value => Value;

        object IEnumerator.Current => Current;

        public void Dispose() => _innerEnumerator.Dispose();

        public bool MoveNext() => _innerEnumerator.MoveNext();

        public void Reset() => _innerEnumerator.Reset();
    }

    internal class KeyCollection<TSource, TKey, TValue> : ICollection<TKey>, ICollection
    {
        private ICollection<TSource> _innerCollection;
        private Func<TSource, TKey> _getKey;
        private Func<TSource, TValue> _getValue;

        internal KeyCollection(ICollection<TSource> collection, Func<TSource, TKey> getKey, Func<TSource, TValue> getValue)
        {
            if (getKey == null)
                throw new ArgumentNullException("getKey");

            if (getValue == null)
                throw new ArgumentNullException("getValue");

            _innerCollection = collection ?? new TSource[0];
            _getKey = getKey;
            _getValue = getValue;
        }

        public int Count => _innerCollection.Count;

        bool ICollection<TKey>.IsReadOnly => true;

        bool ICollection.IsSynchronized => _innerCollection is ICollection && ((ICollection)_innerCollection).IsSynchronized;

        object ICollection.SyncRoot => (_innerCollection is ICollection) ? ((ICollection)_innerCollection).SyncRoot : null;

        void ICollection<TKey>.Add(TKey item) => throw new NotSupportedException();

        void ICollection<TKey>.Clear() => throw new NotSupportedException();

        public bool Contains(TKey item) => _innerCollection.Select(i => _getKey(i)).Contains(item);

        void ICollection<TKey>.CopyTo(TKey[] array, int arrayIndex) => _innerCollection.Select(i => _getKey(i)).ToArray().CopyTo(array, arrayIndex);

        void ICollection.CopyTo(Array array, int index) => _innerCollection.Select(i => _getKey(i)).ToArray().CopyTo(array, index);

        IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => _innerCollection.Select(i => _getKey(i)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _innerCollection.Select(i => _getKey(i)).GetEnumerator();

        bool ICollection<TKey>.Remove(TKey item) => throw new NotSupportedException();
    }

    internal class ValueCollection<TSource, TValue> : ICollection<TValue>, ICollection
    {
        private Func<TSource, TValue> _getValue;
        private ICollection<TSource> _innerCollection;

        internal ValueCollection(ICollection<TSource> collection, Func<TSource, TValue> getValue)
        {
            if (getValue == null)
                throw new ArgumentNullException("getValue");

            _innerCollection = collection ?? new TSource[0];
        }

        public int Count => _innerCollection.Count;
        
        bool ICollection<TValue>.IsReadOnly => true;

        bool ICollection.IsSynchronized => _innerCollection is ICollection && ((ICollection)_innerCollection).IsSynchronized;

        object ICollection.SyncRoot => (_innerCollection is ICollection) ? ((ICollection)_innerCollection).SyncRoot : null;

        void ICollection<TValue>.Add(TValue item) => throw new NotSupportedException();

        void ICollection<TValue>.Clear() => throw new NotSupportedException();

        public bool Contains(TValue item) => _innerCollection.Select(i => _getValue(i)).Contains(item);

        public void CopyTo(TValue[] array, int arrayIndex) => _innerCollection.Select(i => _getValue(i)).ToArray().CopyTo(array, arrayIndex);

        void ICollection.CopyTo(Array array, int index) => _innerCollection.Select(i => _getValue(i)).ToArray().CopyTo(array, index);

        public IEnumerator<TValue> GetEnumerator() => _innerCollection.Select(i => _getValue(i)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _innerCollection.Select(i => _getValue(i)).GetEnumerator();

        bool ICollection<TValue>.Remove(TValue item) => throw new NotSupportedException();
    }

}