using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceAndFireTrapController : MonoBehaviour {

    public List<Mesh> listMesh = new List<Mesh>();

    private MeshFilter meshFilter;
    private int meshCount = -1;
	// Use this for initialization
	void Start () {
        meshFilter = GetComponent<MeshFilter>();      
	}

    private void OnEnable()
    {
        StartCoroutine(LoopMesh());
    }

    IEnumerator LoopMesh()
    {
        while (true)
        {
            if (meshFilter != null)
            {
                meshCount = (meshCount + 1 >= listMesh.Count) ? (0) : (meshCount + 1);
                meshFilter.mesh = listMesh[meshCount];
            }     
            yield return new WaitForSeconds(0.2f);
        }
    }
}
