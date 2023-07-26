using System.Collections;

namespace Unity.LiveCapture.Tests.Editor
{
    public static class TestUtils
    {
        /// <summary>
        /// Delays a unity test until the given number of frame updates have occured.
        /// </summary>
        public static IEnumerator WaitForPlayerLoopUpdates(int frames)
        {
            while (frames > 0)
            {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                yield return null;
                frames--;
            }
        }
    }
}
