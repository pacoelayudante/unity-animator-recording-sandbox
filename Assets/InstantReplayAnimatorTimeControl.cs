using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace InstantReplay
{
[DisallowMultipleComponent]
    public class InstantReplayAnimatorTimeControl : MonoBehaviour, ITimeControl
    {

        Animator animator;
public float StartTime{
	get{return animator?animator.recorderStartTime:0f;}
}
public float StopTime{
	get{return animator?animator.recorderStopTime:0f;}
}
public float PlaybackTime{
	get{return animator?animator.playbackTime:0f;}
}
public AnimatorRecorderMode Estado {
	get{return animator?animator.recorderMode:AnimatorRecorderMode.Offline;}
}

bool onControlTime=false;

        private void OnEnable()
        {
            animator = GetComponent<Animator>();
			animator.applyRootMotion = true;
			animator.StartPlayback();
        }

        public void OnControlTimeStart()
        {
			onControlTime = true;
        }

        public void OnControlTimeStop()
        {
			onControlTime = false;
        }

        public void SetTime(double time)
        {
            if(onControlTime)animator.playbackTime = (float)time + animator.recorderStartTime;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(InstantReplayAnimatorTimeControl))]
    public class InstantReplayAnimatorTimeControlEditor : UnityEditor.Editor
    {
        InstantReplayAnimatorTimeControl este;
        private void OnEnable()
        {
            este = target as InstantReplayAnimatorTimeControl;
        }
        public override void OnInspectorGUI()
        {
            GUILayout.Label(este.Estado.ToString());
            GUILayout.Label(este.StartTime + " >> " + este.PlaybackTime + " >> "+este.StopTime);
        }
    }
#endif

}
