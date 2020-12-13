using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Animator))]
public class RecordAndPlay : MonoBehaviour {

	Rigidbody rigid;
	Rigidbody Rigid
	{
		get
		{
			if (rigid == null)
			{
				rigid = GetComponent<Rigidbody>();
			}
			return rigid;
		}
	}

	Animator animator;
	public Animator Animator
	{
		get
		{
			if (animator == null)
			{
				animator = GetComponent<Animator>();
			}
			return animator;
		}
	}

public bool autoPlay{get;set;}
	public float playbackTimeNormalizado {
		get{ return Mathf.InverseLerp(recorderStopTime,recorderStartTime, Animator.playbackTime);}
		set{ Animator.playbackTime= Mathf.Lerp(recorderStartTime,recorderStopTime, value%1f);}
	}
	public float playbackTime {
		get{ return Animator.playbackTime;}
		set{ Animator.playbackTime= value%recorderStopTime;}
	}
	public float recorderStartTime {
		get{ return Animator.recorderStartTime;}
	}
	public float recorderStopTime {
		get{ return Animator.recorderStopTime;}
	}

private void Update() {
	if (recorderMode == AnimatorRecorderMode.Playback && autoPlay){
		playbackTime += Time.deltaTime;
	}
}

private void OnEnable() {
	if (recorderStopTime > 0f && autoPlay) {
		PlayBack();
		playbackTime=recorderStartTime;
	}
}

	public AnimatorRecorderMode recorderMode{get{return Animator.recorderMode;}}
	public void Grabar(){
			if (recorderMode == AnimatorRecorderMode.Playback) {
				Animator.StopPlayback();
				if(Rigid)Rigid.isKinematic = false;
			}
			if (recorderMode == AnimatorRecorderMode.Record) Animator.StopRecording();
			else Animator.StartRecording(1000);
	}
	public void PlayBack(){
			if (recorderMode == AnimatorRecorderMode.Record)  Animator.StopRecording();
			if (recorderMode == AnimatorRecorderMode.Playback) {
				Animator.StopPlayback();
				Animator.applyRootMotion = false;
				if(Rigid)Rigid.isKinematic = false;
			}
			else {
				Animator.StartPlayback();
				if(Rigid)Rigid.isKinematic = true;
				Animator.applyRootMotion = true;
			}
	}

}

#if UNITY_EDITOR
[CustomEditor(typeof(RecordAndPlay))]
public class RecordAndPlayEditor : Editor{
RecordAndPlay record;
string parmetro;

private void OnEnable() {
	record = target as RecordAndPlay;
}

	public  override void OnInspectorGUI(){
		GUI.enabled = EditorApplication.isPlaying;
		GUILayout.Label( record.recorderMode.ToString() );
		GUILayout.Label( record.recorderStartTime+" >>> "+record.recorderStopTime+" === "+(record.recorderStopTime- record.recorderStartTime) );
		if(GUILayout.Button("Grabar")){
			record.Grabar();
		}
		if(GUILayout.Button("Playback")){
			record.PlayBack();
		}
		GUI.enabled = record.recorderMode == AnimatorRecorderMode.Playback;
		EditorGUI.BeginChangeCheck();
		var autorec = EditorGUILayout.Toggle("autoplay",record.autoPlay);
		var pos = EditorGUILayout.Slider( record.playbackTime,record.recorderStartTime,record.recorderStopTime );
		if(EditorGUI.EndChangeCheck()){
			record.playbackTime = pos;
			record.autoPlay = autorec ;
		}

GUI.enabled=true;
		parmetro = EditorGUILayout.DelayedTextField(parmetro);
		EditorGUILayout.Toggle( record.Animator.IsParameterControlledByCurve(parmetro) );
		GUILayout.Label(record.Animator.parameterCount.ToString());
	}
}
#endif