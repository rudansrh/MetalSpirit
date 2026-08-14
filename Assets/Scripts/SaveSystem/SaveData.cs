using UnityEngine;

[System.Serializable]
public class SaveData
{
    // 저장할 씬 이름
    public string savedSceneName;

    // 플레이어 위치
    public float playerPosX;
    public float playerPosY;

    // 플레이어 상태
    public float playerHp;
    public float playerStamina;
    public PlayerStage currentPlayerStage;
    public bool isSoulState;

    //인벤토리 저장
    public InventoryItem[] inventoryItems;

}