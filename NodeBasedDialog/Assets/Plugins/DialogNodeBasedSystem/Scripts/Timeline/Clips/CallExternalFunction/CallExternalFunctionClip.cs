#if UNITY_TIMELINE
using System;
using UnityEngine;
using UnityEngine.Playables;

namespace cherrydev
{
    [Serializable]
    public class CallExternalFunctionClip : PlayableBehaviour
    {
        public string FunctionName;
        private bool _called;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!Application.isPlaying || _called)
                return;

            DialogBehaviour dialogBehaviour = playerData as DialogBehaviour;
            
            if (dialogBehaviour != null && !string.IsNullOrEmpty(FunctionName))
            {
                dialogBehaviour.CallExternalFunction(FunctionName);
                _called = true;
            }
        }
    }
}
#endif