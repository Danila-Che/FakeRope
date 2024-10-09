using FakePhysics.Utilities;
using Unity.Mathematics;
using UnityEngine;

namespace FakePhysics.Controllers
{
	public class SlingingAnchor : MonoBehaviour
	{
		public FakePose Pose => new(transform.localPosition);

#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(transform.position, 0.05f);
		}

#endif

		public float3 GetAnchorLocalPosition(FakeRigidBodyControllerBase targetRigidBody)
		{
			return transform.position - targetRigidBody.transform.position;
		}
	}
}
