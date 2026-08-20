using System;
using UnityEngine;

public class ItemBox : MonoBehaviour, IBreakable
{
    [SerializeField] int hp = 1;
    public void objectDamaged()
    {
        hp--;
        if(hp <= 0)
        {
            GetComponent<DropItem>().dropItem();
            AudioManager.instance?.PlaySfx(AudioManager.Sfx.BoxOpen); //***
            gameObject.SetActive(false);
        }
    }
}
