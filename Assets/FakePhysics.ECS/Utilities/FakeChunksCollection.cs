using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace FakePhysics.ECS.Utilities
{
    public unsafe struct FakeChunksCollection<T> : IDisposable
        where T : unmanaged
    {
        private const int k_MaxBufferCapacityInBytes = 16 * 1024;

        [NativeDisableUnsafePtrRestriction]
        private T** m_Buffers;
        public int m_Index;
        private int m_BufferCount;
        private int m_BufferCapacity;
        private readonly Allocator m_Allocator;

        public FakeChunksCollection(Allocator allocator, int maxBufferCapacityInBytes = k_MaxBufferCapacityInBytes)
        {
            var sizeInBytes = sizeof(T);
            m_BufferCapacity = math.max(1, maxBufferCapacityInBytes / sizeInBytes);

            m_BufferCount = 1;
            m_Index = 0;
            m_Allocator = allocator;

            m_Buffers = (T**)UnsafeUtility.Malloc(sizeof(T*) * m_BufferCount, sizeof(T*), allocator);

            for (int i = 0; i < m_BufferCount; i++)
            {
                m_Buffers[i] = CreateBuffer();
            }
        }
        
        public readonly int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_BufferCount * m_BufferCapacity;
        }

        public readonly int BufferCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_BufferCount;
        }

        public readonly int BufferCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_BufferCapacity;
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return m_Buffers != null;
            }
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Index;
        }

        public T this[FakeEntity entity]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Get(entity.ID);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(entity.ID, value);
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Get(index);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(index, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            for (int i = 0; i < m_BufferCount; i++)
            {
                if (m_Buffers[i] != null)
                {
                    UnsafeUtility.Free(m_Buffers[i], m_Allocator);
                    m_Buffers[i] = null;
                }
            }

            if (m_Buffers != null)
            {
                UnsafeUtility.Free(m_Buffers, m_Allocator);
            }

            m_Buffers = null;

            m_BufferCount = 0;
            m_BufferCapacity = 0;
            m_Index = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FakeEntity Create(T component)
        {
            if (m_Index == Capacity)
            {
                Extend();
            }

            var id = m_Index;
            this[m_Index] = component;
            m_Index++;

            return new FakeEntity(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Extend()
        {
            var newBufferCount = m_BufferCount + 1;
            var newBuffers = (T**)UnsafeUtility.Malloc(sizeof(T*) * newBufferCount, sizeof(T*), m_Allocator);

            for (int i = 0; i < m_BufferCount; i++)
            {
                newBuffers[i] = m_Buffers[i];
            }

            newBuffers[newBufferCount - 1] = CreateBuffer();

            UnsafeUtility.Free(m_Buffers, m_Allocator);
            m_Buffers = newBuffers;
            m_BufferCount = newBufferCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T Get(int index)
        {
            var bufferIndex = index / m_BufferCapacity;
            index -= bufferIndex * m_BufferCapacity;

            return m_Buffers[bufferIndex][index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Set(int index, T value)
        {
           var bufferIndex = index / m_BufferCapacity;
            index -= bufferIndex * m_BufferCapacity;
            
            m_Buffers[bufferIndex][index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly T* CreateBuffer()
        {
            return (T*)UnsafeUtility.Malloc(sizeof(T) * m_BufferCapacity, UnsafeUtility.AlignOf<T>(), m_Allocator);
        }
    }
}
