using FakePhysics.ECS.SoftBodyDynamics;
using Unity.Mathematics;
using UnityEngine;

namespace FakePhysics.ECS.Controllers
{
	[CreateAssetMenu(fileName = "RopeArgs", menuName = "SoftBodyDynamics/RopeArgs")]
	public class RopeSettings : ScriptableObject, IRopeArgs
	{
		[Min(0.01f)]
		[SerializeField] private float m_SpanDistance = 1f;
		[Min(0f)]
		[SerializeField] private float m_Drag = 0f;
		[Min(0f)]
		[SerializeField] private float m_Density = 7800f;
		[Min(0f)]
		[SerializeField] private float m_Radius = 0.1f;
		[Range(0f, 1f)]
		[SerializeField] private float m_Stiffness = 0f;
		[Range(0f, 1f)]
		[SerializeField] private float m_Stretch = 0.5f;
		[SerializeField] private bool m_NeedDistanceConstraint = true;
		[SerializeField] private bool m_NeedBendConstraint = true;
		[SerializeField] private bool m_UseStretchFactor = false;

		public float SpanDistance => m_SpanDistance;

		public float Drag => m_Drag;

		public float Mass => m_Density * Volume;

		private float Volume => math.PI * m_Radius * m_Radius * m_SpanDistance;
	}
}
