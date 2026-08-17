using UnityEngine;
using System.Collections;
using Unity.Collections;


public class MinigameManager : MonoBehaviour
{
    [Header("Minigame Setting")]
    [SerializeField] float playTime = 20f;
    [SerializeField] float minSpawnInterval = 0.5f;
    [SerializeField] float maxSpawnInterval = 1.5f;

    [Header("Minigame Objects")]
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] GameObject door;
    [SerializeField] Transform leftWall;
    [SerializeField] Transform rightWall;
    [SerializeField] Transform ceiling;
    [SerializeField] Transform retryPoint;
    [SerializeField] float offsetX;

    PlayerController player;
    Transform p_transform;

    float currentTime = 0;
    float cooltime = 4;
    bool gameStart = false;

    void Start()
    {
        player = PlayerController.Instance;
        p_transform = player.transform;
    }

    private void Update()
    {
        if (!gameStart) return;

        if(currentTime > playTime)
        {
            MinigameClear();
            return;
        }

        currentTime += Time.deltaTime;
        cooltime -= Time.deltaTime;

        if( cooltime <= 0 )
        {
            cooltime = Random.Range(minSpawnInterval, maxSpawnInterval); //생성 쿨타임 초기화
            var prefab = Instantiate(obstaclePrefab, transform);
            prefab.GetComponent<Obstacle_Minigame>().minigameManager = this;
            Vector2 fallPoint = new Vector2(Random.Range(leftWall.transform.position.x + offsetX, rightWall.transform.position.x - offsetX), ceiling.position.y);
            prefab.transform.position = fallPoint;
            prefab.GetComponent<Rigidbody2D>().AddForceY(1f, ForceMode2D.Impulse);
        }
    }
  
    void MinigameClear()
    {
        Debug.Log("미니게임 클리어");
        cameraFollow.Instance.ReturnToNormal();
        gameStart = false;
        door.SetActive(false);
        player.isPlayingMinigame = false;
        //TODO : 미니게임 클리어 보상 추가하기
    }
    
    public void MinigameFail()
    {
        Debug.Log("미니게임 실패");
        cameraFollow.Instance.ReturnToNormal();
        gameStart = false;
        Door doorScript = door.GetComponent<Door>();
        doorScript.minigameStarted = false;
        doorScript.currentHp = doorScript.maxHp;
        player.isPlayingMinigame = false;
        player.transform.position = retryPoint.position;
        currentTime = 0;
        cooltime = 4;
    }

    public void MinigameReady()
    {
        door.SetActive(false);
        cameraFollow.Instance.ChangeOffset(new Vector3(0, 3, -10));
        PlayerController.Instance.isPlayingMinigame = true;
        StartCoroutine(MoveToCenter());
    }

    IEnumerator MoveToCenter() // 미니게임 영역 진입시 중앙으로 이동
    {
        player.canMove = false;

        Vector3 start = p_transform.position;
        Vector3 end = (leftWall.position + rightWall.position)/2;
        end.y = start.y;

        while (Mathf.Abs(p_transform.position.x - end.x) > 0.1f)
        {
            p_transform.position = Vector3.MoveTowards(p_transform.position, end, Time.deltaTime*3);

            yield return null;
        }

        player.canMove = true;
        door.SetActive(true);
        door.GetComponent<Door>().minigameStarted = true;
        gameStart = true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (!collision.TryGetComponent<PlayerAbilityManager>(out var ability))
            return;

        if (!ability.isSoul) return;

        Collider2D fieldCollider = GetComponent<Collider2D>();
        ColliderDistance2D distance = fieldCollider.Distance(collision);

        if (!distance.isOverlapped) return;

        Rigidbody2D rb = collision.attachedRigidbody;

        // 겹친 만큼 벽 바깥쪽으로 이동시켜서 벽을 통과하지 못하게 함
        rb.position += distance.normal * Mathf.Abs(distance.distance);
    }
}
