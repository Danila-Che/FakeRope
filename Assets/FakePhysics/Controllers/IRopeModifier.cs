using FakePhysics.SoftBodyDynamics;

namespace FakePhysics.Controllers
{
	public interface IRopeModifier
	{
		void Modify(FakeRope rope, float deltaTime);
	}
}
