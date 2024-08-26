using System.Collections.Generic;
using Unity.Mathematics;

namespace FakePhysics.EditModeTests.Utilities
{
	public static class Utilities
	{
	}

	internal class Float3Comparer : IEqualityComparer<float3>
	{
		private readonly float tolerance;

		public Float3Comparer(float tolerance = math.EPSILON)
		{
			this.tolerance = tolerance;
		}

		public bool Equals(float3 x, float3 y)
		{
			return math.distance(x, y) <= tolerance;
		}

		public int GetHashCode(float3 obj)
		{
			return obj.GetHashCode();
		}
	}

	internal class QuaternionComparer : IEqualityComparer<quaternion>
	{
		private readonly float tolerance;

		public QuaternionComparer(float tolerance = math.EPSILON)
		{
			this.tolerance = tolerance;
		}

		public bool Equals(quaternion x, quaternion y)
		{
			return math.angle(x, y) <= tolerance;
		}

		public int GetHashCode(quaternion obj)
		{
			return obj.GetHashCode();
		}
	}
}
