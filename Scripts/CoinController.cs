using UnityEngine;
using System.Collections;
using SgLib;

public class CoinController : MonoBehaviour
{
    private MeshCollider meshCollider;
    private float speed;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.coin);
            meshCollider.enabled = false;
            CoinManager.Instance.AddCoins(1);
            StartCoroutine(Up());
        }      
    }
    // Use this for initialization
    private void OnEnable()
    {
        if (meshCollider == null)
            meshCollider = GetComponent<MeshCollider>();
        meshCollider.enabled = true;
        speed = Random.Range(4f, 7f);
        StartCoroutine(Rotate());
    }

    IEnumerator Rotate()
    {
        while (true)
        {
            transform.Rotate(Vector3.up * speed);
            yield return null;
        }
    }
    //Move up
    IEnumerator Up()
    {
        float time = 1f;
        float startY = transform.position.y;
        float endY = startY + 10f;

        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            float fraction = t / time;
            float newY = Mathf.Lerp(startY, endY, fraction);
            Vector3 newPos = transform.position;
            newPos.y = newY;
            transform.position = newPos;
            yield return null;
        }
        gameObject.SetActive(false);
        transform.eulerAngles = Vector3.zero;
        transform.position = Vector3.zero;
    }
}
