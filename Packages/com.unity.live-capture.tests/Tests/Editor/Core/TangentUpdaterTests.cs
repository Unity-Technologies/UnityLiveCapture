using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using UnityEditor;

namespace Unity.LiveCapture.Tests.Editor
{
    public class TangentUpdaterTests
    {
        IEnumerable<T> ToEnumerable<T>(IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        [Test]
        public void FirstTangentSingleKey()
        {
            var impl = Substitute.For<ITangentUpdaterImpl<int>>();
            var updater = new TangentUpdater<int>(impl);

            updater.Add(10);
            updater.Flush();

            var values = ToEnumerable(updater).ToArray();

            impl.Received(1).UpdateFirstTangent(10, 10);
            impl.DidNotReceiveWithAnyArgs().UpdateLastTangent(Arg.Any<int>(), Arg.Any<int>());
            impl.DidNotReceiveWithAnyArgs().UpdateTangents(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
        }

        [Test]
        public void FirstTangentTwoKeys()
        {
            var impl = Substitute.For<ITangentUpdaterImpl<int>>();
            var updater = new TangentUpdater<int>(impl);

            updater.Add(10);
            updater.Add(11);
            updater.Flush();

            var values = ToEnumerable(updater).ToArray();

            impl.Received(1).UpdateFirstTangent(10, 11);
            impl.Received(1).UpdateLastTangent(11, Arg.Any<int>());
            impl.DidNotReceiveWithAnyArgs().UpdateTangents(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
        }

        [Test]
        public void FirstTangentSeveralKeys()
        {
            var impl = Substitute.For<ITangentUpdaterImpl<int>>();
            var updater = new TangentUpdater<int>(impl);

            updater.Add(10);
            updater.Add(11);
            updater.Add(12);
            updater.Flush();

            var values = ToEnumerable(updater).ToArray();

            impl.Received(1).UpdateFirstTangent(10, 11);
            impl.Received(1).UpdateLastTangent(12, Arg.Any<int>());
            impl.Received(1).UpdateTangents(11, Arg.Any<int>(), 12);
        }
    }
}
