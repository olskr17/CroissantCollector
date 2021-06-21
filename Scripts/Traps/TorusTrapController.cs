using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorusTrapController : MonoBehaviour {
    private GameManager gameManager;
    private PlayerController playerControl;
    private Vector3 size;
    private bool checkXPos = false;
    private float checkPos;

    private void OnEnable()
    {
        StartCoroutine(CheckPosition());
    }

    void Awake()
    {
        playerControl = FindObjectOfType<PlayerController>();
        gameManager = FindObjectOfType<GameManager>();
        size = GetComponent<Renderer>().bounds.size;
    }


    void Fire()
    {
        Vector3 createPos = transform.position + new Vector3(0, size.y / 2, 0);
        GameObject bullet = gameManager.GetBullet();
        bullet.transform.position = createPos;
        Vector3 dir;
        if (checkXPos)
        {
            bullet.transform.eulerAngles = new Vector3(0, -180, 0);
            dir = -Vector3.forward;
        }
        else
        {
            bullet.transform.eulerAngles = new Vector3(0, -90, 0);
            dir = -Vector3.right;
        }
        bullet.GetComponent<BulletController>().dir = dir;
    }



    IEnumerator CheckPosition()
    {
        while (true)
        {
            if (transform.position != Vector3.zero)
            {
                StartCoroutine(CheckAndFire());
                yield break;
            }              
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator CheckAndFire()
    {
        if (Mathf.Round(transform.eulerAngles.y) == 0)
        {
            checkXPos = true;
            checkPos = Mathf.Round(transform.position.x);
        }
        else
        {
            checkXPos = false;
            checkPos = Mathf.Round(transform.position.z);
        }
        while (!playerControl.isDead)
        {
            float check = (checkXPos) ?
                (Mathf.Round(playerControl.transform.position.x)) :
                (Mathf.Round(playerControl.transform.position.z));
            if (check == checkPos)
            {
                Fire();
                yield break;
            }
            yield return null;
        }
    }
}
