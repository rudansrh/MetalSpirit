using UnityEngine;
using System.Collections;


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
    [SerializeField] Transform retryPoint;

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
            Vector2 fallPoint = new Vector2(Random.Range(leftWall.transform.position.x, rightWall.transform.position.x), 3.5f);
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
        door.GetComponent<Door>().minigameStarted = false;
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

        while (p_transform.position != end)
        {
            p_transform.position = Vector3.MoveTowards(p_transform.position, end, Time.deltaTime*3);

            yield return null;
        }

        p_transform.position = end;
        player.canMove = true;
        door.SetActive(true);
        door.GetComponent<Door>().minigameStarted = true;
        gameStart = true;
    }
}
