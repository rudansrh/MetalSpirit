using UnityEngine;

public class PlayerAbilityManager : MonoBehaviour
{

    [Header("State")]
    public bool isSoul = true;
    public bool canPossess = true;

    [Header("Abilities")]
    public bool canDash = false;
    public bool canWallJump = false;
    public bool canLowAttack = false;
    public bool canHighAttack = false;

    [Header("Inventory")]
    public bool canUseInventory = false;

    public void PossessBody()
    {
        isSoul = false;
        canDash = true;
        canWallJump = true;
        canLowAttack = true;
        canHighAttack = true;
        Debug.Log("ºùÀÇ ¼º°ø");
    }

    public void DepossessBody()
    {
        isSoul = true;
        canDash = false;
        canWallJump = false;
        canLowAttack = false;
        canHighAttack = false;
        Debug.Log("ºùÀÇ ÇØÁ¦");
    }
}
