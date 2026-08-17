using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class BreakableDoor : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(disappear());
        }
    }

    IEnumerator disappear()
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }
}
