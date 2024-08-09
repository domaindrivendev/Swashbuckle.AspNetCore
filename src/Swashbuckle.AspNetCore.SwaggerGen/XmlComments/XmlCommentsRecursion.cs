using System.Xml.XPath;

namespace Swashbuckle.AspNetCore.SwaggerGen;

/// <summary>
///     Service to handle recursive retrieval of XML comments from an XPathNavigator.
/// </summary>
internal static class XmlCommentsRecursionService
{
    private const string MemberXPath = "/doc/members/member[@name='{0}']";

    /// <summary>
    ///     Finds the first node with the specified tag name recursively, starting from the given memberName in the XML
    ///     document.
    /// </summary>
    /// <param name="xmlNavigator">The XPathNavigator representing the XML document.</param>
    /// <param name="memberName">The name of the member to start the recursive search from.</param>
    /// <param name="tag">The tag name to find.</param>
    /// <returns>The XPathNavigator representing the found node or null if the node is not found.</returns>
    private static XPathNavigator FindNodeRecursive(XPathNavigator xmlNavigator, string memberName, string tag)
    {
        while (true)
        {
            // Find the node representing the current memberName in the XML document.
            var memberNode = xmlNavigator.SelectSingleNode(string.Format(MemberXPath, memberName));

            // Try to find the specified tag node within the current member node.
            var node = memberNode?.SelectSingleNode(tag);
            if (node != null)
                return memberNode;

            // If the specified tag node is not found, check if there is an "inheritdoc" tag.
            var inheritDocNode = memberNode?.SelectSingleNode("inheritdoc");
            if (inheritDocNode == null)
                return null;

            // If "inheritdoc" tag exists, get the "cref" attribute to find the referenced node in the XML document.
            var cref = inheritDocNode.GetAttribute("cref", string.Empty);
            if (string.IsNullOrEmpty(cref))
                return null;

            // Update the memberName to the referenced member and continue the recursive search.
            memberName = cref;
        }
    }

    /// <summary>
    ///     Selects multiple nodes with the specified tag name recursively, starting from the given memberName in the XML
    ///     document.
    /// </summary>
    /// <param name="xmlNavigator">The XPathNavigator representing the XML document.</param>
    /// <param name="memberName">The name of the member to start the recursive search from.</param>
    /// <param name="tag">The tag name to find.</param>
    /// <returns>
    ///     An XPathNodeIterator representing the collection of nodes with the specified tag, or null if the nodes are not
    ///     found.
    /// </returns>
    public static XPathNodeIterator SelectNodeRecursive(this XPathNavigator xmlNavigator, string memberName,
        string tag)
    {
        var node = FindNodeRecursive(xmlNavigator, memberName, tag);
        return node?.Select(tag);
    }

    /// <summary>
    ///     Selects the first node with the specified tag name recursively, starting from the given memberName in the XML
    ///     document.
    /// </summary>
    /// <param name="xmlNavigator">The XPathNavigator representing the XML document.</param>
    /// <param name="memberName">The name of the member to start the recursive search from.</param>
    /// <param name="tag">The tag name to find.</param>
    /// <returns>An XPathNavigator representing the found node or null if the node is not found.</returns>
    public static XPathNavigator SelectSingleNodeRecursive(this XPathNavigator xmlNavigator, string memberName,
        string tag)
    {
        var node = FindNodeRecursive(xmlNavigator, memberName, tag);
        return node?.SelectSingleNode(tag);
    }
}
