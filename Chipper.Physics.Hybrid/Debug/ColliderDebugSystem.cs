using Unity.Entities;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using Chipper.Transforms;

namespace Chipper.Physics
{
    public struct PhysicsDebugSettings : IComponentData
    {
        public bool IsEnabled;
        public bool DrawBounds;
        public bool DrawNormals;
        public bool DrawInfoLabels;
        public bool DrawGrid;
        public float2 LabelOffset;
        public float  LabelGap;
    }

    class PhysicsDebugDrawer : MonoBehaviour
    {
        internal interface IDrawable
        {
            void Draw();
        }

        public struct DebugEdge : IDrawable
        {
            public readonly Vector2 Start;
            public readonly Vector2 End;

            public DebugEdge(Vector2 start, Vector2 end) => (Start, End) = (start, end);

            public void Draw() =>
                Handles.DrawLine(Start, End);
        }

        public struct DebugCurve : IDrawable
        {
            public float Angle;
            public float Radius;
            public Vector2 Center;
            public Vector2 From;

            public void Draw() =>
                Handles.DrawWireArc(Center, Vector3.forward, From, Angle, Radius);
        }

        public struct DebugLabel : IDrawable
        {
            public GUIStyle Style;
            public float3 Position;
            public string Text;

            public DebugLabel(Vector3 position, string text, Color color)
            {
                Style = new GUIStyle();
                Style.normal.textColor = color;
                Position = position;
                Text = text;
            }

            public void Draw() => Handles.Label(Position, Text, Style);
        }

        public float NormalDrawLength = 2f;

        readonly Dictionary<Color, List<IDrawable>> m_Pools = new Dictionary<Color, List<IDrawable>>();
        
        public void OnDrawGizmos()
        {
            foreach(var kv in m_Pools)
            {
                var color = kv.Key;
                var drawables = kv.Value;

                Handles.color = color;
                for (int i = 0; i < drawables.Count; i++)
                    drawables[i].Draw();

                drawables.Clear();
            }
        }

        public void Add(Vector3 position, string text, Color color)
        {
            CreatePoolIfDoesntExist(color);

            m_Pools[color].Add(new DebugLabel(position, text, color));
        }

        public void Add(Vector2 a, Vector2 b, Color color)
        {
            CreatePoolIfDoesntExist(color);

            m_Pools[color].Add(new DebugEdge(a, b));
        }

        public void Add(Bounds2D bounds, Color color)
        {
            CreatePoolIfDoesntExist(color);

            m_Pools[color].Add(new DebugEdge(bounds.BL, bounds.BR));
            m_Pools[color].Add(new DebugEdge(bounds.BR, bounds.TR));
            m_Pools[color].Add(new DebugEdge(bounds.TR, bounds.TL));
            m_Pools[color].Add(new DebugEdge(bounds.TL, bounds.BL));
        }

        public void Add(NativeArray<ColliderVertex> vertices, NativeArray<EdgeNormal> normals, Color colliderColor, Color normalColor)
        {
            CreatePoolIfDoesntExist(colliderColor);
            CreatePoolIfDoesntExist(normalColor);

            for (int i = 0; i < vertices.Length; i++)
            {
                var start = vertices[i].Value;
                var end = vertices[(i + 1) % vertices.Length].Value;

                m_Pools[colliderColor].Add(new DebugEdge(start, end));

                if (i < normals.Length)
                {
                    var normalStart = (start + end) / 2;
                    var normalEnd = normalStart + normals[i].Value * NormalDrawLength;

                    m_Pools[normalColor].Add(new DebugEdge(normalStart, normalEnd));
                }
            }
        }

        public void Add(NativeArray<ColliderVertex> vertices, Color colliderColor)
        {
            CreatePoolIfDoesntExist(colliderColor);

            for (int i = 0; i < vertices.Length; i++)
            {
                var start = vertices[i].Value;
                var end = vertices[(i + 1) % vertices.Length].Value;

                m_Pools[colliderColor].Add(new DebugEdge(start, end));
            }
        }

        public void Add(CircleCollider collider, Color color) 
        {
            CreatePoolIfDoesntExist(color);

            m_Pools[color].Add(new DebugCurve
            {
                Angle = 360,
                Center = collider.Center,
                From = collider.Center + new float2(collider.Radius, 0),
                Radius = collider.Radius,
            });
        }

        void CreatePoolIfDoesntExist(Color c)
        {
            if (!m_Pools.ContainsKey(c))
                m_Pools[c] = new List<IDrawable>();
        }
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ColliderDebugSystem : ComponentSystem
    {
        EntityQuery        m_ColliderGroup;
        PhysicsDebugDrawer m_Drawer;

        readonly Color m_BoundsColor       = new Color(255 / 255f, 255 / 255f, 255 / 255f, 255 / 255f);
        readonly Color m_ColliderColor     = new Color(168 / 255f, 245 / 255f,  81 / 255f, 255 / 255f);
        readonly Color m_NormalColor       = new Color(182 / 255f,  81 / 255f, 245 / 255f, 255 / 255f);
        readonly Color m_VelocityColor     = new Color(255 / 255f, 126 / 255f,   5 / 255f, 255 / 255f);
        readonly Color m_AccelerationColor = new Color(255 / 255f, 218 / 255f,   5 / 255f, 255 / 255f);
        readonly Color m_LabelColor        = new Color(  0 / 255f,   0 / 255f, 255 / 255f, 255 / 255f);
        readonly Color m_GridColor         = new Color(163 / 255f, 162 / 255f, 158 / 255f, 255 / 255f);

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<PhysicsDebugSettings>();
            m_ColliderGroup = GetEntityQuery(new EntityQueryDesc
            {
                Any = new ComponentType[]
                {
                    ComponentType.ReadOnly(typeof(Bounds2D)),
                    ComponentType.ReadOnly(typeof(CircleCollider)),
                    ComponentType.ReadOnly(typeof(ColliderVertex)),
                    ComponentType.ReadOnly(typeof(Velocity)),
                    ComponentType.ReadOnly(typeof(Acceleration)),
                }
            });
        }

