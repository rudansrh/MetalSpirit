using System;
using UnityEngine;

public class Door : MonoBehaviour, IBreakable
{
    [SerializeField] MinigameManager minigame;
    public int maxHp = 1;
    public int currentHp = 1;
    public bool minigameStarted = false;

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerController>(out var player) && !minigameStarted)
        {
            if (player.isPossessing) return; //TODO : 미니게임 시작 조건 추가하기

            minigame.MinigameReady();
        }
    }*/

    public void objectDamaged()
    {
        if (minigameStarted) return;

        currentHp--;
        if (currentHp <= 0) //TODO : 미니게임 시작 조건 추가하기
        {
            minigame.MinigameReady();
            gameObject.SetActive(false);
        }
    }
}
