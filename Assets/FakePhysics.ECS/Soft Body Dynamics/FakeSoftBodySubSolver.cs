using FakePhysics.ECS.Dynamics;
using FakePhysics.ECS.SoftBodyDynamics.Systems;
using Unity.Entities;
using Unity.Mathematics;

namespace FakePhysics.ECS.SoftBodyDynamics
{
	public class FakeSoftBodySubSolver : IFakeSubSolver, IFakeDynamicSubSolver, IFakeConstrainedSubSolver
	{
		private World m_World;

		private EntityArchetype m_ParticleArchetype;

		private BeginStepSystem m_BeginStepSystem;
		private StepSystem m_StepSystem;
		private EndStepSystem m_EndStepSystem;
		private ApplyAccelerationSystem m_ApplyAccelerationSystem;
		private ApplyDragSystem m_ApplyDragSystem;

		public void Init(World world)
		{
			m_World = world;

			m_ParticleArchetype = m_World.EntityManager.CreateArchetype(typeof(FakeParticle));

			m_BeginStepSystem = m_World.CreateSystemManaged<BeginStepSystem>();
			m_StepSystem = m_World.CreateSystemManaged<StepSystem>();
			m_EndStepSystem = m_World.CreateSystemManaged<EndStepSystem>();
			m_ApplyAccelerationSystem = m_World.CreateSystemManaged<ApplyAccelerationSystem>();
			m_ApplyDragSystem = m_World.CreateSystemManaged<ApplyDragSystem>();
		}

		public Entity RequireEntity(FakeParticle particle)
		{
			var entity = m_World.EntityManager.CreateEntity(m_ParticleArchetype);

			m_World.EntityManager.SetComponentData(entity, particle);

			return entity;
		}

		public FakeParticle Get(Entity entity)
		{
			return m_World.EntityManager.GetComponentData<FakeParticle>(entity);
		}

		public void BeginStep()
		{
			m_BeginStepSystem.Update();
		}

		public void Step(float deltaTime)
		{
			m_StepSystem.DeltaTime = deltaTime;

			m_StepSystem.Update();
		}

		public void EndStep(float deltaTime)
		{
			m_EndStepSystem.DeltaTime = deltaTime;

			m_EndStepSystem.Update();
		}

		public void ApplyAcceleration(float deltaTime, float3 acceleration)
		{
			m_ApplyAccelerationSystem.DeltaTime = deltaTime;
			m_ApplyAccelerationSystem.Acceleration = acceleration;

			m_ApplyAccelerationSystem.Update();
		}

		public void ApplyDrag(float deltaTime)
		{
			m_ApplyDragSystem.DeltaTime = deltaTime;

			m_ApplyDragSystem.Update();
		}

		public void SolveConstraints(float deltaTime)
		{

		}
	}
}