        protected override void OnUpdate()
        {
            var settings = GetSingleton<PhysicsDebugSettings>();

            if(!settings.IsEnabled)
                return;

            if(m_Drawer == null)
                m_Drawer = CreateDrawer();

            var chunks             = m_ColliderGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            var boundsType         = GetComponentTypeHandle<Bounds2D>(true);
            var circleType         = GetComponentTypeHandle<CircleCollider>(true);
            var positionType       = GetComponentTypeHandle<Position2D>(true);
            var velocityType       = GetComponentTypeHandle<Velocity>(true);
            var accelerationType   = GetComponentTypeHandle<Acceleration>(true);
            var colliderVertexType = GetBufferTypeHandle<ColliderVertex>(true);
            var colliderNormalType = GetBufferTypeHandle<EdgeNormal>(true);

            for (int i = 0; i < chunks.Length; i++)
            {
                var chunk = chunks[i];
                var count = chunk.Count;

                if (settings.DrawGrid)
                    DrawGrid();

                if (settings.DrawBounds && chunk.Has(boundsType))
                    DrawBounds(chunk.GetNativeArray(boundsType));

                if (chunk.Has(circleType))
                    DrawColliders(chunk.GetNativeArray(circleType));
            
                if (chunk.Has(colliderVertexType))
                {
                    var colliders = chunk.GetBufferAccessor(colliderVertexType);
                    var normals   = chunk.GetBufferAccessor(colliderNormalType);

                    if (settings.DrawNormals)
                    {
                        for (int j = 0; j < count; j++)
                            m_Drawer.Add(colliders[j].AsNativeArray(), normals[j].AsNativeArray(), m_ColliderColor, m_NormalColor);
                    }
                    else
                    {
                        for (int j = 0; j < count; j++)
                            m_Drawer.Add(colliders[j].AsNativeArray(), m_ColliderColor);
                    }
                }

                if (settings.DrawInfoLabels && chunk.Has(velocityType) && chunk.Has(accelerationType) && chunk.Has(positionType))
                {
                    var positions = chunk.GetNativeArray(positionType);
                    var accelerations = chunk.GetNativeArray(accelerationType);
                    var velocities = chunk.GetNativeArray(velocityType);

                    for(int j = 0; j < count; j++)
                    {
                        var pos = positions[j].Value.xy;
                        var a   = accelerations[j].Value.xy;
                        var v   = velocities[j].Value.xy;
                        var x   = pos.x + settings.LabelOffset.x;
                        var y   = pos.y + settings.LabelOffset.y;


                        m_Drawer.Add(new float3(x, y, 0), $"Vel.  : {math.length(v):0.##}", m_LabelColor);
                        m_Drawer.Add(new float3(x, y + settings.LabelGap, 0), $"Accel.: {math.length(a):0.##}", m_LabelColor);
                        m_Drawer.Add(pos, pos + v, m_VelocityColor);
                        m_Drawer.Add(pos, pos + a, m_AccelerationColor);
                    }
                }
            }
            chunks.Dispose();
        }

        void DrawBounds(NativeArray<Bounds2D> bounds)
        {
            for (int i = 0; i < bounds.Length; i++)
                m_Drawer.Add(bounds[i], m_BoundsColor);
        }

        void DrawColliders(NativeArray<CircleCollider> colliders)
        {
            for (int i = 0; i < colliders.Length; i++)
                m_Drawer.Add(colliders[i], m_ColliderColor);
        }

        void DrawGrid()
        {
            var scene = SceneView.lastActiveSceneView;
            
            if (scene != null)
            {
                var camera = scene.camera;
                var bl = (float3)camera.ViewportToWorldPoint(Vector3.zero);
                var tr = (float3)camera.ViewportToWorldPoint(Vector3.one);

                var cellSize = Constants.CellSize;
                bl = bl - cellSize - (bl % cellSize);
                tr = tr + cellSize + (tr % cellSize);

                for(var x = bl.x; x <= tr.x; x += cellSize)
                    m_Drawer.Add(new Vector2(x, bl.y), new Vector2(x, tr.y), m_GridColor);

                for (var y = bl.y; y <= tr.y; y += cellSize)
                    m_Drawer.Add(new Vector2(bl.x, y), new Vector2(tr.x, y), m_GridColor);
            }
        }

        protected override void OnDestroy()
        {
            if (m_Drawer != null)
                GameObject.DestroyImmediate(m_Drawer.gameObject);
        }

        PhysicsDebugDrawer CreateDrawer()
        {
            var obj = new GameObject("DebugDrawer");
            obj.hideFlags = HideFlags.NotEditable;
            return obj.AddComponent<PhysicsDebugDrawer>();
        }
    }
}
