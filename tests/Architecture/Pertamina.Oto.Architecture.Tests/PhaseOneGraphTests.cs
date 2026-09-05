using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Pertamina.Oto.Architecture.Tests;
[TestClass]
public sealed class PhaseOneGraphTests
{
    private static readonly Dictionary<string, string[]> Graph = new(StringComparer.Ordinal)
    {
        ["Pertamina.Oto.Contracts"] = [],
        ["Pertamina.Oto.Security"] = ["Pertamina.Oto.Contracts"],
        ["Pertamina.Oto.Observability"] = ["Pertamina.Oto.Contracts"],
        ["Pertamina.Oto.Storage.Abstractions"] = ["Pertamina.Oto.Contracts"],
        ["Pertamina.Oto.Processing.Abstractions"] = ["Pertamina.Oto.Contracts"],
        ["Pertamina.Oto.Modules.Pdf.Contracts"] = ["Pertamina.Oto.Contracts"],
        ["Pertamina.Oto.Modules.Pdf.Domain"] = ["Pertamina.Oto.Modules.Pdf.Contracts"],
        ["Pertamina.Oto.Modules.Pdf.Application"] = ["Pertamina.Oto.Modules.Pdf.Domain"],
    };
    private static bool Compliant(string xml, string[] expected)
    {
        var elements = XDocument.Parse(xml).Descendants().ToArray();
        if (elements.Any(e => e.Name.LocalName is "PackageReference" or "Reference")) return false;
        var actual = elements.Where(e => e.Name.LocalName == "ProjectReference")
            .Select(e => Path.GetFileNameWithoutExtension(((string?)e.Attribute("Include") ?? "").Replace('\\', '/'))).ToArray();
        return actual.Order(StringComparer.Ordinal).SequenceEqual(expected.Order(StringComparer.Ordinal));
    }
    [TestMethod]
    public void Local_foundation_has_no_network_or_server_calls()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "global.json"))) root = root.Parent;
        Assert.IsNotNull(root);
        foreach (var file in Directory.GetFiles(Path.Combine(root.FullName, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !p.Contains(Path.DirectorySeparatorChar + "Hosts" + Path.DirectorySeparatorChar, StringComparison.Ordinal)))
        {
            Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(File.ReadAllText(file),
                @"\b(System\.Net|HttpClient|HttpWebRequest|TcpClient|UdpClient|HubConnection|DllImport|LibraryImport)\b"), Path.GetFileName(file));
        }
    }    [TestMethod]
    public void Foundation_graph_matches_exact_checkpoint_edges()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "global.json"))) root = root.Parent;
        Assert.IsNotNull(root);
        var projects = Directory.GetFiles(Path.Combine(root.FullName, "src"), "*.csproj", SearchOption.AllDirectories);
        foreach (var pair in Graph)
        {
            var matches = projects.Where(p => Path.GetFileNameWithoutExtension(p) == pair.Key).ToArray();
            Assert.HasCount(1, matches);
            Assert.IsTrue(Compliant(File.ReadAllText(matches[0]), pair.Value), pair.Key);
        }
    }
    [TestMethod]
    public void Forbidden_reference_negative_canaries_fail_closed()
    {
        foreach (var target in new[] { "Desktop", "Storage.Abstractions", "Pdf.Infrastructure", "Server", "System.Net.Http" })
        foreach (var kind in new[] { "ProjectReference", "PackageReference", "Reference" })
            Assert.IsFalse(Compliant($"<Project><ItemGroup><{kind} Include='{target}.csproj'/></ItemGroup></Project>", []));
        Assert.IsFalse(Compliant("<Project/>", ["required"]));
    }
}

