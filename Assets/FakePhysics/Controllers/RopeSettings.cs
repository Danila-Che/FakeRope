using FakePhysics.SoftBodyDynamics;
using UnityEngine;

namespace FakePhysics.Controllers
{
	[CreateAssetMenu(fileName = "RopeArgs", menuName = "SoftBodyDynamics/RopeArgs")]
	public class RopeSettings : ScriptableObject
	{
		[Min(0.01f)]
		[SerializeField] private float m_SpanDistance = 1f;
		[Min(0.0f)]
		[SerializeField] private float m_Drag = 0f;
		[Min(0.0f)]
		[SerializeField] private float m_Density = 7800f;
		[Min(0.0f)]
		[SerializeField] private float m_Radius = 0.1f;
		[Range(0f, 1f)]
		[SerializeField] private float m_Stiffness = 0f;

		public RopeArgs Args => new(m_SpanDistance, m_Drag, m_Density, m_Radius, m_Stiffness);
	}
}
