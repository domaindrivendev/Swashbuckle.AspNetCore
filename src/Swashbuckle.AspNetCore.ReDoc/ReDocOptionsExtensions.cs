using System.Text;
using Swashbuckle.AspNetCore.ReDoc;

namespace Microsoft.AspNetCore.Builder
{
    public static class ReDocOptionsExtensions
    {
        /// <summary>
        /// Injects additional CSS stylesheets into the index.html page
        /// </summary>
        /// <param name="options"></param>
        /// <param name="path">A path to the stylesheet - i.e. the link "href" attribute</param>
        /// <param name="media">The target media - i.e. the link "media" attribute</param>
        public static void InjectStylesheet(this ReDocOptions options, string path, string media = "screen")
        {
            var builder = new StringBuilder(options.HeadContent);
            builder.AppendLine($"<link href='{path}' rel='stylesheet' media='{media}' type='text/css' />");
            options.HeadContent = builder.ToString();
        }
    }
}
