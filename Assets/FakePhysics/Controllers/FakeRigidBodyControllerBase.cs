using FakePhysics.RigidBodyDynamics;
using UnityEngine;

namespace FakePhysics.Controllers
{
	public abstract class FakeRigidBodyControllerBase : MonoBehaviour
	{
		public abstract FakeRigidBody Body { get; }
	}
}
