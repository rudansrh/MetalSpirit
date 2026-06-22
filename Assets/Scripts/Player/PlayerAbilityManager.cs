using UnityEngine;

public class PlayerAbilityManager : MonoBehaviour
{

    [Header("State")]
    public bool isSoul = true;
    public bool canPossess = true;

    [Header("Abilities")]
    public bool canDash = false;
    public bool canWallJump = false;

    public void PossessBody()
    {
        isSoul = false;
        canDash = true;
        canWallJump = true;
        Debug.Log("빙의 성공! 물리 능력 활성화.");
    }
}
