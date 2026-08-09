using System;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] MinigameManager minigame;
    public bool minigameStarted = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerController>(out var player) && !minigameStarted)
        {
            if (player.isPossessing) return; //TODO : 미니게임 시작 조건 추가하기

            minigame.MinigameReady();
        }
    }
}
