using Unity.Entities;

namespace FakePhysics.ECS.Dynamics
{
	public interface IFakeSubSolver
	{
		void Init(World world);
	}
}
