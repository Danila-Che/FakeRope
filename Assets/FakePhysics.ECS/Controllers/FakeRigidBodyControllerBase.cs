using FakePhysics.ECS.Utilities;
using UnityEngine;

namespace FakePhysics.ECS.Controllers
{
	public abstract class FakeRigidBodyControllerBase : MonoBehaviour
	{
		public abstract FakeEntity RigidBodyEntity { get; }
	}
}
