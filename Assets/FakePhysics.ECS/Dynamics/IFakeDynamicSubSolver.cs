using Unity.Mathematics;

namespace FakePhysics.ECS.Dynamics
{
	public interface IFakeDynamicSubSolver
	{
		void BeginStep();

		void Step(float deltaTime);

		void EndStep(float deltaTime);

		void ApplyAcceleration(float deltaTime, float3 acceleration);

		void ApplyDrag(float deltaTime);
	}
}
