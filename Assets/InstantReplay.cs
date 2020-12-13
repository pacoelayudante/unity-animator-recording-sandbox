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

    public class InstantReplay : MonoBehaviour
    {
        static InstantReplay activa;
        public static InstantReplay Activa
        {
            get { return activa; }
        }
        public static RuntimeAnimatorController ControllerVacio { get; private set; }

        public static int TamBufferFrames
        {
            get { return Activa ? Activa.tamBufferFrames : 1; }
        }

        public static GameObject DestroyReplayFriendly(GameObject destruir, bool grabando = true)
        {
            if (!destruir) return destruir;
            if (destruir.scene.rootCount == 0)
            {
                Debug.LogError("Intentando Destruir Prefab");
                return destruir;
            }

            if (!Activa)
            {
                Destroy(destruir);
                return null;
            }
            

            if(grabando){
                var grabame = destruir.GetComponent<InstantReplayGrabame>();
                if (grabame == null) destruir.AddComponent<InstantReplayGrabame>();
                else {
                    destruir.SetActive(false);
                    grabame.Stop();
                }
            }
            destruir.SetActive(false);

            var todosLosComponentes = destruir.GetComponentsInChildren<Component>();
            for (int i = todosLosComponentes.Length - 1; i >= 0; i--)
            {
                var comp = todosLosComponentes[i];
                if (!comp) continue;
                else if (comp as Transform) continue;
                else if (comp as Animator) continue;
                else if (comp as Renderer) continue;
                else if (comp as MeshFilter) continue;
                else if (comp as InstantReplayAnimatorTimeControl) continue;
                else Destroy(comp);
            }
            return destruir;
        }

        List<Animator> animators = new List<Animator>();

        PlayableDirector director;
        public PlayableDirector Director
        {
            get
            {
                if (!director) director = GetComponent<PlayableDirector>();
                if (!director)
                {
                    director = gameObject.AddComponent<PlayableDirector>();
                    director.timeUpdateMode = DirectorUpdateMode.Manual;
                    director.playOnAwake = false;
                }
                return director;
            }
        }
        TimelineAsset timeline;
        public TimelineAsset Timeline
        {
            get
            {
                if (!timeline) Director.playableAsset = timeline = TimelineAsset.CreateInstance<TimelineAsset>();
                return timeline;
            }
        }

        public float TActual
        {
            get
            {
                return Time.time - tInicio;
            }
        }
        public float tInicio { get; private set; }

        public RuntimeAnimatorController controllerVacio;
        public int tamBufferFrames = 6000;
        public float stopTime { get; private set; }
        private void Awake()
        {
            if (activa)
            {
                Destroy(activa.gameObject);
            }
            activa = this;
            stopTime = -1f;
            if (controllerVacio != null) ControllerVacio = controllerVacio;
        }

        private void OnEnable()
        {
            if (stopTime >= 0f)
            {
                enabled = false;
                return;
            }
            tInicio = Time.time;
            foreach (var animator in FindObjectsOfType<Animator>())
            {
                animator.gameObject.AddComponent<InstantReplayGrabame>();
            }
            foreach (var rigid in FindObjectsOfType<Rigidbody>())
            {
                if (rigid.GetComponent<InstantReplayGrabame>() == null) rigid.gameObject.AddComponent<InstantReplayGrabame>();
            }
            foreach (var rigid in FindObjectsOfType<Rigidbody2D>())
            {
                if (rigid.GetComponent<InstantReplayGrabame>() == null) rigid.gameObject.AddComponent<InstantReplayGrabame>();
            }
        }
        private void OnDisable()
        {
            if (stopTime >= 0) return;
            stopTime = Time.time;
            foreach (var grabame in FindObjectsOfType<InstantReplayGrabame>())
            {
                grabame.Stop();
                var go = grabame.gameObject;
                DestroyReplayFriendly(go,false);
                go.SetActive(true);
            }
        }

        public void Play()
        {

        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(InstantReplay))]
    public class InstantReplayEditor : Editor
    {
        InstantReplay este;
        private void OnEnable()
        {
            este = target as InstantReplay;
        }
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            var destruir = EditorGUILayout.ObjectField(null, typeof(GameObject), true) as GameObject;
            if (EditorGUI.EndChangeCheck())
            {
                InstantReplay.DestroyReplayFriendly(destruir);
            }
            GUILayout.Label(este.tInicio + " >>> " + este.stopTime);
            DrawDefaultInspector();
        }
    }
#endif

}