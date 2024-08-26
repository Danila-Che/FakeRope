using System.Collections.Generic;
using Unity.Mathematics;

namespace FakePhysics.CollisionDetection
{
	public readonly struct FakeContactPair
	{
		public readonly float3 Normal;
		public readonly float PenetrationDepth;
		public readonly List<float3> Points;

		public FakeContactPair(float3 normal, float penetrationDepth)
		{
			Normal = math.normalize(normal);
			PenetrationDepth = penetrationDepth;
			Points = new List<float3>();
		}
	}
}
