using FakePhysics.ECS.Dynamics;
using FakePhysics.ECS.RigidBodyDynamics.Systems;
using Unity.Entities;
using Unity.Mathematics;

namespace FakePhysics.ECS.RigidBodyDynamics
{
	public sealed class FakeRigidBodySubSolver : IFakeSubSolver, IFakeDynamicSubSolver, IFakeConstrainedSubSolver
	{
		private World m_World;

		private BeginStepSystem m_BeginStepSystem;
		private EndStepSystem m_EndStepSystem;
		private AttachmentConstraintSystem m_AttachmentConstraintSystem;

		private EntityArchetype m_RigidBodyArchetype;
		private EntityArchetype m_JointArchetype;

		public void Init(World world)
		{
			m_World = world;

			m_RigidBodyArchetype = m_World.EntityManager.CreateArchetype(typeof(FakeRigidBody));
			m_JointArchetype = m_World.EntityManager.CreateArchetype(typeof(FakeJoint));

			m_BeginStepSystem = m_World.CreateSystemManaged<BeginStepSystem>();
			m_EndStepSystem = m_World.CreateSystemManaged<EndStepSystem>();
			m_AttachmentConstraintSystem = m_World.CreateSystemManaged<AttachmentConstraintSystem>();
		}

		public Entity RequireEntity(FakeRigidBody rigidBody)
		{
			var entity = m_World.EntityManager.CreateEntity(m_RigidBodyArchetype);

			m_World.EntityManager.SetComponentData(entity, rigidBody);

			return entity;
		}

		public Entity RequireEntity(FakeJoint fakeJoint)
		{
			var entity = m_World.EntityManager.CreateEntity(m_JointArchetype);

			m_World.EntityManager.SetComponentData(entity, fakeJoint);

			return entity;
		}

		public FakeRigidBody Get(Entity entity)
		{
			return m_World.EntityManager.GetComponentData<FakeRigidBody>(entity);
		}

		public void Set(Entity entity, FakeRigidBody rigidBody)
		{
			m_World.EntityManager.SetComponentData(entity, rigidBody);
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
