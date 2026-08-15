using UnityEngine;
using UnityEngine.UI;

public class PossessGauge : MonoBehaviour
{
    PlayerController playerController;

    [SerializeField] Image possessGauge;
    [SerializeField] Image background;
    public float possessionLimitTime = 60;

    private float currentPossessTime;
    private bool gaugeShowing = false;
    public bool isInfinityPossess = true;
    private RectTransform rectTransform;

    public Transform target;

    private void Start()
    {
        playerController = PlayerController.Instance;
        playerController.possessGauge = this;
        rectTransform = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (!gaugeShowing) return;

        if (!playerController.isTalking) currentPossessTime -= Time.deltaTime;
        if (currentPossessTime < 0 && !isInfinityPossess)
        {
            playerController.OnPossess(null);
        }

        possessGauge.fillAmount = currentPossessTime / possessionLimitTime;
        rectTransform.position = Camera.main.WorldToScreenPoint(target.position + Vector3.up * 1f);
    }

    public void possessGaugeShow()
    {
        currentPossessTime = possessionLimitTime;
        gaugeShowing = true;

        if (isInfinityPossess) return;
        possessGauge.gameObject.SetActive(true);
        background.gameObject.SetActive(true);
    }

    public void possessGaugeHide()
    {
        gaugeShowing = false;
        possessGauge.gameObject.SetActive(false);
        background.gameObject.SetActive(false);
    }
}
