using System.Linq;
using DSharpPlus.CommandsNext;
using Kaida.Library.Attributes;

namespace Kaida.Library.Extensions
{
    public static class CommandExtension
    {
        public static string Category(this Command command)
        {
            var categoryAttribute = (CategoryAttribute)command.CustomAttributes.FirstOrDefault(x => x is CategoryAttribute);

            return categoryAttribute != null
                ? categoryAttribute.Category 
                : command.Module.ModuleType.Namespace?.Split('.')
                         .Last();
        }
    }
}