#if UNITY_TIMELINE
using System;
using UnityEngine;
using UnityEngine.Playables;

namespace cherrydev
{
    [Serializable]
    public class SentencePerformingClip : PlayableBehaviour
    {
        public SentenceNode SentenceNode;
        public DialogBehaviour DialogBehaviour;
        
        private bool _isTextDisplayStarted;
        
        public override void OnGraphStart(Playable playable)
        {
            if (!Application.isPlaying) 
                return;

            _isTextDisplayStarted = false;
        }
        
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!Application.isPlaying) 
                return;

            if (DialogBehaviour == null)
                DialogBehaviour = playerData as DialogBehaviour;

            if (DialogBehaviour == null || SentenceNode == null) return;

            double currentTime = playable.GetTime();
            double duration = playable.GetDuration();

            float progress = (float)(currentTime / duration);

            if (!_isTextDisplayStarted)
            {
                _isTextDisplayStarted = true;

                if (SentenceNode.IsExternalFunc())
                    DialogBehaviour.CallExternalFunction(SentenceNode.GetExternalFunctionName());
            }

            DialogBehaviour.PerformSentenceNode(SentenceNode, progress);
        }
    }
}
#endif