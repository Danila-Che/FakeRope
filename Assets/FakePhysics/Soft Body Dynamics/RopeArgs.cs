using System;
using Unity.Mathematics;
using UnityEngine;

namespace FakePhysics.SoftBodyDynamics
{
	[Serializable]
	public class RopeArgs
	{
		[Min(0.01f)]
		[SerializeField] private float m_SpanDistance = 1f;
		[Min(0.0f)]
		[SerializeField] private float m_Drag = 0f;
		[Min(0.0f)]
		[SerializeField] private float m_Density = 7800f;
		[Min(0.0f)]
		[SerializeField] private float m_Radius = 0.1f;

		public RopeArgs(float spanDistance = 1f, float drag = 0f, float density = 7800, float radius = 0.1f)
		{
			m_SpanDistance = spanDistance;
			m_Drag = drag;
			m_Density = density;
			m_Radius = radius;
		}

		public float SpanDistance => m_SpanDistance;

		public float Drag => m_Drag;

		public float Density => m_Density;

		public float Radius => m_Radius;

		public float Mass => m_Density * math.PI * m_Radius * m_Radius * m_SpanDistance;
	}
}
