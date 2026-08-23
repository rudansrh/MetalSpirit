using System.Collections;
using UnityEngine;

public class DoorCoroutine : MonoBehaviour
{
    [Header("문 설정")]
    [SerializeField] private Vector3 targetPositionOffset = new Vector3(0, 2f, 0); // 문이 올라갈 위치 오프셋
    [SerializeField] private Vector3 targetScale = new Vector3(1f, 0.1f, 1f);      // 줄어들 스케일
    [SerializeField] private float duration = 1.0f;                                // 열리고 닫히는 애니메이션 시간(초)

    [Header("자동 닫힘 설정")]
    [SerializeField] private bool autoClose = true;                                // 자동 닫힘 사용 여부
    [SerializeField] private float autoCloseDelay = 5.0f;                          // 대기 시간(초)

    [Header("가벽")]
    [SerializeField] private GameObject wall;                                      // 문이 열릴 때 비활성화할 가벽

    private Vector3 originalPosition;
    private Vector3 originalScale;

    private Coroutine activeCoroutine;    // 이동/스케일 코루틴
    private Coroutine autoCloseCoroutine; // 자동 닫힘 타이머 코루틴

    private bool isOpen = false;

    private void Start()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;
    }

    // 버튼 클릭 시 호출
    public void ToggleDoor()
    {
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    // 문 열기
    public void OpenDoor()
    {
        // 이미 열려있거나 열리는 중이면 진행
        if (isOpen && activeCoroutine != null) return;

        StopActiveCoroutines();

        isOpen = true;
        Vector3 targetPos = originalPosition + targetPositionOffset;

        if (wall != null)
        {
            wall.SetActive(false); // 문이 열릴 때 가벽 비활성화
        }

        activeCoroutine = StartCoroutine(AnimateDoor(targetPos, targetScale, () =>
        {
            // 문이 완전히 열린 후 자동 닫힘 타이머 시작
            if (autoClose)
            {
                autoCloseCoroutine = StartCoroutine(AutoCloseRoutine());
            }
        }));
    }

    // 문 닫기
    public void CloseDoor()
    {
        StopActiveCoroutines();

        isOpen = false;
        activeCoroutine = StartCoroutine(AnimateDoor(originalPosition, originalScale, null));
    }

    // 지정된 시간 대기 후 닫는 코루틴
    private IEnumerator AutoCloseRoutine()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        CloseDoor();
    }

    // 진행 중인 코루틴 정리
    private void StopActiveCoroutines()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }

    // 위치와 스케일을 변경하는 코루틴 (완료 시 콜백 실행 가능)
    private IEnumerator AnimateDoor(Vector3 toPos, Vector3 toScale, System.Action onComplete)
    {
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // SmoothStep으로 부드러운 이동/스케일 감속 효과
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, toPos, t);
            transform.localScale = Vector3.Lerp(startScale, toScale, t);

            yield return null;
        }

        transform.position = toPos;
        transform.localScale = toScale;
        activeCoroutine = null;

        // 애니메이션이 끝나면 실행할 작업이 있다면 호출 (자동 닫힘 타이머 등)
        onComplete?.Invoke();
    }
}