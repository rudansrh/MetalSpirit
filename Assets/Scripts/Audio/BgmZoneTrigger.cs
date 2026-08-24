using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BgmZoneTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private int priority;

    public AudioClip BgmClip => bgmClip;
    public int Priority => priority;

    private void Reset()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        if (zoneCollider != null)
        {
            zoneCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        AudioManager.instance?.EnterBgmZone(this);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        AudioManager.instance?.EnterBgmZone(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player) && player.IsPossessing)
        {
            return;
        }

        if (!IsPlayer(other))
        {
            return;
        }

        AudioManager.instance?.ExitBgmZone(this);
    }

    private void OnDisable()
    {
        AudioManager.instance?.ExitBgmZone(this);
    }

    private static bool IsPlayer(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
        {
            return true;
        }

        if (other.TryGetComponent<Enemy>(out var enemy) && enemy.isPossessed) // 빙의 중에도 플레이어로 간주
        {
            return true;
        }

        return false;
    }
}
