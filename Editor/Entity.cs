using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Jtfer.Ecp.Unity.Editor
{
    [CustomEditor(typeof(EntityObserver))]
    sealed class EntityObserverInspector : UnityEditor.Editor
    {
        const int MaxFieldToStringLength = 128;

        static object[] _componentsCache = new object[32];

        EntityObserver _entity;

        public override void OnInspectorGUI()
        {
            if (_entity.World != null)
            {
                var guiEnabled = GUI.enabled;
                GUI.enabled = true;
                DrawComponents();
                GUI.enabled = guiEnabled;
                EditorUtility.SetDirty(target);
            }
        }

        void OnEnable()
        {
            _entity = target as EntityObserver;
        }

        void OnDisable()
        {
            _entity = null;
        }

        void DrawComponents()
        {
            var count = _entity.World.GetComponents(_entity.Id, ref _componentsCache);
            for (var i = 0; i < count; i++)
            {
                var component = _componentsCache[i];
                _componentsCache[i] = null;
                var type = component.GetType();
                GUILayout.BeginVertical(GUI.skin.box);
                var typeName = EditorHelpers.GetCleanGenericTypeName(type);
                if (!EcsComponentInspectors.Render(typeName, type, component, _entity))
                {
                    EditorGUILayout.LabelField(typeName, EditorStyles.boldLabel);
                    var indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel++;
                    foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
                    {
                        DrawTypeField(component, field, _entity);
                    }
                    EditorGUI.indentLevel = indent;
                }
                GUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }

        void DrawTypeField(object instance, FieldInfo field, EntityObserver entity)
        {
            var fieldValue = field.GetValue(instance);
            var fieldType = field.FieldType;
            if (!EcsComponentInspectors.Render(field.Name, fieldType, fieldValue, entity))
            {
                if (fieldType == typeof(UnityEngine.Object) || fieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(field.Name, GUILayout.MaxWidth(EditorGUIUtility.labelWidth - 16));
                    var guiEnabled = GUI.enabled;
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField(fieldValue as UnityEngine.Object, fieldType, false);
                    GUI.enabled = guiEnabled;
                    GUILayout.EndHorizontal();
                    return;
                }
                var strVal = fieldValue != null ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", fieldValue) : "null";
                if (strVal.Length > MaxFieldToStringLength)
                {
                    strVal = strVal.Substring(0, MaxFieldToStringLength);
                }
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(field.Name, GUILayout.MaxWidth(EditorGUIUtility.labelWidth - 16));
                EditorGUILayout.SelectableLabel(strVal, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
                GUILayout.EndHorizontal();
            }
        }
    }

    static class EcsComponentInspectors
    {
        static readonly Dictionary<Type, IComponentInspector> _inspectors = new Dictionary<Type, IComponentInspector>();

        static EcsComponentInspectors()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(IComponentInspector).IsAssignableFrom(type) && !type.IsInterface)
                    {
                        var inspector = Activator.CreateInstance(type) as IComponentInspector;
                        var componentType = inspector.GetFieldType();
                        if (_inspectors.ContainsKey(componentType))
                        {
                            Debug.LogWarningFormat("Inspector for \"{0}\" already exists, new inspector will be used instead.", componentType.Name);
                        }
                        _inspectors[componentType] = inspector;
                    }
                }
            }
        }

        public static bool Render(string label, Type type, object value, EntityObserver entity)
        {
            IComponentInspector inspector;
            if (_inspectors.TryGetValue(type, out inspector))
            {
                inspector.OnGUI(label, value, entity.World, entity.Id);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Custom inspector for specified field type.
    /// </summary>
    public interface IComponentInspector
    {
        /// <summary>
        /// Supported field type.
        /// </summary>
        Type GetFieldType();

        /// <summary>
        /// Renders provided instance of specified type.
        /// </summary>
        /// <param name="label">Label of field.</param>
        /// <param name="value">Value of field.</param>
        /// <param name="world">World instance.</param>
        /// <param name="entityId">Entity id.</param>
        void OnGUI(string label, object value, EntitySupervisor world, int entityId);
    }
}
