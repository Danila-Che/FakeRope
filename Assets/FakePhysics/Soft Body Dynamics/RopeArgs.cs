using Unity.Mathematics;

namespace FakePhysics.SoftBodyDynamics
{
	public class RopeArgs
	{
		private readonly float m_SpanDistance = 1f;
		private readonly float m_Drag = 0f;
		private readonly float m_Density = 7800f;
		private readonly float m_Radius = 0.1f;
		private readonly float m_Stiffness = 0f;

		public RopeArgs(float spanDistance = 1f, float drag = 0f, float density = 7800, float radius = 0.1f, float stiffness = 0f)
		{
			m_SpanDistance = spanDistance;
			m_Drag = drag;
			m_Density = density;
			m_Radius = radius;
			m_Stiffness = stiffness;
		}

		public float SpanDistance => m_SpanDistance;

		public float Drag => m_Drag;

		public float Density => m_Density;

		public float Radius => m_Radius;

		public float Stiffness => m_Stiffness;

		public float Mass => m_Density * Volume;

		public float Volume => math.PI * m_Radius * m_Radius * m_SpanDistance;
	}
}
