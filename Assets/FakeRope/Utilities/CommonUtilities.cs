using System.Globalization;
using Unity.Mathematics;
using UnityEngine;

namespace Fake.Utilities
{
	public static class CommonUtilities
	{
		public static string ToNumberFormat(string format, params object[] args)
		{
			return string.Format(CultureInfo.InvariantCulture.NumberFormat, format, args);
		}

		public static float3 WorldSize(this BoxCollider boxCollider)
		{
			return (float3)Vector3.Scale(boxCollider.size, boxCollider.transform.localScale);
		}

		public static FakePose ToPose(this Rigidbody rigidbody)
		{
			return new FakePose(rigidbody.position, rigidbody.rotation);
		}
	}
}
