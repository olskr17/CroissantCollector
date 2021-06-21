using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastGroundController : MonoBehaviour {

    public Mesh highLightMesh;

    private MeshFilter meshFilter;
    private Mesh originalMesh;
    // Use this for initialization

    void Start () {
        meshFilter = GetComponent<MeshFilter>();
        originalMesh = meshFilter.mesh;
    }

    private void OnBecameVisible()
    {
        meshFilter.mesh = originalMesh;
    }
    public void Change()
    {
        meshFilter.mesh = highLightMesh;
    }
}
