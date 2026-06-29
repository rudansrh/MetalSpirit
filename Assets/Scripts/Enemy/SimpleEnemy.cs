using Unity.VisualScripting;
using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;           // �̵� �ӵ�
    [SerializeField] private Transform leftWaypoint;     // ���� �̵� �Ѱ���
    [SerializeField] private Transform rightWaypoint;    // ���� �̵� �Ѱ���

    private bool movingRight = true;

    [Header("Damage Settings")]
    [SerializeField] private float damage = 15f;         // �� ���� �� ������
    [SerializeField] private float knockbackForce = 7f;  // �˹� ��

    [Header("Enemy Hp")]
    [SerializeField] private float enemyHp = 30f;

    private void Update()
    {
        MovePatrol();
    }

    // �¿� ���� ����
    private void MovePatrol()
    {
        // ��������Ʈ�� �Ҵ���� �ʾҴٸ� �̵����� ����
        if (leftWaypoint == null || rightWaypoint == null) return;

        if (movingRight)
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
            if (transform.position.x >= rightWaypoint.position.x)
            {
                Flip();
            }
        }
        else
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);
            if (transform.position.x <= leftWaypoint.position.x)
            {
                Flip();
            }
        }
    }

    // ���� ��ȯ �� ��������Ʈ ����
    private void Flip()
    {
        movingRight = !movingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // �÷��̾�� �浹 �� ������ �� �˹� ó�� (SpikeObstacle ����)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. ������ ����
        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage, DamageType.Normal);
        }

        // 2. ���� �˹� ����
        if (collision.gameObject.TryGetComponent<Rigidbody2D>(out var rb))
        {
            Vector2 knockbackDir = collision.transform.position - transform.position;

            // X�� ���� ���� �� Y���� ����ִ� ȿ��
            knockbackDir = new Vector2(Mathf.Sign(knockbackDir.x) * 0.4f, 1f).normalized;

            // �ϰ� ���� �� �˹� ���� ����
            if (rb.linearVelocityY > 0) knockbackDir.y = -1;

            rb.linearVelocity = Vector2.zero; // ���� �ӵ� �ʱ�ȭ
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }
    }

    public void Attacked(float playerDamage)
    {
        enemyHp -= playerDamage;
        if (enemyHp <= 0)
        {
            this.gameObject.SetActive(false);
            Debug.Log("Enemy killed");
        }
    }
}
