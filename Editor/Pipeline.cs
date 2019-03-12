using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Jtfer.Ecp.Unity.Editor
{
    [CustomEditor(typeof(PipelineObserver))]
    sealed class PipelineObserverInspector : UnityEditor.Editor
    {
        static IPreInitOperation[] _preInitList = new IPreInitOperation[32];
        static Stack<IInitOperation[]> _initList = new Stack<IInitOperation[]>(8);
        static Stack<IUpdateOperation[]> _runList = new Stack<IUpdateOperation[]>(8);

        public override void OnInspectorGUI()
        {
            var savedState = GUI.enabled;
            GUI.enabled = true;
            var observer = target as PipelineObserver;
            var systems = observer.GetSystems();
            int count;

            count = systems.GetPreInitSystems(ref _preInitList);
            if (count > 0)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("PreInitialize operations", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                for (var i = 0; i < count; i++)
                {
                    EditorGUILayout.LabelField(_preInitList[i].GetType().Name);
                    _preInitList[i] = null;
                }
                EditorGUI.indentLevel--;
                GUILayout.EndVertical();
            }

            GUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Initialize operation", EditorStyles.boldLabel);
            OnInitSystemsGUI(systems);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Update systems", EditorStyles.boldLabel);
            OnRunSystemsGUI(systems);
            GUILayout.EndVertical();

            GUI.enabled = savedState;
        }

        void OnInitSystemsGUI(Pipeline pipeline)
        {
            var initList = _initList.Count > 0 ? _initList.Pop() : null;
            var count = pipeline.GetInitSystems(ref initList);
            if (count > 0)
            {
                EditorGUI.indentLevel++;
                for (var i = 0; i < count; i++)
                {
                    var asSystems = initList[i] as Pipeline;
                    EditorGUILayout.LabelField(asSystems != null ? asSystems.Name : initList[i].GetType().Name);
                    if (asSystems != null)
                    {
                        OnInitSystemsGUI(asSystems);
                    }
                    initList[i] = null;
                }
                EditorGUI.indentLevel--;
            }
            _initList.Push(initList);
        }

        void OnRunSystemsGUI(Pipeline pipeline)
        {
            var runList = _runList.Count > 0 ? _runList.Pop() : null;
            var count = pipeline.GetRunSystems(ref runList);
            if (count > 0)
            {
                EditorGUI.indentLevel++;
                for (var i = 0; i < count; i++)
                {
                    var asSystems = runList[i] as Pipeline;
                    var name = asSystems != null ? asSystems.Name : runList[i].GetType().Name;
                    pipeline.DisabledInDebugSystems[i] = !EditorGUILayout.ToggleLeft(name, !pipeline.DisabledInDebugSystems[i]);
                    if (asSystems != null)
                    {
                        GUI.enabled = !pipeline.DisabledInDebugSystems[i];
                        OnRunSystemsGUI(asSystems);
                        if (pipeline.DisabledInDebugSystems[i])
                        {
                            GUI.enabled = true;
                        }
                    }
                    runList[i] = null;
                }
                EditorGUI.indentLevel--;
            }
            _runList.Push(runList);
        }
    }
}
