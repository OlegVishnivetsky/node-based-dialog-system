#if UNITY_TIMELINE
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace cherrydev
{
    [Serializable]
    public class CallExternalFunctionClipAsset : PlayableAsset, ITimelineClipAsset
    {
        public string FunctionName;

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            ScriptPlayable<CallExternalFunctionClip> playable = ScriptPlayable<CallExternalFunctionClip>.Create(graph);
            CallExternalFunctionClip behaviour = playable.GetBehaviour();
            behaviour.FunctionName = FunctionName;
            return playable;
        }
    }
}
#endif