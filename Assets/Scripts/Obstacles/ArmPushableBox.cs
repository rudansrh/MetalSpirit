using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ArmPushableBox : MonoBehaviour
{
    [Header("Push Settings")]
    [SerializeField] private float pushForce = 20f; // 팔 공격에 맞았을 때 날아가는 힘

    [Header("Buoyancy Settings")]
    [SerializeField] private float floatPower = 3f; // 물에 뜨는 부력의 강도
    [SerializeField] private float waterDrag = 3f;  // 물 속에서의 저항

    private Rigidbody2D rb;
    private bool isInWater = false;
    private float waterSurfaceY;
    private float defaultDrag;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultDrag = rb.linearDamping;
    }

    public void Push(float directionX)
    {
        Vector2 forceDirection = new Vector2(directionX, 0.5f).normalized;

        rb.AddForce(forceDirection * pushForce, ForceMode2D.Impulse);

    }

    private void FixedUpdate()
    {
        if (isInWater)
        {
            float depth = waterSurfaceY - transform.position.y;

            if (depth > 0)
            {

                float buoyancy = Mathf.Clamp(depth * floatPower, 0f, 20f);
                rb.AddForce(Vector2.up * buoyancy * rb.mass);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.TryGetComponent<WaterObstacle>(out _))
        {
            isInWater = true;
            rb.linearDamping = waterDrag;

            waterSurfaceY = collision.bounds.max.y;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<WaterObstacle>(out _))
        {
            isInWater = false;
            rb.linearDamping = defaultDrag;
        }
    }
}