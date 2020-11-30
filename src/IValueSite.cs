using System.ComponentModel;

namespace FsDedunderator
{
    public interface IValueSite<TValue> : ISite
    {
         TValue Value { get; set; }
    }
}