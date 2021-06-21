using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectReseter : MonoBehaviour {

    List<string> listNotCreatePath = new List<string>();
    List<string> listCreatePath = new List<string>();

    private void Start()
    {
        listNotCreatePath.Add("Rock");
        listNotCreatePath.Add("Coin");
        listNotCreatePath.Add("Obstacle");
        listNotCreatePath.Add("TorusTrap");

        listCreatePath.Add("FireTrap");
        listCreatePath.Add("IceTrap");
        listCreatePath.Add("SawTrap");
        listCreatePath.Add("SpeedDownTrap");
        listCreatePath.Add("ThornTrap");
        listCreatePath.Add("Ground");
        listCreatePath.Add("LastGround");
        listCreatePath.Add("Stair");
    }


    private void OnTriggerEnter(Collider other)
    {
        if (listNotCreatePath.Contains(other.tag))
        {
            ResetObject(other.gameObject, false);
        }
        else
        {
            ResetObject(other.gameObject, true);
        }
    }

    void ResetObject(GameObject currentObject, bool createPath)
    {
        currentObject.transform.position = Vector3.zero;
        currentObject.transform.eulerAngles = Vector3.zero;
        currentObject.transform.SetParent(GameManager.Instance.transform);
        currentObject.SetActive(false);
        if (createPath)
            GameManager.Instance.CreatePath();
    }
}
