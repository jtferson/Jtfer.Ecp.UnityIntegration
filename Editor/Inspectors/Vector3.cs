using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Jtfer.Ecp.Unity.Editor.Inspectors
{
    sealed class StringInspector : IComponentInspector
    {
        Type IComponentInspector.GetFieldType()
        {
            return typeof(Vector3);
        }

        void IComponentInspector.OnGUI(string label, object value, EntitySupervisor world, int entityId)
        {
            EditorGUILayout.Vector3Field(label, (Vector3)value);
        }
    }
}
