using System.Text;
using DSharpPlus;

namespace Defcon.Library.Extensions
{
    public static class StringBuilderExtension
    {
        public static StringBuilder AppendLineBold(this StringBuilder stringBuilder, string title, object value)
        {
            return stringBuilder.AppendLine($"{title}: {Formatter.Bold($"{value}")}");
        }
    }
}
