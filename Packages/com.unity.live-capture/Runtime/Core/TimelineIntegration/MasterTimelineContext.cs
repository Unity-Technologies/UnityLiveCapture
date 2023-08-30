using UnityEngine.Playables;

namespace Unity.LiveCapture
{
    class MasterTimelineContext
    {
        class MasterDirectorProvider : IDirectorProvider
        {
            public PlayableDirector Director => Timeline.MasterDirector;
        }

        public static DirectorContext Instance { get; } = new DirectorContext(new MasterDirectorProvider());
    }
}
