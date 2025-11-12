/*
 * Copyright (c) Microsoft Corporation. All rights reserved.

 * MIT License
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 */

// Adapted from https://github.com/microsoft/OpenAPI.NET/blob/3b61b45991dded1aaecb16330430628d26a406de/src/Microsoft.OpenApi/OpenApiTagComparer.cs#L1

namespace Microsoft.OpenApi;

/// <summary>
/// This comparer is used to maintain a globally unique list of tags encountered
/// in a particular OpenAPI document.
/// </summary>
internal sealed class OpenApiTagComparer :
    IComparer<IOpenApiTag>,
    IComparer<OpenApiTagReference>,
    IEqualityComparer<IOpenApiTag>,
    IEqualityComparer<OpenApiTagReference>
{
    private static readonly Lazy<OpenApiTagComparer> _lazyInstance = new(() => new OpenApiTagComparer());

    // Tag comparisons are case-sensitive by default. Although the OpenAPI specification
    // only outlines case sensitivity for property names, we extend this principle to
    // property values for tag names as well.
    // See https://spec.openapis.org/oas/v3.1.0#format.
    private static readonly StringComparer StringComparer = StringComparer.Ordinal;

    /// <summary>
    /// Default instance for the comparer.
    /// </summary>
    internal static OpenApiTagComparer Instance => _lazyInstance.Value;

    public int Compare(IOpenApiTag x, IOpenApiTag y)
    {
        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is OpenApiTagReference referenceX && y is OpenApiTagReference referenceY)
        {
            return StringComparer.Compare(referenceX.Name ?? referenceX.Reference.Id, referenceY.Name ?? referenceY.Reference.Id);
        }

        return StringComparer.Compare(x.Name, y.Name);
    }

    public int Compare(OpenApiTagReference x, OpenApiTagReference y)
    {
        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        return StringComparer.Compare(x.Name ?? x.Reference.Id, y.Name ?? y.Reference.Id);
    }

    /// <inheritdoc/>
    public bool Equals(IOpenApiTag x, IOpenApiTag y) => Compare(x, y) == 0;

    /// <inheritdoc/>
    public bool Equals(OpenApiTagReference x, OpenApiTagReference y) => Compare(x, y) == 0;

    /// <inheritdoc/>
    public int GetHashCode(IOpenApiTag obj)
    {
        string value = obj?.Name;

        if (value is null && obj is OpenApiTagReference reference)
        {
            value = reference.Reference.Id;
        }

        return string.IsNullOrEmpty(value) ? 0 : StringComparer.GetHashCode(value);
    }

    /// <inheritdoc/>
    public int GetHashCode(OpenApiTagReference obj)
    {
        string value = obj?.Name ?? obj.Reference.Id;

        return string.IsNullOrEmpty(value) ? 0 : StringComparer.GetHashCode(value);
    }
}
