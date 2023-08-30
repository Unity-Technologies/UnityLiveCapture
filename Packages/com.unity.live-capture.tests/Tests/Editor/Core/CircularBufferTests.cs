using System;
using NUnit.Framework;

namespace Unity.LiveCapture.Tests.Editor
{
    class CircularBufferTests
    {
        [Test]
        public void CanThrowExceptions()
        {
            var buffer = new CircularBuffer<string>(3);
            Assert.That(() => buffer.PeekFront(), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => buffer.PeekBack(), Throws.TypeOf<InvalidOperationException>());
            buffer.PushBack("one");
            Assert.That(() => buffer[1], Throws.TypeOf<IndexOutOfRangeException>());
            buffer.PopFront();
            Assert.That(() => buffer.PopFront(), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => buffer.PopBack(), Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void CanPushAndPop()
        {
            var buffer = new CircularBuffer<string>(4)
            {
                "A",
                "B",
                "C"
            };

            Assert.That(buffer.PopFront(), Is.EqualTo("A"));
            buffer.PushBack("D");
            buffer.PushFront("1");
            Assert.That(buffer.PeekFront(), Is.EqualTo("1"));
            buffer.PushBack("E");
            // "1" got overwritten
            buffer.PushBack("F");
            // "B" got overwritten
            Assert.That(buffer.Count, Is.EqualTo(4));
            Assert.That(buffer.PeekFront(), Is.EqualTo("C"));
            Assert.That(buffer.PeekBack(), Is.EqualTo("F"));
            buffer.PushFront("2");
            Assert.That(buffer, Is.EquivalentTo(new [] { "2", "C", "D", "E" }));
        }

        [Test]
        public void CanRollOver()
        {
            var buffer = new CircularBuffer<int>(5);
            for (int i = 0; i < 12; i++)
            {
                buffer.PushBack(i);
            }

            Assert.That(buffer, Is.EquivalentTo(new[] {7, 8, 9, 10, 11}));
        }

        [Test]
        public void CanSetCapacity()
        {
            var buffer = new CircularBuffer<string>(5)
            {
                "A",
                "B",
                "C",
                "D",
                "E",
                "F",
                "G"
            };

            buffer.Capacity = 4;
            Assert.That(buffer.Capacity, Is.EqualTo(4));
            Assert.That(buffer, Is.EquivalentTo(new[] {"D", "E", "F", "G"}));
            buffer.Capacity = 6;
            Assert.That(buffer.Capacity, Is.EqualTo(6));
            Assert.That(buffer, Is.EquivalentTo(new[] {"D", "E", "F", "G"}));
        }

        [Test]
        public void CanClear()
        {
            var buffer = new CircularBuffer<string>(3)
            {
                "A",
                "B",
                "C",
                "D",
            };

            Assert.That(buffer.Count, Is.EqualTo(3));
            buffer.Clear();
            Assert.That(buffer.Count, Is.EqualTo(0));
            buffer.PushBack("E");
            buffer.PushBack("F");
            Assert.That(buffer, Is.EquivalentTo(new[] {"E", "F"}));
        }

        [Test]
        public void TestIndexer()
        {
            var buffer = new CircularBuffer<string>(4)
            {
                "A",
                "B",
                "C",
                "D",
            };

            Assert.AreEqual("A", buffer[0]);
            Assert.AreEqual("B", buffer[1]);
            Assert.AreEqual("C", buffer[2]);
            Assert.AreEqual("D", buffer[3]);

            buffer.PushBack("F");

            Assert.AreEqual("B", buffer[0]);
            Assert.AreEqual("C", buffer[1]);
            Assert.AreEqual("D", buffer[2]);
            Assert.AreEqual("F", buffer[3]);
        }

        [Test]
        public void TestInsert()
        {
            var buffer = new CircularBuffer<string>(4)
            {
                "A",
                "B",
                "C",
                "D",
            };

            buffer.PushIndex(buffer.Count, "F");

            Assert.AreEqual("F", buffer[3]);

            buffer.PushIndex(1, "Q");

            Assert.AreEqual("C", buffer[0]);
            Assert.AreEqual("Q", buffer[1]);
            Assert.AreEqual("D", buffer[2]);
            Assert.AreEqual("F", buffer[3]);
        }
    }
}
