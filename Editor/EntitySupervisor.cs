using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Jtfer.Ecp.Unity.Editor
{
    [CustomEditor(typeof(SupervisorObserver))]
    sealed class SupervisorObserverInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var observer = target as SupervisorObserver;
            var stats = observer.GetStats();
            var guiEnabled = GUI.enabled;
            GUI.enabled = true;
            GUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Components", stats.Components.ToString());
            EditorGUILayout.LabelField("OneFrame components", stats.OneFrameComponents.ToString());
            EditorGUILayout.LabelField("Filters", stats.Filters.ToString());
            EditorGUILayout.LabelField("Active entities", stats.ActiveEntities.ToString());
            EditorGUILayout.LabelField("Reserved entities", stats.ReservedEntities.ToString());
            GUILayout.EndVertical();
            GUI.enabled = guiEnabled;
        }
    }
}