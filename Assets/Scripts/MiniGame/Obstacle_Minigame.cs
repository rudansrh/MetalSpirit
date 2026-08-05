using UnityEngine;

public class Obstacle_Minigame : MonoBehaviour
{
    public MinigameManager minigameManager;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("미니게임 실패");
            minigameManager.MinigameFail();
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.1f)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
