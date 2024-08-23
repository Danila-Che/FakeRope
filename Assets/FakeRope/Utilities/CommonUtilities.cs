using System.Globalization;

namespace FakeRope.Utilities
{
	public static class CommonUtilities
	{
		public static string ToNumberFormat(string format, params object[] args)
		{
			return string.Format(CultureInfo.InvariantCulture.NumberFormat, format, args);
		}
	}
}
