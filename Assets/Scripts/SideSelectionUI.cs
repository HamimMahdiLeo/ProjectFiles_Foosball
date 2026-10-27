using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class SideSelectionUI : MonoBehaviour
{
    [Header("Device Badges (UI Transforms)")]
    public RectTransform gamepadBadge;
    public RectTransform wasdBadge;
    public RectTransform arrowsBadge;

    [Header("Device Text Labels")]
    public TMP_Text gamepadText;
    public TMP_Text wasdText;
    public TMP_Text arrowsText;

    [Header("Column X Positions")]
    public float blueX = -300f;
    public float centerX = 0f;
    public float redX = 300f;

    // -1 = Blue Team, 0 = Center (CPU), 1 = Red Team
    private int gamepadSide = 0;
    private int wasdSide = 0;
    private int arrowsSide = 0;
    private bool stickInUse = false;

    void Start()
    {
        if (Gamepad.current != null)
            gamepadText.text = Gamepad.current.name;
        else
            gamepadText.text = "Controller (Disconnected)";

        wasdText.text = "Keyboard [WASD]";
        arrowsText.text = "Keyboard [Arrows]";
    }

    void Update()
    {
        var kb = Keyboard.current;
        var pad = Gamepad.current;

        // 1. Keyboard WASD (A = Left, D = Right)
        if (kb != null)
        {
            if (kb.aKey.wasPressedThisFrame) MoveBadge(ref wasdSide, -1);
            if (kb.dKey.wasPressedThisFrame) MoveBadge(ref wasdSide, 1);

            // 2. Keyboard Arrows (LeftArrow = Left, RightArrow = Right)
            if (kb.leftArrowKey.wasPressedThisFrame) MoveBadge(ref arrowsSide, -1);
            if (kb.rightArrowKey.wasPressedThisFrame) MoveBadge(ref arrowsSide, 1);

            if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame) ConfirmAndStart();
        }

        // 3. Controller / Gamepad (D-Pad or Left Stick Left/Right)
        if (pad != null)
        {
            float stickX = pad.leftStick.ReadValue().x;
            bool lHit = (stickX < -0.5f && !stickInUse) || pad.dpad.left.wasPressedThisFrame;
            bool rHit = (stickX > 0.5f && !stickInUse) || pad.dpad.right.wasPressedThisFrame;

            if (Mathf.Abs(stickX) > 0.5f) stickInUse = true;
            else if (Mathf.Abs(stickX) < 0.2f) stickInUse = false;

            if (lHit) MoveBadge(ref gamepadSide, -1);
            if (rHit) MoveBadge(ref gamepadSide, 1);

            if (pad.startButton.wasPressedThisFrame || pad.buttonSouth.wasPressedThisFrame) ConfirmAndStart();
        }

        // Smoothly animate badges to columns
        AnimateBadge(gamepadBadge, gamepadSide);
        AnimateBadge(wasdBadge, wasdSide);
        AnimateBadge(arrowsBadge, arrowsSide);
    }

    void MoveBadge(ref int side, int dir)
    {
        int target = Mathf.Clamp(side + dir, -1, 1);
        // Prevent two devices from taking the exact same team slot (Center 0 can hold multiple)
        if (target != 0 && (gamepadSide == target || wasdSide == target || arrowsSide == target)) return;
        side = target;
    }

    void AnimateBadge(RectTransform badge, int side)
    {
        if (badge == null) return;
        float targetX = side == -1 ? blueX : (side == 1 ? redX : centerX);
        Vector2 pos = badge.anchoredPosition;
        pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * 15f);
        badge.anchoredPosition = pos;
    }

    // Hook this method up to your "START MATCH" UI Button OnClick() event
    public void ConfirmAndStart()
    {
        MatchAllocation.BlueSide = AssignedInput.CPU;
        MatchAllocation.RedSide = AssignedInput.CPU;

        // Assign Blue Team (-1)
        if (gamepadSide == -1) MatchAllocation.BlueSide = AssignedInput.Gamepad;
        else if (wasdSide == -1) MatchAllocation.BlueSide = AssignedInput.KeyboardWASD;
        else if (arrowsSide == -1) MatchAllocation.BlueSide = AssignedInput.KeyboardArrows;

        // Assign Red Team (1)
        if (gamepadSide == 1) MatchAllocation.RedSide = AssignedInput.Gamepad;
        else if (wasdSide == 1) MatchAllocation.RedSide = AssignedInput.KeyboardWASD;
        else if (arrowsSide == 1) MatchAllocation.RedSide = AssignedInput.KeyboardArrows;

        SceneManager.LoadScene("Game");
    }
}