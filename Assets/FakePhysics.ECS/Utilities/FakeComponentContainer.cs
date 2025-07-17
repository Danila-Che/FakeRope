using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace FakePhysics.ECS.Utilities
{
    public interface IFakeComponentContainer : IDisposable
    {
        int Length { get; }

        JobHandle Dependency { get; set; }
    }

    public class FakeComponentContainer<T> : IFakeComponentContainer
        where T : unmanaged
    {
        private FakeChunksCollection<T> m_Components;
        private JobHandle m_Dependency; 

        public FakeComponentContainer()
        {
            if (UnsafeUtility.SizeOf<T>() == 0)
            {
                throw new InvalidOperationException("The size of a component cannot be zero.");
            }

            m_Components = new FakeChunksCollection<T>(Allocator.Persistent, maxBufferCapacityInBytes: 1024);
        }

        public FakeChunksCollection<T> Components
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Components;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Components.Length;
        }

        public JobHandle Dependency
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Dependency;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => m_Dependency = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            m_Dependency.Complete();

            if (m_Components.IsCreated)
            {
                m_Components.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FakeEntity Create(T component)
        {
            m_Dependency.Complete();

            return m_Components.Create(component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(FakeEntity entity)
        {
            CheckEntityExistence(entity);
            m_Dependency.Complete();

            return m_Components[entity];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(FakeEntity entity, T component)
        {
            CheckEntityExistence(entity);
            m_Dependency.Complete();

            m_Components[entity] = component;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckEntityExistence(FakeEntity entity)
        {
            if (entity.ID >= m_Components.Length)
            {
                throw new ArgumentException(nameof(entity), "There is no such entity");
            }
        }
    }
}
