using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Jtfer.Ecp.Unity.Editor.Inspectors
{
    sealed class QuaternionInspector : IComponentInspector
    {
        Type IComponentInspector.GetFieldType()
        {
            return typeof(Quaternion);
        }

        void IComponentInspector.OnGUI(string label, object value, EntitySupervisor world, int entityId)
        {
            EditorGUILayout.Vector3Field(label, ((Quaternion)value).eulerAngles);
        }
    }
}
