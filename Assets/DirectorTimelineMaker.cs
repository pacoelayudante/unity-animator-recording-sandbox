using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(PlayableDirector))]
public class DirectorTimelineMaker : MonoBehaviour
{

    PlayableDirector director;
    PlayableDirector Director
    {
        get
        {
            return director ? director : director = GetComponent<PlayableDirector>();
        }
    }
    TimelineAsset timeline;
    TimelineAsset Timeline
    {
        get
        {
            if (!timeline)
            {
                timeline = TimelineAsset.CreateInstance<TimelineAsset>();
                Director.playableAsset = timeline;
            }
            return timeline;
        }
    }

    public void GenerarTrack(GameObject go)
    {
        if (!go) return;
        if (go.scene.rootCount == 0) return;
        //	var niuTrack = ActivationTrack.CreateInstance<ActivationTrack>();
        var niuTrack = Timeline.CreateTrack<ActivationTrack>(null, "Activation " + go.name);
		var clip = niuTrack.CreateDefaultClip();
		Director.SetGenericBinding(niuTrack, go);
		clip.start=.5f;
		clip.duration=1f;
		Timeline.fixedDuration = 1.5f;
		//Timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
        //niuTrack.
		Director.RebuildGraph();
		Director.Play();
    }

}


#if UNITY_EDITOR
[CustomEditor(typeof(DirectorTimelineMaker))]
class DirectorTimelineMakerEditor : Editor
{
    DirectorTimelineMaker este;

    private void OnEnable()
    {
        este = target as DirectorTimelineMaker;
    }

    public override void OnInspectorGUI()
    {
		GUI.enabled = EditorApplication.isPlaying;
        EditorGUI.BeginChangeCheck();
        var go = EditorGUILayout.ObjectField(null, typeof(GameObject), true) as GameObject;
        if (EditorGUI.EndChangeCheck())
        {
            este.GenerarTrack(go);
        }
    }
}
#endif