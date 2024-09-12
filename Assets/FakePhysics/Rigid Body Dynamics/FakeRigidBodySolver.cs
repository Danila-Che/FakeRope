using FakePhysics.CollisionDetection;
using FakePhysics.Dynamics;
using FakePhysics.RigidBodyDynamics.Systems;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace FakePhysics.RigidBodyDynamics
{
	public class FakeRigidBodySolver : IDynamicBody
	{
		private BeginStepSystem m_BeginStepSystem;
		private StepSystem m_StepSystem;
		private ApplyAccelerationSystem m_ApplyAccelerationSystem;
		private ApplyDragSystem m_ApplyDragSystem;
		private EndStepSystem m_EndStepSystem;

		private FakeSolver m_ParentSolver;
		private EntityArchetype m_Archetype;

		public void RegisterTo(FakeSolver parentSolver)
		{
			m_ParentSolver = parentSolver;
			m_Archetype = m_ParentSolver.EntityManager.CreateArchetype(typeof(TEMP_FakeRigidBody));

			var world = m_ParentSolver.DynamicWorld;

			m_BeginStepSystem = world.CreateSystemManaged<BeginStepSystem>();
			m_StepSystem = world.CreateSystemManaged<StepSystem>();
			m_ApplyAccelerationSystem = world.CreateSystemManaged<ApplyAccelerationSystem>();
			m_ApplyDragSystem = world.CreateSystemManaged<ApplyDragSystem>();
			m_EndStepSystem = world.CreateSystemManaged<EndStepSystem>();

			m_ParentSolver.Add(this);
		}

		public Entity RequireEnity(Rigidbody rigidbody, FakeBoxCollider boxCollider)
		{
			var entity = m_ParentSolver.EntityManager.CreateEntity(m_Archetype);
			var rigidBody = TEMP_FakeRigidBody.Create(rigidbody, boxCollider);

			m_ParentSolver.EntityManager.SetComponentData(entity, rigidBody);

			return entity;
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

		public void EndStep(float deltaTime)
		{
			m_EndStepSystem.DeltaTime = deltaTime;

			m_EndStepSystem.Update();
		}
	}
}
