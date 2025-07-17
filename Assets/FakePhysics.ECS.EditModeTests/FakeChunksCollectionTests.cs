using FakePhysics.ECS.Utilities;
using NUnit.Framework;
using Unity.Collections;

namespace FakePhysics.ECS.EditModeTests
{
    [TestFixture]
    public class FakeChunksCollectionTests
    {
        private struct Stub
        {
            public int Value;
        }

        [Test]
        public void Test_CreateChunksCollection()
        {
            using var chunksCollection = new FakeChunksCollection<Stub>(Allocator.Temp, maxBufferCapacityInBytes: 16);

            Assert.That(chunksCollection.IsCreated, Is.True);
            Assert.That(chunksCollection.BufferCount, Is.EqualTo(1));
            Assert.That(chunksCollection.BufferCapacity, Is.EqualTo(4));
        }

        [Test]
        public void Test_IsNotCreatedChunksCollection()
        {
            using var chunksCollection = default(FakeChunksCollection<Stub>);

            Assert.That(chunksCollection.IsCreated, Is.False);
        }

        [Test]
        public void Test_ModifyElementOfChunksCollection()
        {
            var chunksCollection = new FakeChunksCollection<Stub>(Allocator.Temp, maxBufferCapacityInBytes: 16);

            try
            {
                chunksCollection[0] = new Stub { Value = 42 };

                Assert.That(chunksCollection[0].Value, Is.EqualTo(42));
            }
            finally
            {
                chunksCollection.Dispose();
            }
        }

        [Test]
        public void Test_ExtendChunksCollection()
        {
            var chunksCollection = new FakeChunksCollection<Stub>(Allocator.Temp, maxBufferCapacityInBytes: 16);

            try
            {
                Assert.That(chunksCollection.BufferCount, Is.EqualTo(1));

                for (int i = 0; i < 8; i++)
                {
                    chunksCollection.Create(new Stub());
                }

                Assert.That(chunksCollection.BufferCount, Is.EqualTo(2));
            }
            finally
            {
                chunksCollection.Dispose();
            }
        }
    }
}
