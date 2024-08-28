using System.Collections.Generic;
using System.Globalization;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace FakePhysics.Utilities
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

		public static FakePose ToPose(this Transform transform)
		{
			return new FakePose(transform.position, transform.rotation);
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

		public static void RemoveLastElement<T>(this List<T> list)
		{
			if (list.Count > 0)
			{
				list.RemoveAt(list.Count - 1);
			}
		}

		public static void RemoveLastElement<T>(this NativeList<T> list)
			where T : unmanaged
		{
			if (list.Length > 0)
			{
				list.RemoveAt(list.Length - 1);
			}
		}
	}
}
