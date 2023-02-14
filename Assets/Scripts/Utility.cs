using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility{

    public static class Vectors
    {
        public static Vector3 FindClosestEdge(Renderer target, Transform player)
        {
            Vector3[] edges = {
                // Forward Edge
                new Vector3(
                        target.transform.position.x,
                        target.bounds.extents.y * 0.9625f + target.transform.position.y,
                        target.bounds.extents.z * 0.9625f + target.transform.position.z
                ),
                // Backward Edge
                new Vector3(
                        target.transform.position.x,
                        target.bounds.extents.y * 0.9625f + target.transform.position.y,
                        -(target.bounds.extents.z * 0.9625f) + target.transform.position.z
                ),
                // Right Edge 
                new Vector3(
                        target.bounds.extents.x * 0.9625f + target.transform.position.x,
                        target.bounds.extents.y * 0.9625f + target.transform.position.y,
                        target.transform.position.z
                ),
                // Right Edge 
                new Vector3(
                        -(target.bounds.extents.x * 0.9625f) + target.transform.position.x,
                        target.bounds.extents.y * 0.9625f + target.transform.position.y,
                        target.transform.position.z
                )
            };

            float maxDistance = Mathf.Infinity;
            float temp = 0;
            Vector3 closestEdge = Vector3.zero;
            foreach(Vector3 edge in edges)
            {
                temp = Vector3.Distance(edge, player.position);
                if(temp < maxDistance) {
                    maxDistance = temp;
                    closestEdge = edge;
                }
            }
            return closestEdge;
        }

        public static Vector3 FindClosestEdge(Transform target, Transform player)
        {
            Vector3[] edges = {
                // Forward Edge
                new Vector3(
                        target.position.x,
                        target.localScale.y * 0.9625f + target.position.y,
                        target.localScale.z * 0.9625f + target.position.z
                ),
                // Backward Edge
                new Vector3(
                        target.position.x,
                        target.localScale.y * 0.9625f + target.position.y,
                        -(target.localScale.z * 0.9625f) + target.position.z
                ),
                // Right Edge 
                new Vector3(
                        target.localScale.x * 0.9625f + target.position.x,
                        target.localScale.y * 0.9625f + target.position.y,
                        target.position.z
                ),
                // Right Edge 
                new Vector3(
                        -(target.localScale.x * 0.9625f) + target.position.x,
                        target.localScale.y * 0.9625f + target.position.y,
                        target.position.z
                )
            };

            float maxDistance = Mathf.Infinity;
            float temp = 0;
            Vector3 closestEdge = Vector3.zero;
            foreach(Vector3 edge in edges)
            {
                temp = Vector3.Distance(edge, player.position);
                if(temp < maxDistance) {
                    maxDistance = temp;
                    closestEdge = edge;
                }
            }
            return closestEdge;
        }

        public static Vector3 MultiplyV3(Vector3 a, Vector3 b, bool ignoreY)
        {
            return new Vector3(a.x*b.x, ignoreY ? a.y : (a.y*b.y), a.z*b.z);
        }
        public static Vector3 MultiplyV3(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x*b.x, a.y*b.y, a.z*b.z);
        }
        public static Vector3 InvertV3(Vector3 a)
        {
            Vector3 b = FloorV3(AbsV3(a));
            return new Vector3(b.x==0 ? 1 : 0, b.y==0 ? 1 : 0, b.z==0 ? 1 : 0);
        }
        public static Vector3 AbsV3(Vector3 a)
        {
            return new Vector3(Mathf.Abs(a.x), Mathf.Abs(a.y), Mathf.Abs(a.z));
        }
        public static Vector3 FloorV3(Vector3 a)
        {
            return new Vector3(Mathf.Floor(a.x), Mathf.Floor(a.y), Mathf.Floor(a.z));
        }
        public static Vector3 RoundV3(Vector3 a)
        {
            return new Vector3((int)a.x, (int)a.y, (int)a.z);
        }
    }

    public static class Debug
    {
        public static Vector3[] PlaceObjectOnCorner(Transform targetObject, GameObject objectToPlace)
        {
            Vector3 temp = new Vector3(
                    targetObject.localScale.x * 0.5f + targetObject.position.x,
                    targetObject.localScale.y * 0.5f + targetObject.position.y,
                    targetObject.localScale.z * 0.5f + targetObject.position.z
            );
            objectToPlace.transform.position = temp;
            return new Vector3[] {  targetObject.position,
                                    targetObject.localScale,
                                    temp
            };
        }
    }
}