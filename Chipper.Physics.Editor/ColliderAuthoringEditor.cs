using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using System;

namespace Chipper.Physics.Editor
{
    [CustomEditor(typeof(ColliderAuthoring))]
    public class ColliderAuthoringEditor : UnityEditor.Editor
    {
        public GUIStyle HeaderStyle;

        public float HandleSize       = .5f;
        public Color ValidRoofColor   = new Color(138 / 255f, 186 / 255f, 37 / 255f, .6f);
        public Color ValidBaseColor   = new Color(138 / 255f, 186 / 255f, 37 / 255f, .3f);
        public Color ValidLineColor   = new Color(21 / 255f,  51 / 255f, 0 / 255f, 1f);
        public Color InvalidRoofColor = new Color(235 / 255f, 64 / 255f, 52 / 255f, 1f);
        public Color InvalidBaseColor = new Color(235 / 255f, 64 / 255f, 52 / 255f, .3f);
        public Color InvalidLineColor = new Color(235 / 255f, 64 / 255f, 52 / 255f, 1f);
        public Color PivotColor       = new Color(255 / 255f, 179 / 255f, 0/ 255f,  1f);
        public Color PositionColor    = new Color(255 / 255f, 242 / 255f, 64/ 255f, 1f);
        public Color ValidFaceColor   = new Color(111 / 255f, 150 / 255f, 44 / 255f, .4f);
        public Color InvalidFaceColor = new Color(176 / 255f, 50 / 255f, 0 / 255f, .4f);
        public Color ShadowColor      = new Color(13 / 255f, 13 / 255f, 12 / 255f, .4f);
        public Color ValidBoundsColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, .5f);
        public Color InvalidBoundsColor = new Color(235 / 255f, 64 / 255f, 52 / 255f, 1f);

        public Vector3[] ConvexPolygonBuffer = new Vector3[0];
        public Vector3[] FaceBuffer          = new Vector3[4];
        public bool      SnapToGrid          = false;

        public void OnEnable()
        {
            var collider = (ColliderAuthoring)target;
            if (collider.Vertices == null)
                collider.Vertices = new List<Vector2>();

            Array.Resize(ref ConvexPolygonBuffer, collider.Vertices.Count);
        }

        public override void OnInspectorGUI()
        {
            HeaderStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

            var collider = (ColliderAuthoring)target;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Collider Settings", HeaderStyle);
            EditorGUI.BeginChangeCheck();
            collider.Shape  = (ColliderShapeType)EditorGUILayout.EnumPopup("Collider Shape:", collider.Shape);
            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();

            collider.ColliderTags = (ColliderTagType)EditorGUILayout.EnumFlagsField("Collider Tags:", collider.ColliderTags);
            collider.CollidesWith = (ColliderTagType)EditorGUILayout.EnumFlagsField("Collides With:", collider.CollidesWith);
            collider.CollisionResolution = (CollisionResolutionType)EditorGUILayout.EnumPopup("Collision Resolution:", collider.CollisionResolution);

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Shape Settings", HeaderStyle);
            switch (collider.Shape)
            {
                case ColliderShapeType.Polygon:
                    DrawPolygonInspector(collider);
                    break;
                case ColliderShapeType.Circle:
                    DrawCircleInspector(collider);
                    break;
            }
            EditorGUILayout.EndVertical();
        }

        void DrawPolygonInspector(ColliderAuthoring collider)
        {
            EditorGUI.BeginChangeCheck();
            SnapToGrid = EditorGUILayout.Toggle("Snap to Grid?", SnapToGrid);
            collider.PivotOffset = EditorGUILayout.Vector3Field("Pivot", collider.PivotOffset);
            collider.Height = EditorGUILayout.FloatField("Height:", collider.Height);
            if (EditorGUI.EndChangeCheck())
            {
                collider.Height = math.max(0, collider.Height);
                SceneView.RepaintAll();
            }

            var vertexCount = collider.Vertices.Count;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Add/Remove Vertex");
            if (GUILayout.Button("+"))
            {
                collider.Vertices.Add(new Vector2(0, 1));
                Array.Resize(ref ConvexPolygonBuffer, collider.Vertices.Count);
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("-") && vertexCount > 0)
            {
                collider.Vertices.RemoveAt(vertexCount - 1);
                Array.Resize(ref ConvexPolygonBuffer, collider.Vertices.Count);
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Vertices", EditorStyles.boldLabel);
            for(int i = 0; i < collider.Vertices.Count; i++)
            {
                var vertex = collider.Vertices[i];
                EditorGUILayout.LabelField($"Vertex {i}: ({vertex.x}, {vertex.y})", EditorStyles.miniLabel);
            }
            if (GUILayout.Button("Round to Int"))
            {
                for(int i = 0; i < collider.Vertices.Count; i++)
                {
                    collider.Vertices[i] = math.round(collider.Vertices[i]);
                }
            }
        }

        void DrawCircleInspector(ColliderAuthoring collider)
        {
            EditorGUI.BeginChangeCheck();
            collider.Height = EditorGUILayout.FloatField("Height:", collider.Height);
            collider.Radius = EditorGUILayout.FloatField("Radius:", collider.Radius);
            if (EditorGUI.EndChangeCheck())
            {
                collider.Height = math.max(0, collider.Height);
                collider.Radius = math.max(0, collider.Radius);
                SceneView.RepaintAll();
            }
        }

        public void OnSceneGUI()
        {
            var collider   = (ColliderAuthoring)target;
            var transform  = collider.GetComponent<Transform>();
            var rotation   = transform.eulerAngles.z;
            var offset2    = (Vector2)collider.PivotOffset;
            var position2  = (Vector2)transform.position;
            var isConvex   = EditorUtils.IsConvex(collider.Vertices);
            var handleSize = EditorUtils.FixHandleSize(HandleSize);
            EditorUtils.DrawPivot(ref position2, ref offset2, handleSize, PositionColor, PivotColor);

            var shape = new EditorShapeProperty
            {
                Height = collider.Height,
                Offset = collider.PivotOffset,
                Position = transform.position,
                Rotation = rotation,
                HandleSize = handleSize,
            };
            var color = new EditorShapeColor
            {
                Line = isConvex ? ValidLineColor : InvalidLineColor,
                Base = isConvex ? ValidBaseColor : InvalidBaseColor,
                Roof = isConvex ? ValidRoofColor : InvalidRoofColor,
                Face = isConvex ? ValidFaceColor : InvalidFaceColor,
                Shadow = ShadowColor,
            };

            var isBiggerThanCellSize = false;
            switch (collider.Shape)
            {
                case ColliderShapeType.Polygon:
                    var bounds = EditorUtils.GetBounds(offset2 + position2, rotation, collider.Vertices);
                    isBiggerThanCellSize = bounds.IsBiggerThanCellSize;
                    var boundsColor = isBiggerThanCellSize ? InvalidBoundsColor : ValidBoundsColor;
                    EditorUtils.DrawBounds(bounds, boundsColor);
                    EditorUtils.DrawPolygonCollider(SnapToGrid, shape, color, collider.Vertices, ConvexPolygonBuffer, FaceBuffer);
                    break;
                case ColliderShapeType.Circle:
                    EditorUtils.DrawCircleCollider(shape, color, ref collider.CenterOffset, ref collider.Radius);
                    break;
                case ColliderShapeType.Cone:
                    break;
            }

            collider.IsBiggerThanCellSize = isBiggerThanCellSize;
            collider.PivotOffset = new Vector3(offset2.x, offset2.y, transform.position.z);
            transform.position = new Vector3(position2.x, position2.y, transform.position.z);
        }
    }
}
