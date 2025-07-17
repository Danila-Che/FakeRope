using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;

namespace FakePhysics.ECS.Utilities
{
    public abstract class FakeSystemBase
    {
        private const int k_MaxCacheSize = 3;

        private const int k_InnerloopBatchCount = 4;

        private FakeWorld m_World;
        private readonly IFakeComponentContainer[] m_Cache;
        private int m_Index;

        public FakeSystemBase()
        {
            m_Cache = new IFakeComponentContainer[k_MaxCacheSize];
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Cache[0] != null && m_Cache[0].Length > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(FakeWorld world)
        {
            m_World = world;
            m_Index = 0;
            
            OnCreate();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Update()
        {
            if (IsValid)
            {
                OnUpdate();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void OnCreate();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void OnUpdate();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RequireAsPrimary<T>()
            where T : unmanaged
        {
            m_Cache[0] = m_World.RequireContainer<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RequireAsSecondary<T>()
            where T : unmanaged
        {
            m_Cache[1 + m_Index] = m_World.RequireContainer<T>();
            m_Index++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected FakeChunksCollection<T> Get<T>()
            where T : unmanaged
        {
            for (int i = 0; i < 1 + m_Index; i++)
            {
                if (m_Cache[i] is FakeComponentContainer<T> cache)
                {
                    return cache.Components;
                }
            }

            throw new OperationCanceledException($"There is no cache assotiated with type {typeof(T)}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ScheduleParallel<TJob>(TJob job)
            where TJob : unmanaged, IJobParallelFor
        {
            m_Cache[0].Dependency = job.Schedule(m_Cache[0].Length, k_InnerloopBatchCount, m_Cache[0].Dependency);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Schedule<TJob>(TJob job)
            where TJob : unmanaged, IJob
        {
            if (m_Index == 0)
            {
                ScheduleAsSingle(job);
            }
            else if (m_Index == 1)
            {
                ScheduleAsCouple(job);
            }
            else if (m_Index == 2)
            {
                ScheduleAsTriple(job);
            }
            else
            {
                ScheduleAsMany(job);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScheduleAsSingle<TJob>(TJob job)
            where TJob : unmanaged, IJob
        {
            m_Cache[0].Dependency = job.Schedule(m_Cache[0].Dependency);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScheduleAsCouple<TJob>(TJob job)
            where TJob : unmanaged, IJob
        {
            var dependency = JobHandle.CombineDependencies(m_Cache[0].Dependency, m_Cache[1].Dependency);

            dependency = job.Schedule(dependency);

            m_Cache[0].Dependency = dependency;
            m_Cache[1].Dependency = dependency;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScheduleAsTriple<TJob>(TJob job)
            where TJob : unmanaged, IJob
        {
            var dependency = JobHandle.CombineDependencies(m_Cache[0].Dependency, m_Cache[1].Dependency, m_Cache[2].Dependency);

            dependency = job.Schedule(dependency);

            m_Cache[0].Dependency = dependency;
            m_Cache[1].Dependency = dependency;
            m_Cache[2].Dependency = dependency;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScheduleAsMany<TJob>(TJob job)
            where TJob : unmanaged, IJob
        {
            var length = 1 + m_Index;
            var dependencies = new NativeArray<JobHandle>(length, Allocator.Temp);

            for (int i = 0; i < length; i++)
            {
                dependencies[i] = m_Cache[i].Dependency;
            }

            var dependency = JobHandle.CombineDependencies(dependencies);

            dependency = job.Schedule(dependency);

            for (int i = 0; i < length; i++)
            {
                m_Cache[i].Dependency = dependency;
            }

            dependencies.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ScheduleFor<TJob>(TJob job)
            where TJob : unmanaged, IJobFor
        {
            if (m_Index == 0)
            {
                ScheduleForAsSingle(job);
            }
            else if (m_Index == 1)
            {
                ScheduleForAsCouple(job);
            }
            else if (m_Index == 2)
            {
                ScheduleForAsTriple(job);
            }
            else
            {
                ScheduleForAsMany(job);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScheduleForAsSingle<TJob>(TJob job)
            where TJob : unmanaged, IJobFor
        {
            m_Cache[0].Dependency = job.Schedule(m_Cache[0].Length, m_Cache[0].Dependency);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScheduleForAsCouple<TJob>(TJob job)
            where TJob : unmanaged, IJobFor
        {
            var dependency = JobHandle.CombineDependencies(m_Cache[0].Dependency, m_Cache[1].Dependency);

            dependency = job.Schedule(m_Cache[0].Length, dependency);

            m_Cache[0].Dependency = dependency;
            m_Cache[1].Dependency = dependency;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScheduleForAsTriple<TJob>(TJob job)
            where TJob : unmanaged, IJobFor
        {
            var dependency = JobHandle.CombineDependencies(m_Cache[0].Dependency, m_Cache[1].Dependency, m_Cache[2].Dependency);

            dependency = job.Schedule(m_Cache[0].Length,dependency);

            m_Cache[0].Dependency = dependency;
            m_Cache[1].Dependency = dependency;
            m_Cache[2].Dependency = dependency;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScheduleForAsMany<TJob>(TJob job)
            where TJob : unmanaged, IJobFor
        {
            var length = 1 + m_Index;
            var dependencies = new NativeArray<JobHandle>(length, Allocator.Temp);

            for (int i = 0; i < length; i++)
            {
                dependencies[i] = m_Cache[i].Dependency;
            }

            var dependency = JobHandle.CombineDependencies(dependencies);

            dependency = job.Schedule(m_Cache[0].Length,dependency);

            for (int i = 0; i < length; i++)
            {
                m_Cache[i].Dependency = dependency;
            }

            dependencies.Dispose();
        }
    }
}
