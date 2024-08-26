using Fake.Utilities;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Fake.CollisionDetection
{
	public readonly struct FakeBoxCollider
	{
		private const int k_BoxVerticesCount = 8;

		public readonly BoxCollider UnityBoxCollider;
		public readonly float3[] WorldVertices;

		private readonly float3 m_HalfSize;
		private readonly float3[] m_Vertices;

		public FakeBoxCollider(BoxCollider unityBoxCollider)
			: this(unityBoxCollider.WorldSize())
		{
			UnityBoxCollider = unityBoxCollider;
		}

		public FakeBoxCollider(float3 size)
		{
			UnityBoxCollider = null;
			m_HalfSize = 0.5f * size;

			m_Vertices = new float3[k_BoxVerticesCount];
			WorldVertices = new float3[k_BoxVerticesCount];

			m_Vertices[0] = new float3(-m_HalfSize.x, -m_HalfSize.y, -m_HalfSize.z);
			m_Vertices[1] = new float3(-m_HalfSize.x, -m_HalfSize.y, m_HalfSize.z);
			m_Vertices[2] = new float3(-m_HalfSize.x, m_HalfSize.y, -m_HalfSize.z);
			m_Vertices[3] = new float3(-m_HalfSize.x, m_HalfSize.y, m_HalfSize.z);
			m_Vertices[4] = new float3(m_HalfSize.x, -m_HalfSize.y, -m_HalfSize.z);
			m_Vertices[5] = new float3(m_HalfSize.x, -m_HalfSize.y, m_HalfSize.z);
			m_Vertices[6] = new float3(m_HalfSize.x, m_HalfSize.y, -m_HalfSize.z);
			m_Vertices[7] = new float3(m_HalfSize.x, m_HalfSize.y, m_HalfSize.z);
		}

		public float3 Size => 2f * m_HalfSize;

		public void Update(FakePose pose)
		{
			for (int i = 0; i < k_BoxVerticesCount; i++)
			{
				WorldVertices[i] = Computations.Transform(pose, m_Vertices[i]);
			}
		}
	}

	public static partial class CollisionComputations
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 FindFurthestPoint(FakeBoxCollider boxCollider, float3 direction)
		{
			var result = boxCollider.WorldVertices[0];
			var maxDistance = math.dot(result, direction);

			for (int i = 1; i < boxCollider.WorldVertices.Length; i++)
			{
				var distance = math.dot(boxCollider.WorldVertices[i], direction);

				if (distance > maxDistance)
				{
					maxDistance = distance;
					result = boxCollider.WorldVertices[i];
				}
			}

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 CalculateInverseInertiaTensor(FakeBoxCollider boxCollider, float mass)
		{
			var size = boxCollider.Size;

			var inertiaTensor = new float3()
			{
				x = (mass / 12.0f) * (size.y * size.y + size.z * size.z),
				y = (mass / 12.0f) * (size.x * size.x + size.z * size.z),
				z = (mass / 12.0f) * (size.x * size.x + size.y * size.y),
			};

			return new float3()
			{
				x = 1.0f / inertiaTensor.x,
				y = 1.0f / inertiaTensor.y,
				z = 1.0f / inertiaTensor.z,
			};
		}
	}
}
