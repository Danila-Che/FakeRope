using System;
using System.Collections.Generic;
using UnityEngine;

namespace FakePhysics.Controllers
{
	public class RopeConnector : MonoBehaviour
	{
		private readonly Func<FakeRopeController> m_CreateRope;
		private readonly List<FakeRopeController> m_ConnectedRopes;

		public RopeConnector(Func<FakeRopeController> createRope)
		{
			m_CreateRope = createRope;
			m_ConnectedRopes = new();
		}

		public void Connect(
			FakeRigidBodyControllerBase sourceRigidBody,
			SlingingAnchor sourceAnchor,
			FakeRigidBodyControllerBase targetRigidBody,
			SlingingAnchor[] targetAnchors)
		{
			var ropes = ConnectWithAnchors(sourceRigidBody, sourceAnchor, targetRigidBody, targetAnchors);

			LevelRopes(ropes);
			m_ConnectedRopes.AddRange(ropes);
		}

		public void Disconnect()
		{
			m_ConnectedRopes.ForEach(rope =>
			{
				Destroy(rope.gameObject);
			});

			m_ConnectedRopes.Clear();
		}

		private List<FakeRopeController> ConnectWithAnchors(
			FakeRigidBodyControllerBase sourceRigidBody,
			SlingingAnchor sourceAnchor,
			FakeRigidBodyControllerBase targetRigidBody,
			SlingingAnchor[] targetAnchors)
		{
			var ropes = new List<FakeRopeController>(targetAnchors.Length);

			foreach (var anchor in targetAnchors)
			{
				var rope = CreateRope(targetRigidBody, anchor);
				ropes.Add(rope);
			}

			return ropes;
		}

		private FakeRopeController CreateRope(FakeRigidBodyControllerBase rigidBody, SlingingAnchor anchor)
		{
			var rope = m_CreateRope();
			//var acnchorLocalAttachment = m_InteractionHook.AttachPoint - m_InteractionHook.GetRididBody().transform.position;
			//var targetLocalAttachment = anchor.transform.position - cargo.transform.position;

			//targetLocalAttachment = cargo.transform.rotation * targetLocalAttachment;

			//rope.Create(
			//	m_InteractionHook.GetRididBody(),
			//	acnchorLocalAttachment,
			//	cargo.GetRigidBody(),
			//	targetLocalAttachment);

			return rope;
		}

		private void LevelRopes(List<FakeRopeController> ropes)
		{
			var averageLength = 0f;

			ropes.ForEach(rope => averageLength += rope.Length);

			averageLength /= ropes.Count;

			ropes.ForEach(rope => rope.SetLength(averageLength));
		}
	}
}
