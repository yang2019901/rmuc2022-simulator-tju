using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


#if UNITY_EDITOR
public class MyMenuTools {
    [MenuItem("MyTools/CountObjsMeshes _F3")]
    static void LogMeshF() {
        List<MeshFilter> mesh = new List<MeshFilter>();
        GameObject[] gameObjects = Selection.gameObjects;
        for (int i = 0; i < gameObjects.Length; i++) {
            MeshFilter[] meshFilters = gameObjects[i].GetComponentsInChildren<MeshFilter>(includeInactive: true);
            for (int j = 0; j < meshFilters.Length; j++) {
                if (!mesh.Contains(meshFilters[j])) {
                    mesh.Add(meshFilters[j]);
                }
            }
        }

        int 顶点数 = 0;
        int 三角面数 = 0;
        for (int i = 0; i < mesh.Count; i++) {
            顶点数 += mesh[i].sharedMesh.vertexCount;
            三角面数 += mesh[i].sharedMesh.triangles.Length / 3;

        }
        Debug.Log("所有选中物体：顶点数:" + 顶点数 + "_三角面数:" + 三角面数);
    }

    [MenuItem("MyTools/CountObjsMeshes(inactive excluded) _F4")]
    static void LogActivMeshF() {
        List<MeshFilter> mesh = new List<MeshFilter>();
        GameObject[] gameObjects = Selection.gameObjects;
        for (int i = 0; i < gameObjects.Length; i++) {
            MeshFilter[] meshFilters = gameObjects[i].GetComponentsInChildren<MeshFilter>(includeInactive: false);
            for (int j = 0; j < meshFilters.Length; j++) {
                if (!mesh.Contains(meshFilters[j])) {
                    mesh.Add(meshFilters[j]);
                }
            }
        }

        int verts = 0;
        int tris = 0;
        for (int i = 0; i < mesh.Count; i++) {
            verts += mesh[i].sharedMesh.vertexCount;
            tris += mesh[i].sharedMesh.triangles.Length / 3;
        }
        Debug.Log("所有Active物体：顶点数:" + verts + "_三角面数:" + tris);
    }
}
#endif