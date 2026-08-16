using System;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    [SerializeField] int hp = 1;
    public void objectDamaged()
    {
        hp--;
        if(hp <= 0)
        {
            GetComponent<DropItem>().dropItem();
            gameObject.SetActive(false);
        }
    }
}
