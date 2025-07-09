#if UNITY_TIMELINE
using UnityEngine.Timeline;

namespace cherrydev
{
    /// <summary>
    /// Represents a custom Timeline track used for managing and controlling dialog behaviors
    /// </summary>
    /// <seealso cref="SentencePerformingClipAsset"/>
    /// <seealso cref="CallExternalFunctionClipAsset"/>
    /// <seealso cref="DialogBehaviour"/>
    [TrackColor(0.2f, 0.7f, 1f)]
    [TrackBindingType(typeof(DialogBehaviour))]
    [TrackClipType(typeof(SentencePerformingClipAsset))]
    [TrackClipType(typeof(CallExternalFunctionClipAsset))]
    public class DialogBehaviourTrack : TrackAsset
    {
    }
}
#endif