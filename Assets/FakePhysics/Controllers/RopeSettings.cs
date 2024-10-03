using FakePhysics.SoftBodyDynamics;
using UnityEngine;

namespace FakePhysics.Controllers
{
	[CreateAssetMenu(fileName = "RopeArgs", menuName = "SoftBodyDynamics/RopeArgs")]
	public class RopeSettings : ScriptableObject
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

		public RopeArgs Args => new RopeArgsBuilder()
			.SetSpanDistance(m_SpanDistance)
			.SetDrag(m_Drag)
			.SetDensity(m_Density)
			.SetRadius(m_Radius)
			.SetStiffness(m_Stiffness)
			.SetStretch(m_Stretch)
			.SetNeedDistanceConstraint(m_NeedDistanceConstraint)
			.SetNeedBendConstraint(m_NeedBendConstraint)
			.SetUseStretchFactor(m_UseStretchFactor);
	}
}
