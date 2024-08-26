using System.Collections.Generic;
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

		public static float3 GetOrigin(this IEnumerable<float3> points)
		{
			var result = float3.zero;
			var count = 0;

			foreach (var point in points)
			{
				result += point;
				count++;
			}

			return result / count;
		}
	}
}
