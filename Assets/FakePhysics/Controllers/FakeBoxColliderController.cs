using FakePhysics.CollisionDetection;
using UnityEngine;

namespace FakePhysics.Controllers
{
	[SelectionBase]
	[RequireComponent(typeof(BoxCollider))]
	[DefaultExecutionOrder((int)ExecutionOrder.Collider)]
	public sealed class FakeBoxColliderController : MonoBehaviour
	{
		private FakeBoxCollider m_BoxCollider;

		public FakeBoxCollider BoxCollider => m_BoxCollider;

		private void OnEnable()
		{
			var boxCollider = GetComponent<BoxCollider>();

			m_BoxCollider = new FakeBoxCollider(boxCollider);
		}
	}
}
