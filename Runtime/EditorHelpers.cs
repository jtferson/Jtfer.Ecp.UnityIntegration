#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jtfer.Ecp.Unity
{
    public static class EditorHelpers
    {
        public static string GetCleanGenericTypeName(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.Name;
            }
            var constraints = "";
            foreach (var constr in type.GetGenericArguments())
            {
                constraints += constraints.Length > 0 ? string.Format(", {0}", GetCleanGenericTypeName(constr)) : constr.Name;
            }
            return string.Format("{0}<{1}>", type.Name.Substring(0, type.Name.LastIndexOf("`")), constraints);
        }
    }

    public sealed class EntityObserver : MonoBehaviour
    {
        public int Id;
        public EntitySupervisor World;
    }

    public sealed class PipelineObserver : MonoBehaviour, IPipelineDebugListener
    {
        Pipeline _pipeline;

        public static GameObject Create(Pipeline pipeline)
        {
            if (pipeline == null)
            {
                throw new ArgumentNullException("systems");
            }
            var go = new GameObject(string.Format("[{0}]", pipeline.Name ?? "[ECP-PIPELINE]"));
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.NotEditable;
            var observer = go.AddComponent<PipelineObserver>();
            observer._pipeline = pipeline;
            pipeline.AddDebugListener(observer);
            return go;
        }

        public Pipeline GetSystems()
        {
            return _pipeline;
        }

        void OnDestroy()
        {
            if (_pipeline != null)
            {
                _pipeline.RemoveDebugListener(this);
                _pipeline = null;
            }
        }

        void IPipelineDebugListener.OnSystemsDestroyed()
        {
            // for immediate unregistering this MonoBehaviour from ECS.
            OnDestroy();
            // for delayed destroying GameObject.
            Destroy(gameObject);
        }
    }

    public sealed class SupervisorObserver : MonoBehaviour, ISupervisorDebugListener
    {
        EntitySupervisor _world;
        readonly Dictionary<int, GameObject> _entities = new Dictionary<int, GameObject>(1024);
        static object[] _componentsCache = new object[32];

        public static GameObject Create(EntitySupervisor world, string name = null)
        {
            if (world == null)
            {
                throw new ArgumentNullException("world");
            }
            var go = new GameObject(name != null ? string.Format("[ECS-WORLD {0}]", name) : "[ECS-WORLD]");
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.NotEditable;
            var observer = go.AddComponent<SupervisorObserver>();
            observer._world = world;
            world.AddDebugListener(observer);
            return go;
        }

        public EcsWorldStats GetStats()
        {
            return _world.GetStats();
        }

        void ISupervisorDebugListener.OnEntityCreated(int entity)
        {
            GameObject go;
            if (!_entities.TryGetValue(entity, out go))
            {
                go = new GameObject();
                go.transform.SetParent(transform, false);
                go.hideFlags = HideFlags.NotEditable;
                var unityEntity = go.AddComponent<EntityObserver>();
                unityEntity.World = _world;
                unityEntity.Id = entity;
                _entities[entity] = go;
                UpdateEntityName(entity, false);
            }
            go.SetActive(true);
        }

        void ISupervisorDebugListener.OnEntityRemoved(int entity)
        {
            GameObject go;
            if (!_entities.TryGetValue(entity, out go))
            {
                throw new Exception("Unity visualization not exists, looks like a bug");
            }
            UpdateEntityName(entity, false);
            go.SetActive(false);
        }

        void ISupervisorDebugListener.OnComponentAdded(int entity, object component)
        {
            UpdateEntityName(entity, true);
        }

        void ISupervisorDebugListener.OnComponentRemoved(int entity, object component)
        {
            UpdateEntityName(entity, true);
        }

        void ISupervisorDebugListener.OnWorldDestroyed(EntitySupervisor world)
        {
            // for immediate unregistering this MonoBehaviour from ECS.
            OnDestroy();
            // for delayed destroying GameObject.
            Destroy(gameObject);
        }

        void UpdateEntityName(int entity, bool requestComponents)
        {
            var entityName = entity.ToString("D8");
            if (requestComponents)
            {
                var count = _world.GetComponents(entity, ref _componentsCache);
                for (var i = 0; i < count; i++)
                {
                    entityName = string.Format("{0}:{1}", entityName, EditorHelpers.GetCleanGenericTypeName(_componentsCache[i].GetType()));
                    _componentsCache[i] = null;
                }
            }
            _entities[entity].name = entityName;
        }

        void OnDestroy()
        {
            if (_world != null)
            {
                _world.RemoveDebugListener(this);
                _world = null;
            }
        }
    }
}

#endif
