using FakePhysics.ECS.Dynamics;
using FakePhysics.ECS.RigidBodyDynamics.Systems;
using FakePhysics.ECS.Utilities;
using Unity.Mathematics;

namespace FakePhysics.ECS.RigidBodyDynamics
{
	public sealed class FakeRigidBodySubSolver : IFakeSubSolver, IFakeDynamicSubSolver, IFakeConstrainedSubSolver
	{
		private FakeWorld m_World;

		private BeginStepSystem m_BeginStepSystem;
		private EndStepSystem m_EndStepSystem;
		private AttachmentConstraintSystem m_AttachmentConstraintSystem;

		public void Init(FakeWorld world)
		{
			m_World = world;

			m_BeginStepSystem = m_World.CreateSystem<BeginStepSystem>();
			m_EndStepSystem = m_World.CreateSystem<EndStepSystem>();
			m_AttachmentConstraintSystem = m_World.CreateSystem<AttachmentConstraintSystem>();
		}

		public FakeEntity RequireEntity(FakeRigidBody rigidBody)
		{
			return  m_World.CreateEntity(rigidBody);
		}

		public FakeEntity RequireEntity(FakeJoint fakeJoint)
		{
			return m_World.CreateEntity(fakeJoint);
		}

		public FakeRigidBody Get(FakeEntity entity)
		{
			return m_World.GetComponent<FakeRigidBody>(entity);
		}

		public void Set(FakeEntity entity, FakeRigidBody rigidBody)
		{
			m_World.SetComponent(entity, rigidBody);
		}

		public void BeginStep()
		{
			m_BeginStepSystem.Update();
		}

		public void EndStep(float deltaTime)
		{
			m_EndStepSystem.DeltaTime = deltaTime;

			m_EndStepSystem.Update();
		}

		public void Step(float deltaTime) { }

		public void ApplyAcceleration(float deltaTime, float3 acceleration) { }

		public void ApplyDrag(float deltaTime) { }

		public void SolveConstraints(float deltaTime)
		{
			m_AttachmentConstraintSystem.DeltaTime = deltaTime;

			m_AttachmentConstraintSystem.Update();
		}
	}
}
