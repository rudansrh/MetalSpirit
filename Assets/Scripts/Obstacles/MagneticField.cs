using System.Collections;
using UnityEngine;

public class MagneticField : MonoBehaviour
{
    [SerializeField] float bounceSpeed = 5f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerAbilityManager>(out var ability))
        {
            if (!ability.isSoul) return;

            Rigidbody2D rb = collision.attachedRigidbody;

            ColliderDistance2D distance = GetComponent<Collider2D>().Distance(collision);

            Vector2 normal = distance.normal;
            StartCoroutine(playerBounce());
            rb.linearVelocity = Vector2.Reflect(rb.linearVelocity, normal)*bounceSpeed;
        }
    }

    private void OnTriggerStay2D(Collider2D cpllision)
    {
        if (!cpllision.CompareTag("Player")) return;

        Collider2D fieldCollider = GetComponent<Collider2D>();
        ColliderDistance2D distance = fieldCollider.Distance(cpllision);

        if (!distance.isOverlapped) return;

        Rigidbody2D rb = cpllision.attachedRigidbody;
        Vector2 normal = distance.normal;

        //겹친 만큼 바깥쪽으로 밀어냄
        rb.position += normal * Mathf.Abs(distance.distance);
    }

    IEnumerator playerBounce()
    {
        PlayerController.Instance.canMove = false;
        yield return new WaitForSeconds(0.3f);
        PlayerController.Instance.canMove = true;
    }

    public void canApproach()
    {
        gameObject.SetActive(false);
    }
}
