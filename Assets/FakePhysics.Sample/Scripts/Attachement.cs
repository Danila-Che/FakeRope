using UnityEngine;

namespace FakePhysics.Sample
{
	public class Attachement : MonoBehaviour
	{
		[SerializeField] private Rigidbody m_Anchor;
		[SerializeField] private Vector3 m_AnchorLocalPose;
		[SerializeField] private Rigidbody m_Target;
		[SerializeField] private Vector3 m_TargetLocalPose;

		private void FixedUpdate()
		{
			var anchorGlobalPose = m_Anchor.position + m_Anchor.rotation * m_AnchorLocalPose;
			var targetGlobalPose = m_Target.position + m_Target.rotation * m_TargetLocalPose;

			var correction = anchorGlobalPose - targetGlobalPose;

			Debug.DrawRay(targetGlobalPose, correction, Color.green);

			m_Target.AddForceAtPosition(correction, targetGlobalPose, ForceMode.Impulse);
		}
	}
}
