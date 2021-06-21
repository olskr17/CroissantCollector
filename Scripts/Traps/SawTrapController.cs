using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawTrapController : MonoBehaviour {
    private GameObject gear;
    private int turn;
    private float movingTime = 2f;
	// Use this for initialization
    void OnEnable()
    {
        turn = -1;
        gear = transform.GetChild(0).gameObject;
        StartCoroutine(RotateGear());
        StartCoroutine(MoveGear());
    }
    IEnumerator RotateGear()
    {
        while (true)
        {
            gear.transform.eulerAngles += new Vector3(0, 0, 300 * turn * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator MoveGear()
    {
        bool firstMove = true;
        while (true)
        {
            turn = -1;
            float startX = gear.transform.localPosition.x;
            float endX = (firstMove) ? (startX + 1) : (-startX);
            float t = 0;
            while (t < movingTime)
            {
                t += Time.deltaTime;
                float fraction = t / movingTime;
                float newX = Mathf.Lerp(startX, endX, fraction);
                Vector3 newPos = gear.transform.localPosition;
                newPos.x = newX;
                gear.transform.localPosition = newPos;
                yield return null;
            }

            turn = 1;
            t = 0;
            startX = (firstMove) ? (-endX) : (startX);
            while (t < movingTime)
            {
                t += Time.deltaTime;
                float fraction = t / movingTime;
                float newX = Mathf.Lerp(endX, startX, fraction);
                Vector3 newPos = gear.transform.localPosition;
                newPos.x = newX;
                gear.transform.localPosition = newPos;
                yield return null;
            }
            firstMove = false;
        }
    }
}
