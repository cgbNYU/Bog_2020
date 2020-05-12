using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Using the transforms of each Terrain Chunk object to set the low x and low z bound
/// 4 Quads per Unity Unit
/// 
/// </summary>
public class TerrainChunk : MonoBehaviour
{
    static int _chunkSize = 16;
    private int _terrainUnitDivisionsPerUnityUnit = 2;
    private float _noiseOctaveScale0 = 100;
    private Mesh _myTerrainChunkMesh;
    Vector3[,] _postions = new Vector3[_chunkSize + 1, _chunkSize + 1];
    Vector3[] _verts = new Vector3[(_chunkSize + 1) * (_chunkSize + 1)];
    int[] _triangles = new int[_chunkSize * _chunkSize * 2 * 3];
    public float heightScale;
    // Start is called before the first frame update
    
    
    void Start()
    {   
        _myTerrainChunkMesh = new Mesh();
        //get all the vertices
        for (int i = 0; i <= _chunkSize; i++)
        {
            for (int j = 0; j <= _chunkSize; j++)
            {
                float xPos = transform.position.x + (float)i / _terrainUnitDivisionsPerUnityUnit;
                float zPos = transform.position.z + (float) j / _terrainUnitDivisionsPerUnityUnit;
                float yPos = Perlin.Noise(xPos / _noiseOctaveScale0, zPos / _noiseOctaveScale0);
                _verts[j * (_chunkSize + 1) + i] = new Vector3(xPos, yPos, zPos) * heightScale;
            }
        }
        //Make all the tris
        for (int i = 0; i < _chunkSize; i++)
        {
            for (int j = 0; j < _chunkSize; j++)
            {
                int index = (j * _chunkSize + i) * 6;
                _triangles[index] = GetIndex(i, j);
                _triangles[index + 1] = GetIndex(i, j + 1);
                _triangles[index + 2] = GetIndex(i + 1, j + 1);
                _triangles[index + 3] = GetIndex(i, j);
                _triangles[index + 4] = GetIndex(i + 1, j + 1);
                _triangles[index + 5] = GetIndex(i + 1, j);
            }
        }
        _myTerrainChunkMesh.vertices = _verts;
        _myTerrainChunkMesh.triangles = _triangles;
        _myTerrainChunkMesh.RecalculateNormals();
        
        MeshFilter myMF = gameObject.AddComponent<MeshFilter>();
        myMF.mesh = _myTerrainChunkMesh;
        MeshRenderer myMR = gameObject.AddComponent<MeshRenderer>();
        myMR.material = new Material(Shader.Find("Standard"));
        
        
    }
    int GetIndex(int i, int j)
    {
        return j * (_chunkSize + 1) + i;
    }
}


//public class TerrainChunk : MonoBehaviour
//{ //usually jsons for more important data and lets moders mod your game.
//    
//    static int chunkSize = 16;
//    private int terrainUnitDivisionsPerUnityUnit = 2;
//    private float noiseOctaveScale0 = 100;
//
//    private Mesh myTerrainChunkMesh;
//
//    Vector3[,] positions = new Vector3[chunkSize + 1, chunkSize + 1];    //rep in a 2D array, all the positions that we want. 
//    Vector3[] verts = new Vector3[(chunkSize + 1) * (chunkSize+1)];
//    int[] triangles = new int[chunkSize  * chunkSize *2 * 3]; 
//    //*2 b/c tris from quads
//    //*3 b/c three indices per tri 
// 
//        void Start()
//    {
//        myTerrainChunkMesh = new Mesh();
//
//        for (int i = 0; i <= chunkSize; i++)
//        {
//            for (int j = 0; j <= chunkSize; j++)
//            {
//                float xPos = transform.position.x + (float)i / terrainUnitDivisionsPerUnityUnit;
//                float zPos = transform.position.x + (float)j / terrainUnitDivisionsPerUnityUnit;
//
//                float yPos = Perlin.Noise(xPos / noiseOctaveScale0, zPos / noiseOctaveScale0);
//                
//                verts[j * (chunkSize + 1) + i] = new Vector3(xPos, yPos, zPos);
//            }
//        }
//
//        for (int i = 0; i < chunkSize; i++)
//        {
//            for (int j = 0; j < chunkSize; j++)
//            {
//                int index = (j * (chunkSize) + i) * 6; //increment our tris  by 6  indices with each iteration. 
//
//                triangles[index] = j * (chunkSize + 1) + i;
//                triangles[index + 1] = GimmeIndex(i, j + 1);
//                triangles[index + 2] = GimmeIndex(i + 1, j + 1);
//
//                triangles[index + 3] = GimmeIndex(i,j);
//                triangles[index + 3] = GimmeIndex(i + 1,j + 1);
//                triangles[index + 3] = GimmeIndex(i+ 1,j);
//            }
//        }
//
//        myTerrainChunkMesh.vertices = verts;
//        myTerrainChunkMesh.triangles = triangles;
//
//        MeshFilter myMF = gameObject.AddComponent<MeshFilter>();
//        myMF.mesh = myTerrainChunkMesh;
//
//        MeshRenderer myMR = gameObject.AddComponent<MeshRenderer>();
//        
//        
//    }
//
//        int GimmeIndex(int i, int j)
//        {
//            return j * (chunkSize + 1) + i; 
//        }
//    
//}
