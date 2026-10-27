using UnityEngine;
using UnityEngine.InputSystem;

public class TeamKeyboardController : MonoBehaviour
{
    public enum KeyboardLayout
    {
        WASD,   // W/S = Slide, A/D = Kick, Q/E or 1-4 = Switch Rod
        Arrows  // Up/Down = Slide, Left/Right = Kick, R-Shift/R-Ctrl or Numpad 1-4 = Switch Rod
    }

    [Header("Keyboard Mode")]
    public KeyboardLayout layout = KeyboardLayout.WASD;

    [Header("Team Rods (0: GK, 1: Def, 2: Mid, 3: Att)")]
    public FoosballRod[] rods;
    public int activeRod = 0;

    [Header("Movement Settings")]
    public float slideSpeed = 2.5f;
    public float maxAngle = 80f;
    public float snapSpeed = 30f;

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || rods == null || rods.Length == 0) return;

        float slide = 0f;
        float kick = 0f;

        if (layout == KeyboardLayout.WASD)
        {
            // Switch Rods (Q / E or Top Row 1-4)
            if (kb.qKey.wasPressedThisFrame)
                activeRod = Mathf.Clamp(activeRod + 1, 0, rods.Length - 1);
            if (kb.eKey.wasPressedThisFrame)
                activeRod = Mathf.Clamp(activeRod - 1, 0, rods.Length - 1);

            if (kb.digit1Key.wasPressedThisFrame) activeRod = 0;
            if (kb.digit2Key.wasPressedThisFrame && rods.Length > 1) activeRod = 1;
            if (kb.digit3Key.wasPressedThisFrame && rods.Length > 2) activeRod = 2;
            if (kb.digit4Key.wasPressedThisFrame && rods.Length > 3) activeRod = 3;

            // Slide (W / S)
            if (kb.wKey.isPressed) slide = 1f;
            else if (kb.sKey.isPressed) slide = -1f;

            // Kick (A / D)
            if (kb.aKey.isPressed) kick = -1f;
            else if (kb.dKey.isPressed) kick = 1f;
        }
        else if (layout == KeyboardLayout.Arrows)
        {
            // Switch Rods (Comma/Period or RightShift/RightCtrl or Numpad 1-4)
            if (kb.commaKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame)
                activeRod = Mathf.Clamp(activeRod + 1, 0, rods.Length - 1);
            if (kb.periodKey.wasPressedThisFrame || kb.rightCtrlKey.wasPressedThisFrame)
                activeRod = Mathf.Clamp(activeRod - 1, 0, rods.Length - 1);

            if (kb.numpad1Key.wasPressedThisFrame) activeRod = 0;
            if (kb.numpad2Key.wasPressedThisFrame && rods.Length > 1) activeRod = 1;
            if (kb.numpad3Key.wasPressedThisFrame && rods.Length > 2) activeRod = 2;
            if (kb.numpad4Key.wasPressedThisFrame && rods.Length > 3) activeRod = 3;

            // Slide (Up / Down Arrows)
            if (kb.upArrowKey.isPressed) slide = 1f;
            else if (kb.downArrowKey.isPressed) slide = -1f;

            // Kick (Left / Right Arrows)
            if (kb.leftArrowKey.isPressed) kick = -1f;
            else if (kb.rightArrowKey.isPressed) kick = 1f;
        }

        // Apply movement to active rod, reset unselected rods to 0
        for (int i = 0; i < rods.Length; i++)
        {
            if (rods[i] == null) continue;

            if (i == activeRod)
                rods[i].MoveAndRotate(slide, kick, slideSpeed, maxAngle, snapSpeed);
            else
                rods[i].MoveAndRotate(0f, 0f, slideSpeed, maxAngle, snapSpeed);
        }
    }
}