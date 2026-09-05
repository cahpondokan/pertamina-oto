using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Pertamina.Oto.Architecture.Tests;

internal static class ContractsProjectDependencyRule
{
    private static readonly HashSet<string> OutboundReferenceElements =
        new(StringComparer.Ordinal)
        {
            "PackageReference",
            "ProjectReference",
            "Reference",
        };

    public static IReadOnlyList<string> FindViolations(string projectXml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectXml);

        var document = XDocument.Parse(projectXml, LoadOptions.None);

        return document
            .Descendants()
            .Where(element => OutboundReferenceElements.Contains(element.Name.LocalName))
            .Select(element =>
            {
                var include = (string?)element.Attribute("Include") ?? "<missing Include>";
                return $"{element.Name.LocalName}:{include}";
            })
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    public static void EnsureCompliant(string projectXml)
    {
        var violations = FindViolations(projectXml);
        if (violations.Count != 0)
        {
            throw new InvalidOperationException(
                $"Pertamina.Oto.Contracts must remain dependency-free. Forbidden outbound references: {string.Join(", ", violations)}");
        }
    }
}
