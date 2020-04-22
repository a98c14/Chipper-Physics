using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Chipper.Physics.Editor
{
    public struct EditorUtils
    {
        public static Bounds2D GetBounds(Vector2 pivot, float rotation, List<Vector2> vertices)
        {
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var maxX = float.MinValue;
            var maxY = float.MinValue;

            var rot = MathUtil.GetRotationMatrix(rotation);

            for (int i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                v = math.mul(rot, v);
                v += pivot;
                minX = math.min(v.x, minX);
                minY = math.min(v.y, minY);
                maxX = math.max(v.x, maxX);
                maxY = math.max(v.y, maxY);
            }

            return new Bounds2D
            {
                Min = new float2(minX, minY),
                Max = new float2(maxX, maxY),
            };
        }

        public static Bounds2D GetBounds(Vector2 center, float radius)
        {
            return new Bounds2D
            {
                Min = new float2(center.x - radius, center.y - radius),
                Max = new float2(center.x + radius, center.y + radius),
            };
        }

        public static bool IsConvex(List<Vector2> vertices)
        {
            var count = vertices.Count;
            var state = true;
            for (int i = 0; i < count; i++)
            {
                var v0 = vertices[i];
                var v1 = vertices[(i + 1) % count];
                var v2 = vertices[(i + 2) % count];
                var result = IsLeft(v0, v1, v2);
                if(i == 0)
                    state = result;
                else if(state != result)
                    return false;
            }
            return true;
        }

        public static bool IsLeft(Vector2 a, Vector2 b, Vector2 c)
        {
            return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) > 0;
        }

        public static void DrawBounds(Bounds2D bounds, Color color)
        {
            Handles.color = color;
            Handles.DrawLine((Vector2)bounds.BL, (Vector2)bounds.BR);
            Handles.DrawLine((Vector2)bounds.BR, (Vector2)bounds.TR);
            Handles.DrawLine((Vector2)bounds.TR, (Vector2)bounds.TL);
            Handles.DrawLine((Vector2)bounds.TL, (Vector2)bounds.BL);
        }

        public static void DrawPolygonCollider(
            bool snapToGrid,
            EditorShapeProperty shape,
            EditorShapeColor color,
            List<Vector2> vertices, 
            Vector3[] polygonBuffer, 
            Vector3[] faceBuffer)
        {
            var zPos = shape.Position.z;
            var baseHeight = zPos * Constants.ZConstant;
            var roofHeight = (zPos + shape.Height) * Constants.ZConstant;
            var floorPos = (Vector2)shape.Position;
            var basePos = floorPos + new Vector2(0, baseHeight);
            var roofPos = floorPos + new Vector2(0, roofHeight);

            var offset2 = (Vector2)shape.Offset;
            var floorPivot = floorPos + offset2;
            var roofPivot = roofPos + offset2;
            var basePivot = basePos + offset2;

            DrawFilledPolygon(floorPivot, shape.Rotation, vertices, polygonBuffer, color.Shadow);
            DrawPolygon3DFeatures(floorPivot, baseHeight, roofHeight, shape.Rotation, vertices, faceBuffer, color.Face);
            DrawFilledPolygon(roofPivot, shape.Rotation, vertices, polygonBuffer, color.Roof);
            DrawFilledPolygon(basePivot, shape.Rotation, vertices, polygonBuffer, color.Base);
            DrawVertexHandles(snapToGrid, basePivot, shape.Rotation, vertices, shape.HandleSize, color.Line);
        }

        public static void DrawCircleCollider(
            EditorShapeProperty shape,
            EditorShapeColor color,
            ref Vector2 centerOffset,
            ref float radius)
        {
            var zPos = shape.Position.z;
            var baseHeight = zPos * Constants.ZConstant;
            var roofHeight = (zPos + shape.Height) * Constants.ZConstant;
            var floorPos = (Vector2)shape.Position;
            var basePos = floorPos + new Vector2(0, baseHeight);
            var roofPos = floorPos + new Vector2(0, roofHeight);
            var offset2 = (Vector2)shape.Offset;
            var floorPivot = floorPos + offset2;
            var roofPivot  = roofPos + offset2;
            var basePivot  = basePos + offset2;
            var rot = MathUtil.GetRotationMatrix(shape.Rotation);
            var unrot = math.transpose(rot);
            centerOffset = math.mul(rot, centerOffset);
            var floorCenter = floorPivot + centerOffset;
            var roofCenter = roofPivot + centerOffset;
            var baseCenter = basePivot + centerOffset;

            Handles.color = color.Shadow;
            Handles.DrawSolidDisc(floorCenter, Vector3.forward, radius);
            Handles.color = color.Roof;
            Handles.DrawSolidDisc(roofCenter, Vector3.forward, radius);
            Handles.color = color.Base;
            Handles.DrawWireDisc(baseCenter, Vector3.forward, radius);

            Handles.DrawLine(baseCenter + new Vector2(radius, 0), roofCenter + new Vector2(radius, 0));
            Handles.DrawLine(baseCenter - new Vector2(radius, 0), roofCenter - new Vector2(radius, 0));
            Handles.DrawLine(baseCenter - new Vector2(0, radius), roofCenter - new Vector2(0, radius));
            Handles.DrawLine(baseCenter - new Vector2(radius, radius).normalized * radius, roofCenter - new Vector2(radius, radius).normalized * radius);
            Handles.DrawLine(baseCenter - new Vector2(-radius, radius).normalized * radius, roofCenter - new Vector2(-radius, radius).normalized * radius);

            Handles.DrawDottedLine(floorCenter + new Vector2(radius, 0), baseCenter + new Vector2(radius, 0), 5);
            Handles.DrawDottedLine(floorCenter - new Vector2(radius, 0), baseCenter - new Vector2(radius, 0), 5);
            Handles.DrawDottedLine(floorCenter - new Vector2(0, radius), baseCenter - new Vector2(0, radius), 5);
            
            centerOffset = math.mul(unrot, MoveHandle(centerOffset + floorPivot, shape.HandleSize) - floorPivot);
        }

        public static void DrawFilledPolygon(Vector2 pivot, float rotation, List<Vector2> vertices, Vector3[] buffer, Color handleColor)
        {
            Debug.Assert(vertices.Count == buffer.Length, "Vector3 Buffer is not the same size as vertex list");
            Handles.color = handleColor;
            var rot = MathUtil.GetRotationMatrix(rotation);
            var count = vertices.Count;

            for (int i = 0; i < count; i++)
            {
                var v0 = vertices[i];
                v0 = math.mul(rot, v0);
                v0 += pivot;
                buffer[i] = v0;
            }

            Handles.DrawAAConvexPolygon(buffer);
        }

        public static void DrawPolygon3DFeatures(Vector2 pivot, float baseHeight, float roofHeight, float rotation, List<Vector2> vertices, Vector3[] rectBuffer, Color handleColor)
        {
            Debug.Assert(rectBuffer.Length == 4, "Rect buffer doesn't have 4 elements");
            Handles.color = handleColor;
            var rot = MathUtil.GetRotationMatrix(rotation);
            var count = vertices.Count;
            var roofV = new Vector2(0, baseHeight);
            var baseV = new Vector2(0, roofHeight);
            var shouldDrawFaces = roofHeight - baseHeight > 0;

            for (int i = 0; i < count; i++)
            {
                var v0 = vertices[i];
                var v1 = vertices[(i + 1) % count];
                v0 = math.mul(rot, v0);
                v1 = math.mul(rot, v1);
                var floorV0 = v0 + pivot;
                var floorV1 = v1 + pivot;
                var baseV0  = floorV0 + baseV;
                var baseV1  = floorV1 + baseV;
                var roofV0  = floorV0 + roofV;
                var roofV1  = floorV1 + roofV;
                Handles.DrawDottedLine(floorV0, baseV0, 5f);

                if(shouldDrawFaces)
                {
                    rectBuffer[0] = baseV0;
                    rectBuffer[1] = baseV1;
                    rectBuffer[2] = roofV1;
                    rectBuffer[3] = roofV0;
                    Handles.DrawAAConvexPolygon(rectBuffer);
                }
            }
        }

        public static void DrawVertexHandles(bool snapToGrid, Vector2 pivot, float rotation, List<Vector2> vertices, float handleSize, Color color)
        {
            Handles.color = color;
            var count  = vertices.Count;
            var rot    = MathUtil.GetRotationMatrix(rotation);
            var unrot  = math.transpose(rot);

            for (int i = 0; i < count; i++)
            {
                var v0 = vertices[i];
                var v1 = vertices[(i + 1) % count];
                v0 = math.mul(rot, v0);
                v1 = math.mul(rot, v1);
                v0 += pivot;
                v1 += pivot;
                Handles.DrawLine(v0, v1);
                EditorGUI.BeginChangeCheck();
                v0 = MoveHandle(v0, handleSize);
                if (EditorGUI.EndChangeCheck())
                    vertices[i] = snapToGrid ? math.round(math.mul(unrot, v0 - pivot)) : math.mul(unrot, v0 - pivot);                
            }
        }

        public static void DrawPivot(ref Vector2 position, ref Vector2 pivotOffset, float handleSize, Color positionColor, Color pivotColor)
        {
            var pivot = position + pivotOffset;
            Handles.color = positionColor;
            position = MoveHandle(position, handleSize);
            Handles.color = pivotColor;
            pivotOffset = MoveHandle(pivot, handleSize) - position;
        }

        public static void DrawCone()
        {
            // TODO:
        }

        public static void DrawCircle(ref Vector2 position, ref Vector2 pivotOffset, ref Vector2 centerOffset, ref float radius, float handleSize, Color handleColor)
        {
            Handles.color = handleColor;
            var pivot = position + pivotOffset;
            var center = centerOffset + pivot;
            Handles.DrawWireDisc(center, Vector3.forward, radius);

            EditorGUI.BeginChangeCheck();
            var radiusHandle = center + new Vector2(radius, 0);
            radiusHandle = MoveHandle(radiusHandle, handleSize) - center;
            if (EditorGUI.EndChangeCheck())
                radius = radiusHandle.x;

            centerOffset = MoveHandle(center, handleSize) - pivot;
            pivotOffset = MoveHandle(pivot, handleSize) - position;
        }

        public static float FixHandleSize(float handleSize)
        {
            return HandleUtility.GetHandleSize(SceneView.lastActiveSceneView.camera.transform.position) * .1f * handleSize;
        }

        public static Vector2 MoveHandle(Vector2 v, float handleSize)
        {
            return Handles.FreeMoveHandle(v, Quaternion.identity, handleSize, Vector3.zero, Handles.DotHandleCap);
        }
    }
}
