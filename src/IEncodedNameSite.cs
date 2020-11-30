using System.ComponentModel;

namespace FsDedunderator
{
    public interface IEncodedNameSite : ISite
    {
         string RawName { get; }
    }
}