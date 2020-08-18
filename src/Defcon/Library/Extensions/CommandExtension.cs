using System.Linq;
using Defcon.Library.Attributes;
using DSharpPlus.CommandsNext;

namespace Defcon.Library.Extensions
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