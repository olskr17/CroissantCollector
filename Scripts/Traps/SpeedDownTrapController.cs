using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedDownTrapController : MonoBehaviour {

    public List<Mesh> listSPtrapMesh = new List<Mesh>();

    private MeshFilter meshFilter;
    private Mesh originalMesh;
    private int objectCount = -1;
	// Use this for initialization

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        originalMesh = meshFilter.mesh;
    }
  
    void OnEnable()
    {
        meshFilter.mesh = originalMesh;
        StartCoroutine(Change());
    }

  
    IEnumerator Change()
    {
        while (true)
        {
            meshFilter.mesh = PickAMesh();
            yield return new WaitForSeconds(0.05f);
        }
    }

    Mesh PickAMesh()
    {
        objectCount++;
        if (objectCount > listSPtrapMesh.Count - 1)
            objectCount = 0;
        return listSPtrapMesh[objectCount];
    }
}
