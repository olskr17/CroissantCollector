using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour {

    [HideInInspector]
    public Vector3 dir;
    private GameManager gameManager;
	// Use this for initialization
	void Start () {
        gameManager = FindObjectOfType<GameManager>();
	}
	
	// Update is called once per frame
	void Update () {

        transform.position += dir * gameManager.bulletSpeed * Time.deltaTime;
        Vector2 pos = Camera.main.WorldToViewportPoint(transform.position);
        if (pos.y >= 1.5f)
        {
            Reset();
        }
	}

    void Reset()
    {
        transform.SetParent(gameManager.transform);
        transform.position = Vector3.zero;
        transform.eulerAngles = Vector3.zero;
        gameObject.SetActive(false);
    }
}
