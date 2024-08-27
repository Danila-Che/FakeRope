using FakePhysics.SoftBodyDynamics;

namespace FakePhysics.Controllers
{
	internal interface IRopeModifier
	{
		void Modify(FakeRope rope, float deltaTime);
	}
}
