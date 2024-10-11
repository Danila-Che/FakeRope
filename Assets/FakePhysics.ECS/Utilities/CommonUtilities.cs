using System.Globalization;
using UnityEngine;

namespace FakePhysics.ECS.Utilities
{
	public static class CommonUtilities
	{
		public static string ToNumberFormat(string format, params object[] args)
		{
			return string.Format(CultureInfo.InvariantCulture.NumberFormat, format, args);
		}

		public static FakePose ToPose(this Rigidbody rigidbody)
		{
			return new FakePose(rigidbody.position, rigidbody.rotation);
		}

		public static FakePose ToPose(this Transform transform)
		{
			return new FakePose(transform.position, transform.rotation);
		}
	}
}
