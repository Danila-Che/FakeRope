using System;
using System.Linq;
using UnityEngine;

namespace FakePhysics.Controllers
{
	[DefaultExecutionOrder((int)ExecutionOrder.Joint)]
	public class FakeConnectorController : MonoBehaviour
	{
		[Serializable]
		private struct SourceAnchor
		{
			public FakeRigidBodyControllerBase RigidBody;
			public SlingingAnchor Anchor;
		}

		[SerializeField] private SourceAnchor[] m_SourceAnchors;
		[SerializeField] private FakeRigidBodyControllerBase m_Target;
		[SerializeField] private SlingingAnchor[] m_TargetAnchors;

		private readonly Connector m_Connector = new();

		[ContextMenu(nameof(Connect))]
		public void Connect()
		{
			var solver = GetComponentInParent<FakeSolverController>();

			var sourceRigidBodies = m_SourceAnchors.Select(anchor => anchor.RigidBody.Body).ToArray();
			var sourceAnchors = m_SourceAnchors.Select(anchor => anchor.Anchor.GetAnchorLocalPosition(anchor.RigidBody)).ToArray();
			var targetAnchors = m_TargetAnchors.Select(anchor => anchor.GetAnchorLocalPosition(m_Target)).ToArray();

			m_Connector.Connect(sourceRigidBodies, sourceAnchors, m_Target.Body, targetAnchors, solver);
		}

		[ContextMenu(nameof(Disconnect))]
		public void Disconnect()
		{
			var solver = GetComponentInParent<FakeSolverController>();

			m_Connector.Disconnect(solver);
		}
	}
}
