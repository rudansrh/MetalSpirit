using UnityEngine;
using UnityEngine.UI;

public class PossessGauge : MonoBehaviour
{
    PlayerController playerController;

    [SerializeField] Image possessGauge;
    [SerializeField] Image background;
    [SerializeField] private float possessionLimitTime;
    private float currentPossessTime;
    private bool gaugeShowing = false;
    private RectTransform rectTransform;

    public Transform target;

    private void Start()
    {
        playerController = PlayerController.Instance;
        playerController.possessGauge = this;
        rectTransform = GetComponent<RectTransform>();
    }

    /*void LateUpdate()
    {
        if (!gaugeShowing) return;

        if (!playerController.isTalking) currentPossessTime -= Time.deltaTime;
        if (currentPossessTime < 0)
        {
            playerController.OnPossess(null);
        }

        possessGauge.fillAmount = currentPossessTime / possessionLimitTime;
        rectTransform.position = Camera.main.WorldToScreenPoint(target.position + Vector3.up * 1f);
    }*/
    private void LateUpdate()
    {
        if (!gaugeShowing)
            return;

        if (!playerController.isTalking)
            currentPossessTime -= Time.deltaTime;

        if (currentPossessTime < 0)
        {
            playerController.OnPossess(null);
        }

        possessGauge.fillAmount =
            currentPossessTime / possessionLimitTime;

        Vector3 worldPosition =
            target.position + Vector3.up * 1f;

        Vector3 screenPosition =
            Camera.main.WorldToScreenPoint(worldPosition);

        rectTransform.position = screenPosition;
    }

    public void possessGaugeShow()
    {
        currentPossessTime = possessionLimitTime;
        gaugeShowing = true;
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
