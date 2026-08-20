using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private static PlayerController instance;

    Rigidbody2D rigid;
    Collider2D col;
    SpriteRenderer spriteRenderer;
    [SerializeField] PlayerVisualManager visualManager;
    [SerializeField] PlayerAbilityManager abilityManager;
    [SerializeField] PlayerProgressionManager progressionManager;
    [SerializeField] PlayerCombatManager combatManager;

    //스태미나 컴포넌트 참조 추가
    Stamina stamina;

    [Header("Movement Settings")]
    [SerializeField] float speed;
    [SerializeField] float soulSpeed = 10f;
    [SerializeField] float jumpForce;
    [SerializeField] float wallJumpDelay;
    [SerializeField] float wallClimbSpeed = 0.25f;
    [SerializeField] float wallClimbHoldStaminaCostPerSecond = 2f;
    [SerializeField] float wallClimbMoveStaminaCostPerSecond = 3f;
    [SerializeField] float wallClimbFallOffDistance = 0.1f;
    [SerializeField] float jumpStaminaCost = 10f;
    [SerializeField] int jumpLimit = 3;
    [SerializeField]int jumpCount = 0;
    [SerializeField] float jumpCooldown = 0.2f;
    [SerializeField]float jumpCooldownTimer = 0f;

    [Header("Dash Settings")]
    [SerializeField] float dashSpeed = 20f;     // 돌진 속도
    [SerializeField] float dashDuration = 0.2f; // 돌진 유지 시간
    [SerializeField] float dashCooldown = 1f;   // 돌진 쿨타임
    //대시할 때 스태미나 소모량
    [SerializeField] float dashStaminaCost = 20f;


    [Header("Slow Effect Settings")]
    [SerializeField] float speedMultiplier = 1f; // 느려지는 효과 수치
    [SerializeField] int slowEffectCount = 0;    // 느려지는 효과 중첩 카운트
    [SerializeField] float cobwebMaxRiseSpeed = 2.5f;
    [SerializeField] float cobwebMaxFallSpeed = 0.1f;

    [Header("Possession Settings")]
    public Enemy targetEnemyToPossess = null;
    public PossessGauge possessGauge;

    [Header("Interaction Settings")]
    public IInteractable nearbyInteractable = null; // 근처에 있는 상호작용 객체


    bool isDashing = false;
    bool canDashAgain = true;
    Coroutine DashCoroutine;
    float facingDirection = -1f; // 현재 기본 애니메이션이 왼쪽을 바라보므로 초기 방향도 왼쪽
    float originalGravity = 1f;

    //벽점프 관련 변수
    public bool isJump = false;
    bool isWallClimbing = false;
    int insideWall = 0;
    [SerializeField]float wallClimbDetachDirection = 0f;

    public bool isWallAttatching = false;

    Vector2 moveInput;

    public bool canMove = true;
    public bool isInvincibility = false;

    public bool isPossessing { get; private set; } = false; //에너미한테 빙의중인지 판단
    public int[] unlockedPassword = new int[4] {0,0,0,0};

    public bool isPlayingMinigame = false;
    public bool isTalking = false;
    public bool isUIopen = false;
    bool isHeadEnemy = false;

    public int lastSavedSlot = 0;
    public bool isMovingToNextScene = false;

    public CanInteractUI canInteractUI;
    public GameObject possessChecker;
    public static PlayerController Instance => instance == null ? null : instance;
    public bool IsAutoDepossessDialogueActive => isAwaitingAutoDepossessAfterDialogue;

    bool isSubscribedToProgressionState = false;
    bool isAwaitingAutoDepossessAfterDialogue = false;
    readonly PlayerMovementBoundsController movementBoundsController = new PlayerMovementBoundsController();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            rigid = GetComponent<Rigidbody2D>();
            originalGravity = rigid.gravityScale;
            stamina = GetComponent<Stamina>();
            col = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (abilityManager == null)
            {
                abilityManager = GetComponent<PlayerAbilityManager>();
            }

            if (visualManager == null)
            {
                visualManager = GetComponent<PlayerVisualManager>();
            }

            if (progressionManager == null)
            {
                progressionManager = GetComponent<PlayerProgressionManager>();
            }

            if (combatManager == null)
            {
                combatManager = GetComponent<PlayerCombatManager>();
            }

            if (combatManager == null)
            {
                combatManager = gameObject.AddComponent<PlayerCombatManager>();
            }

            SubscribeToProgressionState();
            UpdateFormState();
            UpdateFacingVisual();
            UpdateAnimationState();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        cameraFollow.Instance.SetTarget(transform);
        SaveManager.Instance.SaveGame(0);
    }

    void OnDestroy()
    {
        UnsubscribeAutoDepossessDialogueHandler();
        UnsubscribeFromProgressionState();

        if (instance == this)
        {
            instance = null;
        }
    }

    void OnEnable()
    {
        SubscribeToProgressionState();
    }

    void OnDisable()
    {
        UnsubscribeAutoDepossessDialogueHandler();
        UnsubscribeFromProgressionState();
    }

    void UpdateFormState()
    {
        if (IsSoulForm())
        {
            rigid.gravityScale = 0f;
            col.isTrigger = true;
        }
        else
        {
            rigid.gravityScale = originalGravity;
            col.isTrigger = false;
        }

        if (visualManager != null)
        {
            visualManager.ApplyCurrentVisual();
        }
    }

    void Update()
    {
        UpdateAnimationState();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        jumpCooldownTimer += Time.deltaTime;
        // 대시 중일 땐 이동과 중력 무시
        if (isDashing || !canMove)
        {
            rigid.linearVelocity = FilterVelocityAgainstBounds(rigid.linearVelocity);
            ClampControlledBodyToBounds();
            return;
        }

        if (IsAttackInProgress)
        {
            rigid.linearVelocity = FilterVelocityAgainstBounds(rigid.linearVelocity);
            ClampControlledBodyToBounds();
            return;
        }

        // 영혼 상태일 때의 이동
        if (IsSoulForm() || isHeadEnemy)
        {
            Vector2 soulMoveDir = moveInput;
            if (soulMoveDir.magnitude > 1f)
            {
                soulMoveDir.Normalize();
            }

            rigid.linearVelocity = FilterVelocityAgainstBounds(soulMoveDir * soulSpeed);
            ClampControlledBodyToBounds();
            return;
        }

        // 빙의 상태일 때
        if (UpdateWallClimbState())
        {
            ClampControlledBodyToBounds();
            return;
        }

        ApplyCobwebVerticalLimit();
        Vector2 desiredVelocity = rigid.linearVelocity;
        desiredVelocity.x = moveInput.x * speed * speedMultiplier;
        rigid.linearVelocity = FilterVelocityAgainstBounds(desiredVelocity);
        ClampControlledBodyToBounds();
    }

    #region Movement Bounds
    public void SetMovementBounds(Collider2D boundsCollider)
    {
        movementBoundsController.SetMovementBounds(boundsCollider);
        ClampControlledBodyToBounds();
    }

    public void ClearMovementBounds(Collider2D boundsCollider)
    {
        movementBoundsController.ClearMovementBounds(boundsCollider);
    }

    void ClampControlledBodyToBounds()
    {
        if (!ShouldApplyMovementBounds())
        {
            return;
        }

        movementBoundsController.ClampControlledBodyToBounds(rigid, GetActiveControlledCollider());
    }

    Vector2 FilterVelocityAgainstBounds(Vector2 desiredVelocity)
    {
        if (!ShouldApplyMovementBounds())
        {
            return desiredVelocity;
        }

        return movementBoundsController.FilterVelocityAgainstBounds(rigid, GetActiveControlledCollider(), desiredVelocity);
    }
    #endregion

    bool ShouldApplyMovementBounds()
    {
        return IsSoulForm();
    }

    Collider2D GetActiveControlledCollider()
    {
        if (col != null && col.enabled)
        {
            return col;
        }

        return rigid != null ? rigid.GetComponent<Collider2D>() : null;
    }

    public void enemyAttack(string attackParts) // 빙의상태로 공격
    {
        if(rigid.TryGetComponent<LegEnemy>(out var legEnemy) && attackParts=="Leg")
        {
            StartCoroutine(legEnemy.StompRoutine());
        }
        else if(rigid.TryGetComponent<ArmEnemy>(out var armEnemy) && attackParts == "Arm")
        {
            StartCoroutine(armEnemy.AttackRoutine());
        }
        else if (rigid.TryGetComponent<HeadEnemy>(out var headEnemy) && attackParts == "Head")
        {
            StartCoroutine(headEnemy.LaserRoutine());
        }
        else if (rigid.TryGetComponent<BodyEnemy>(out var bodyEnemy) && attackParts == "Body")
        {
            StartCoroutine(bodyEnemy.ChargeRoutine());
        }
    }

    public void resetJump()
    {
        isJump = false;
        jumpCooldownTimer = 0;
        jumpCount = 0;
    }

    public void OnMove(InputValue value)
    {
        if (isUIopen)
        {
            moveInput = Vector2.zero;
            return;
        }

        Vector2 input = value.Get<Vector2>();
        moveInput = new Vector2(
            input.x,
            IsSoulForm() || wallClimbDetachDirection != 0 || isHeadEnemy ? input.y : 0f);

        // 바라보는 방향을 업데이트
        if (moveInput.x != 0)
        {
            facingDirection = Mathf.Sign(moveInput.x);
            UpdateFacingVisual();
        }
    }

    #region Cobweb Slow Effect
    public void SetSpeedMultiplier(float multiplier)
    {
        slowEffectCount++;
        speedMultiplier = Mathf.Clamp(multiplier, 0f, 1f);
        ApplyCobwebVerticalLimit();
    }

    public void ResetSpeedMultiplier()
    {
        slowEffectCount = Mathf.Max(0, slowEffectCount - 1);

        if (slowEffectCount == 0)
        {
            speedMultiplier = 1f;
        }
    }

    void ApplyCobwebVerticalLimit()
    {
        if (slowEffectCount <= 0) return;

        float clampedY = rigid.linearVelocityY;

        if (clampedY > cobwebMaxRiseSpeed)
        {
            clampedY = cobwebMaxRiseSpeed;
        }
        else if (clampedY < -cobwebMaxFallSpeed)
        {
            clampedY = -cobwebMaxFallSpeed;
        }

        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, clampedY);
    }
    #endregion


    public void OnJump(InputValue value)
    {
        if (isUIopen) return;

        // 점프 불가
        if (isDashing || IsSoulForm() || !canMove || isWallClimbing || (jumpCooldownTimer < jumpCooldown && isJump) || jumpCount >= jumpLimit) return;

        if (value.isPressed)
        {
            if (stamina != null && !stamina.UseStamina(jumpStaminaCost))
            {
                return;
            }
            jumpCooldownTimer = 0f;
            jumpCount++;

            StopWallClimb();
            rigid.linearVelocityY = 0;
            rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            transform.Translate(new Vector3(0,0.01f,0));
            if(IsPossessing) rigid.transform.Translate(new Vector3(0, 0.01f, 0));
            isJump = true;
        }
    }

    // 대쉬 액션
    public void OnDash(InputValue value)
    {
        if (IsSoulForm() || !canMove || isUIopen) return;

        if ((abilityManager.canDash || IsPossessing) && canDashAgain && !isDashing && canMove && !IsAttackInProgress)
        {
            if (stamina != null && stamina.UseStamina(dashStaminaCost))
            {
                DashCoroutine = StartCoroutine(DashRoutine());
            }
        }
    }

    public void OnMap(InputValue value)
    {
        if (!value.isPressed) return;
        if (isPlayingMinigame || isTalking) return;

        bool isMapOpen = MapUIManager.Instance != null && MapUIManager.Instance.IsOpen;
        if (isUIopen && !isMapOpen) return;

        MapUIManager.Instance?.ToggleMap();
    }

    public void OnTutorial(InputValue value)
    {
        if (!value.isPressed) return;
        if (isPlayingMinigame || isTalking) return;

        bool isTutorialOpen = TurotialUIManager.Instance != null && TurotialUIManager.Instance.IsOpen;
        if (isUIopen && !isTutorialOpen) return;

        TurotialUIManager.Instance?.ToggleTutorial();
    }

    public void OnSetting(InputValue value)
    {
        if (!value.isPressed) return;
        if (isPlayingMinigame || isTalking) return;

        bool isSettingOpen = SettingUIManager.Instance != null && SettingUIManager.Instance.IsOpen;
        if (isUIopen && !isSettingOpen) return;

        SettingUIManager.Instance?.ToggleSetting();
    }

    IEnumerator DashRoutine()
    {

        canDashAgain = false;
        isDashing = true;
        rigid.gravityScale = 0f;

        rigid.linearVelocity = FilterVelocityAgainstBounds(new Vector2(
            facingDirection * dashSpeed * speedMultiplier,
            0.0000001f));

        yield return new WaitForSeconds(dashDuration);

        rigid.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDashAgain = true;
    }

    public void StopDash()
    {
        if (DashCoroutine == null) return;

        StopCoroutine(DashCoroutine);
        UpdateFormState();
        isDashing = false;
        canDashAgain = true;
        DashCoroutine = null;
    }

    #region Wall Climb
    // 벽 타기 상태를 업데이트
    bool UpdateWallClimbState()
    {
        if (!CanWallClimb() || Math.Sign(moveInput.x) == wallClimbDetachDirection)
        {
            StopWallClimb();
            return false;
        }

        resetJump();
        float climbInput = moveInput.y;
        bool isMovingVertically = Mathf.Abs(climbInput) > 0.01f;
        float staminaCostPerSecond = isMovingVertically
            ? wallClimbMoveStaminaCostPerSecond
            : wallClimbHoldStaminaCostPerSecond;
        float staminaCost = staminaCostPerSecond * Time.fixedDeltaTime;

        if (stamina != null && !stamina.UseStaminaSilently(staminaCost))
        {
            ForceWallClimbFall();
            return true;
        }

        isWallClimbing = true;
        rigid.gravityScale = 0f;
        rigid.linearVelocity = FilterVelocityAgainstBounds(new Vector2(
            -wallClimbDetachDirection,
            isMovingVertically ? climbInput * wallClimbSpeed : 0f));
        return true;
    }

    // 벽 타기 가능 여부
    bool CanWallClimb()
    {
        return (abilityManager.canWallJump || IsPossessing)
            && !IsSoulForm()
            && isWallAttatching
            && wallClimbDetachDirection != 0
            && !isDashing
            && canMove
            && (isWallClimbing
                || rigid.linearVelocityY < -0.01f
                || (Mathf.Abs(moveInput.y) > 0.01f && rigid.linearVelocityY <= 0.01f));
    }

    // 벽 타기 중지
    void StopWallClimb()
    {
        if (!isWallClimbing)
        {
            return;
        }

        isWallClimbing = false;
        rigid.gravityScale = originalGravity;
    }

    IEnumerator FallFromWall()
    {
        StopWallClimb();
        rigid.AddForce(Vector2.right * wallClimbDetachDirection * wallClimbFallOffDistance * 5, ForceMode2D.Impulse); //벽에 붙어있다면 반대방향으로 점프
        canMove = false;
        yield return new WaitForSeconds(0.4f);
        canMove = true;
    }

    // 벽 타기 중 스태미나 부족 시 강제 낙하 처리
    void ForceWallClimbFall()
    {
        StartCoroutine(FallFromWall());
    }

    // 벽과의 충돌에서 떨어지는 방향을 결정
    public void UpdateWallClimbDetachDirection(Collision2D collision)
    {
        if (collision.contactCount <= 0)
        {
            return;
        }

        wallClimbDetachDirection = 0;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.1f)
            {
                if (abilityManager.canWallJump) resetJump();

                wallClimbDetachDirection = Mathf.Sign(contact.normal.x);
                return;
            }
        }
    }
    #endregion

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            isWallAttatching = true;

            UpdateWallClimbDetachDirection(collision);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            isWallAttatching = true;

            UpdateWallClimbDetachDirection(collision);
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.1f)
                {
                    resetJump();
                    return;
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            isWallAttatching = false;
        }
    }

    public void OnPossess(InputValue value)
    {
        if (isUIopen || isPlayingMinigame || isTalking) return;

        //if (!value.isPressed) return;

        if (IsSoulForm() && abilityManager.canPossess)
        {
            if (!isPossessing && targetEnemyToPossess != null) //영혼 -> 빙의
            {
                PossessToEnemy();
            }
            else //영혼 -> 물질상태
            {
                if (insideWall > 0) return;

                rigid.linearVelocity = Vector3.zero;
                abilityManager.PossessBody();
                UpdateFormState();
                ClampControlledBodyToBounds();
            }
        }
        else if (!IsSoulForm() && abilityManager.canBeSoul)
        {
            if (isPossessing) //빙의 -> 영혼
            {
                DepossessFromEnemy();
            }
            else //물질 -> 영혼
            {
                rigid.linearVelocity = Vector3.zero;
                abilityManager.DepossessBody();
                UpdateFormState();
                ClampControlledBodyToBounds();
            }
        }
        else if (!IsSoulForm() && !abilityManager.canBeSoul) //파츠 얻은 후 (영혼상태 불가)
        {
            if (!IsPossessing && targetEnemyToPossess != null) //물질 -> 빙의
            {
                PossessToEnemy();
            }
            else if (isPossessing) //빙의 -> 물질
            {
                DepossessFromEnemy();
            }
        }
    }

    void PossessToEnemy()
    {
        Enemy targetEnemy = targetEnemyToPossess;
        possessGauge.target = targetEnemy.transform;
        possessGauge.possessGaugeShow();

        isPossessing = true;
        rigid.linearVelocity = Vector3.zero;
        rigid.bodyType = RigidbodyType2D.Kinematic;

        rigid = targetEnemy.GetComponent<Rigidbody2D>();
        rigid.linearVelocity = Vector3.zero;
        cameraFollow.Instance.SetTarget(targetEnemy.transform);

        targetEnemy.SetPossessed(true);
        spriteRenderer.enabled = false;
        col.enabled = false;
        col = targetEnemy.GetComponent<Collider2D>();

        abilityManager.PossessBody();
        UpdateFormState();
        resetJump();
        ClampControlledBodyToBounds();

        targetEnemyToPossess = null;
        possessChecker.SetActive(false);

        if (targetEnemy.TryGetComponent<HeadEnemy>(out var headEnemy))
        {
            isHeadEnemy = true;
            rigid.gravityScale = 0;
        }

        TryStartPossessedEnemySelfDialogue(targetEnemy);
    }

    public void DepossessFromEnemy()
    {
        isAwaitingAutoDepossessAfterDialogue = false;
        UnsubscribeAutoDepossessDialogueHandler();

        isPossessing = false;
        possessGauge.possessGaugeHide();
        transform.position = rigid.GetComponent<Transform>().position;
        cameraFollow.Instance.SetTarget(transform);
        rigid.linearVelocity = Vector3.zero;
        Enemy controlledEnemy = rigid.GetComponent<Enemy>();
        if (controlledEnemy != null)
        {
            controlledEnemy.SetPossessed(false);
        }

        rigid = GetComponent<Rigidbody2D>();
        rigid.bodyType = RigidbodyType2D.Dynamic;
        rigid.linearVelocity = Vector3.zero;
        spriteRenderer.enabled = true;
        col = GetComponent<Collider2D>();
        col.enabled = true;

        abilityManager.DepossessBody();
        UpdateFormState();
        ClampControlledBodyToBounds();

        if (isDashing) StopDash();

        isHeadEnemy = false;
        possessChecker.SetActive(true);

        if (!abilityManager.canBeSoul)
        {
            rigid.gravityScale = originalGravity;
        }
    }

    void TryStartPossessedEnemySelfDialogue(Enemy possessedEnemy)
    {
        if (possessedEnemy == null || DialogueManager.Instance == null || progressionManager == null)
        {
            return;
        }

        if (IsSoulForm() || progressionManager.EffectiveUnlockedStage != PlayerStage.FullBody)
        {
            return;
        }

        if (!possessedEnemy.TryGetComponent<NPC>(out var npc))
        {
            return;
        }

        UnsubscribeAutoDepossessDialogueHandler();
        isAwaitingAutoDepossessAfterDialogue = StartDialogueWhilePossessing(npc);

        if (!isAwaitingAutoDepossessAfterDialogue)
        {
            canMove = true;
            return;
        }

        DialogueManager.Instance.DialogueEnded += HandlePossessionDialogueEnded;
    }

    void HandlePossessionDialogueEnded(DialogueData _)
    {
        if (!isAwaitingAutoDepossessAfterDialogue)
        {
            return;
        }

        canMove = false;
        StopMovement();
        isAwaitingAutoDepossessAfterDialogue = false;
        UnsubscribeAutoDepossessDialogueHandler();

        if (isPossessing)
        {
            DepossessFromEnemy();
        }
    }

    void UnsubscribeAutoDepossessDialogueHandler()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.DialogueEnded -= HandlePossessionDialogueEnded;
        }
    }

    bool StartDialogueWhilePossessing(NPC npc)
    {
        if (npc == null)
        {
            return false;
        }

        if (rigid != null && rigid.TryGetComponent<Enemy>(out var controlledEnemy))
        {
            controlledEnemy.PrepareForPossessionDialogue();
        }

        transform.position = rigid.GetComponent<Transform>().position;
        canMove = false;
        rigid.linearVelocity = Vector3.zero;
        Debug.Log("대화시작");
        return npc.Talk();
    }

    public Vector2 GetPossessedEnemyPosition()
    {
        return rigid.transform.position;
    }

    //E키 상호작용
    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;
        
        Debug.Log("E키 입력 감지됨!");

        if (PasswordUIManager.IsUiOpen)
        {
            PasswordUIManager.Instance.Close();
            return;
        }
        else if(SaveSlotUIManager.Instance.isOpen)
        {
            SaveSlotUIManager.Instance.CloseSlotUI();
            return;
        }

        // E키가 눌렸고, 상호작용 가능한 객체가 있으며, 영혼 상태가 아닐 때만 작동
        if (nearbyInteractable != null /*&& !abilityManager.isSoul*/)
        {
            nearbyInteractable.Interact(this.gameObject);
            if (IsPossessing)
            {
                transform.position = rigid.GetComponent<Transform>().position;
            }
        }
        else
        {
            Enemy controlledEnemy = isPossessing && !isTalking ? rigid.GetComponent<Enemy>() : null;
            if (controlledEnemy == null || controlledEnemy.nearbyEnemy == null)
            {
                canInteractUI.hideInterectUI();
                rigid.linearVelocity = Vector2.zero;
                return;
            }

            StartDialogueWhilePossessing(controlledEnemy.nearbyEnemy.GetComponent<NPC>());
        }
        canInteractUI.hideInterectUI();
        rigid.linearVelocity = Vector2.zero;
    }

    // 트리거 감지 로직
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 빙의 대상 감지 로직
        if (IsSoulForm() && abilityManager.canPossess)
        {
            if (collision.TryGetComponent<Enemy>(out var enemy))
            {
                targetEnemyToPossess = enemy;
                canInteractUI.showInterectUI(collision.transform, "v", "빙의");
            }
        }

        touchInteractable(collision);

        if (collision.CompareTag("Wall")) insideWall++;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 빙의 대상 해제 로직
        if (IsSoulForm())
        {
            if (collision.TryGetComponent<Enemy>(out var enemy))
            {
                if (targetEnemyToPossess == enemy)
                {
                    targetEnemyToPossess = null;
                }
                canInteractUI.hideInterectUI();
            }
        }

        fallFromInteractable(collision);

        if (collision.CompareTag("Wall")) insideWall--;
        insideWall = Math.Clamp(insideWall, 0, 10);
    }

    //상호작용 함수
    public void touchInteractable(Collider2D collision)
    {
        if (collision.TryGetComponent<IInteractable>(out var interactable))
        {
            if (abilityManager.isSoul && !collision.CompareTag("Document") && !collision.CompareTag("Parts")) return;

            nearbyInteractable = interactable;
            canInteractUI.showInterectUI(collision.transform, "e", interactable.Purpose);
        }
    }

    public void fallFromInteractable(Collider2D collision)
    {
        // 상호작용 객체 해제 로직
        if (collision.TryGetComponent<IInteractable>(out var interactable))
        {
            // 방금 벗어난 객체가 내가 타겟팅하던 객체라면 초기화
            if (nearbyInteractable == interactable)
            {
                nearbyInteractable = null;
                canInteractUI.hideInterectUI();
            }
        }
    }

    public void canTalk(Transform hit)
    {
        canInteractUI.showInterectUI(hit.transform, "e", "대화");
        nearbyInteractable = null;
    }

    public void StopMovement()
    {
        if (rigid != null)
        {
            if (abilityManager != null && IsSoulForm())
            {
                rigid.linearVelocity = Vector2.zero;
            }
            else
            {
                rigid.linearVelocity = new Vector2(0f, rigid.linearVelocity.y);
            }
        }
    }

    void UpdateAnimationState()
    {
        if (visualManager == null || rigid == null || abilityManager == null)
        {
            return;
        }

        float animationSpeed = canMove && moveInput.sqrMagnitude > 0.01f ? 1f : 0f;

        bool isGrounded = !IsSoulForm() && isWallAttatching;

        visualManager.UpdateAnimationState(
            animationSpeed,
            isGrounded,
            rigid.linearVelocity.y,
            isDashing,
            isWallClimbing,
            IsSoulForm());
    }

    void UpdateFacingVisual()
    {
        if (visualManager == null)
        {
            return;
        }

        visualManager.UpdateFacingDirection(facingDirection);
    }

    void HandleProgressionStateChanged()
    {
        UpdateFormState();
        UpdateFacingVisual();
        UpdateAnimationState();
    }

    bool IsSoulForm()
    {
        if (progressionManager != null)
        {
            return progressionManager.EffectiveIsSoul;
        }

        return abilityManager != null && abilityManager.isSoul;
    }

    void SubscribeToProgressionState()
    {
        if (isSubscribedToProgressionState)
        {
            return;
        }

        if (progressionManager == null)
        {
            progressionManager = GetComponent<PlayerProgressionManager>();
        }

        if (progressionManager == null)
        {
            return;
        }

        progressionManager.StateChanged += HandleProgressionStateChanged;
        isSubscribedToProgressionState = true;
    }

    void UnsubscribeFromProgressionState()
    {
        if (!isSubscribedToProgressionState || progressionManager == null)
        {
            return;
        }

        progressionManager.StateChanged -= HandleProgressionStateChanged;
        isSubscribedToProgressionState = false;
    }

    public Rigidbody2D CurrentRigidbody => rigid;
    public Collider2D CurrentCollider => GetActiveControlledCollider();
    public PlayerAbilityManager AbilityManager => abilityManager;
    public float FacingDirection => facingDirection;
    public bool IsUiOpen => isUIopen;
    public bool IsPossessing => isPossessing;
    public bool IsDashing => isDashing;
    public bool IsAttackInProgress => combatManager != null && combatManager.IsAttacking;

    public Vector2 FilterVelocityForBounds(Vector2 desiredVelocity)
    {
        return FilterVelocityAgainstBounds(desiredVelocity);
    }
}
