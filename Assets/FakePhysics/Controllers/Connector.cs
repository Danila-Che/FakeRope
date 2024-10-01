using FakePhysics.RigidBodyDynamics;
using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace FakePhysics.Controllers
{
	public class Connector
	{
		private readonly List<FakeAttachmentConstraint> m_Attachments = new();

		private FakeRigidBody[] m_SourceRigidBodies;

		public void Connect(FakeRigidBody[] sourceRigidBodies, float3[] sourceAnchors, FakeRigidBody targetRigidBody, float3[] targetAcnhors, FakeSolverController solver)
		{
			m_SourceRigidBodies = sourceRigidBodies;

			for (int i = 0; i < sourceAnchors.Length && i < targetAcnhors.Length; i++)
			{
				var joint = new FakeJoint(
					sourceRigidBodies[i],
					targetRigidBody,
					sourceAnchors[i],
					targetAcnhors[i]);
				var attachement = new FakeAttachmentConstraint(joint);


				solver.RegisterSolfBody(attachement);
				m_Attachments.Add(attachement);
			}

			Array.ForEach(m_SourceRigidBodies, rigidBody => rigidBody.IsTrigger = true);
		}

		public void Disconnect(FakeSolverController solver)
		{
			Array.ForEach(m_SourceRigidBodies, rigidBody => rigidBody.IsTrigger = false);

			m_Attachments.ForEach(attachment => solver.UnregisterSolfBody(attachment));
			m_Attachments.Clear();
		}
	}
}
