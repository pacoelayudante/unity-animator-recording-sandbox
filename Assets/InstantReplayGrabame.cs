using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InstantReplay
{

    [DisallowMultipleComponent]
    public class InstantReplayGrabame : MonoBehaviour
    {

        float tInicio = -1f, tEnabled = -1f;
        Animator animator;
        ActivationTrack activationTrack;
        TimelineClip controlClip;
        Rigidbody2D rigid2D;
        Rigidbody rigid;

        public float PlaybackTime
        {
            get
            {
                return animator.playbackTime + tEnabled;
            }
            set
            {
                animator.playbackTime = value - tEnabled;
            }
        }
        public float StartTime
        {
            get { return animator.recorderStartTime + tEnabled; }
        }
        public float StopTime
        {
            get { return animator.recorderStopTime + tEnabled; }
        }

        private void Awake()
        {
            if (!InstantReplay.Activa) { Destroy(this); return; }
            rigid2D = GetComponent<Rigidbody2D>();
            rigid = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            if (animator)
            {
                if (animator.runtimeAnimatorController == null)
                {
                    SospecharTimelineContol();
                }
            }
            else
            {
                if (rigid2D || rigid)
                {
                    animator = gameObject.AddComponent<Animator>();
                    animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
                    animator.runtimeAnimatorController = InstantReplay.ControllerVacio;
                }
            }

            tInicio = InstantReplay.Activa.TActual;
            if (animator) animator.StartRecording(InstantReplay.TamBufferFrames);
        }

        public void OnEnable()
        {
            if (!InstantReplay.Activa) { Destroy(this); return; }
            tEnabled = InstantReplay.Activa.TActual;
            if (animator) animator.StartRecording(InstantReplay.TamBufferFrames);
        }
        private void OnDisable()
        {
            if (!this) return;
            if (!InstantReplay.Activa) { Destroy(this); return; }
            if (InstantReplay.Activa.enabled)
            {
                ActualizarActivationTrack();
				ActualizarAnimatorTrack();
            }
        }

        void ActualizarAnimatorTrack()
        {
            if (animator)
            {
                if (controlClip == null)
                {
                    var controlTrack = InstantReplay.Activa.Timeline.CreateTrack<ControlTrack>(null, "control "+gameObject.name);
                    controlClip = controlTrack.CreateClip<ControlPlayableAsset>();
                    var controlAsset = controlClip.asset as ControlPlayableAsset;
                    controlAsset.updateDirector = controlAsset.updateParticle = controlAsset.active = controlAsset.searchHierarchy = false;
					controlAsset.sourceGameObject.exposedName = gameObject.GetInstanceID().ToString();
                    InstantReplay.Activa.Director.SetGenericBinding(activationTrack, gameObject);
                    InstantReplay.Activa.Director.SetReferenceValue(controlAsset.sourceGameObject.exposedName, gameObject);
                }
                controlClip.duration = animator.recorderStopTime - animator.recorderStartTime;
                controlClip.start = InstantReplay.Activa.TActual - controlClip.duration;
            }
        }
        void ActualizarActivationTrack()
        {
            if (tEnabled == InstantReplay.Activa.TActual) tEnabled = InstantReplay.Activa.tInicio;

            if (activationTrack == null)
            {
                activationTrack = InstantReplay.Activa.Timeline.CreateTrack<ActivationTrack>(null, "activation "+gameObject.name);
                InstantReplay.Activa.Director.SetGenericBinding(activationTrack, gameObject);
            }
            var clipActual = activationTrack.CreateDefaultClip();
            clipActual.start = tEnabled;
            clipActual.duration = InstantReplay.Activa.TActual - tEnabled;
            tEnabled = (float)clipActual.end;
        }
        public void Stop()
        {
            if (activationTrack) ActualizarActivationTrack();
            if (animator)
            {
                animator.StopRecording();
                ActualizarAnimatorTrack();
                gameObject.AddComponent<InstantReplayAnimatorTimeControl>();
            }
        }

        private void SospecharTimelineContol()
        {
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(InstantReplayGrabame))]
    public class InstantReplayGrabameEditor : Editor
    {
        InstantReplayGrabame este;
        private void OnEnable()
        {
            este = target as InstantReplayGrabame;
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Label(este.StartTime + " >>> " + este.StopTime);
        }
    }
#endif

}