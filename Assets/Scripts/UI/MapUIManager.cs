using UnityEngine;
using UnityEngine.UI;

public class MapUIManager : MonoBehaviour
{
    public static MapUIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private Image mapImage;
    [SerializeField] private RectTransform mapViewport;

    [Header("Fallback")]
    [SerializeField] private Sprite defaultMapSprite;

    private MapAreaTrigger currentArea;

    public bool IsOpen => mapPanel != null && mapPanel.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (mapPanel != null)
        {
            mapPanel.SetActive(false);
        }

        if (mapImage != null)
        {
            mapImage.preserveAspect = true;
        }
    }

    public void ToggleMap()
    {
        if (IsOpen)
        {
            CloseMap();
            return;
        }

        OpenMap();
    }

    public void OpenMap()
    {
        if (mapPanel == null)
        {
            Debug.LogWarning("MapUIManager: mapPanel reference is missing.");
            return;
        }

        mapPanel.SetActive(true);
        RefreshMapImage();

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.isUIopen = true;
            PlayerController.Instance.canMove = false;
            PlayerController.Instance.StopMovement();
            PlayerController.Instance.StopDash();
        }
    }

    public void CloseMap()
    {
        if (mapPanel != null)
        {
            mapPanel.SetActive(false);
        }

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.isUIopen = false;
            PlayerController.Instance.canMove = true;
        }
    }

    public void SetCurrentArea(MapAreaTrigger area)
    {
        currentArea = area;
        RefreshMapImage();
    }

    public void ClearCurrentArea(MapAreaTrigger area)
    {
        if (currentArea != area)
        {
            return;
        }

        currentArea = null;
        RefreshMapImage();
    }

    private void RefreshMapImage()
    {
        if (mapImage == null)
        {
            return;
        }

        Sprite targetSprite = currentArea != null && currentArea.MapSprite != null
            ? currentArea.MapSprite
            : defaultMapSprite;

        mapImage.sprite = targetSprite;
        UpdateMapImageLayout(targetSprite);
    }

    private void OnRectTransformDimensionsChange()
    {
        if (mapImage == null || mapImage.sprite == null)
        {
            return;
        }

        UpdateMapImageLayout(mapImage.sprite);
    }

    private void UpdateMapImageLayout(Sprite targetSprite)
    {
        if (mapImage == null || targetSprite == null)
        {
            return;
        }

        RectTransform imageRect = mapImage.rectTransform;
        RectTransform viewportRect = mapViewport != null ? mapViewport : imageRect.parent as RectTransform;

        if (imageRect == null || viewportRect == null)
        {
            return;
        }

        Vector2 spriteSize = targetSprite.rect.size;
        Vector2 viewportSize = viewportRect.rect.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f || viewportSize.x <= 0f || viewportSize.y <= 0f)
        {
            return;
        }

        float scale = Mathf.Min(viewportSize.x / spriteSize.x, viewportSize.y / spriteSize.y);
        imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, spriteSize.x * scale);
        imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, spriteSize.y * scale);
    }
}
