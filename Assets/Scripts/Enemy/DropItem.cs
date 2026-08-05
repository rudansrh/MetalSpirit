using UnityEngine;

public class DropItem : MonoBehaviour
{
    [SerializeField] GameObject[] ItemPrefabs;
    [SerializeField] Transform Items;

    public void dropItem()
    {
        foreach (var item in ItemPrefabs)
        {
            var drop = Instantiate(item, Items);

            LayerMask wallLayer = LayerMask.GetMask("Wall");
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 5f, wallLayer);
            Vector2 targetPos = transform.position;
            targetPos.y -= hit.distance;

            drop.transform.position = targetPos;
        }
    }
}
