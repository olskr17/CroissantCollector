using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornTrapController : MonoBehaviour {

    public void GoUp()
    {
        StartCoroutine(Up());
    }
    IEnumerator Up()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0, 0.6f, 0);
        float time = 0.1f;
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            float factor = t / time;
            transform.position = Vector3.Lerp(startPos, endPos, factor);
            yield return null;
        }
    }
}
