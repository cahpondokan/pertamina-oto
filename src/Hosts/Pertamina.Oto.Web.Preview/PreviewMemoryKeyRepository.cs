using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace Pertamina.Oto.Web.Preview;

// Local UI preview only: no persistent sessions, identity or production key storage.
internal sealed class PreviewMemoryKeyRepository : IXmlRepository
{
    private readonly List<XElement> elements = [];
    private readonly object gate = new();

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        lock (gate) return elements.Select(element => new XElement(element)).ToArray();
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        ArgumentNullException.ThrowIfNull(element);
        lock (gate) elements.Add(new XElement(element));
    }
}
