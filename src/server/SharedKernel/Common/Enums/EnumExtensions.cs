using System.ComponentModel;
using System.Reflection;

namespace Common.Enums;

public static class EnumExtensions
{
	public static string GetDescription(this Enum value)
	{
		var field = value.GetType().GetField(value.ToString());
		var attribute = (DescriptionAttribute)(field ?? throw new InvalidOperationException()).GetCustomAttribute(typeof(DescriptionAttribute))!;
		return attribute.Description;
	}
}