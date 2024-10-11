using Unity.Entities;
using UnityEngine;

namespace FakePhysics.ECS.Controllers
{
	public abstract class FakeRigidBodyControllerBase : MonoBehaviour
	{
		public abstract Entity RigidBodyEntity { get; }
	}
}
