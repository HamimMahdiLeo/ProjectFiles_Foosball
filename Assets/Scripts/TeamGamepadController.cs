using UnityEngine;
using UnityEngine.InputSystem;

public class TeamGamepadController : MonoBehaviour
{
    [Header("Team Rods (0: GK, 1: Def, 2: Mid, 3: Att)")]
    public FoosballRod[] rods;
    public int activeRod = 0;

    [Header("Movement Settings")]
    public float slideSpeed = 2.5f;
    public float maxAngle = 80f;
    public float snapSpeed = 25f;
    public float deadzone = 0.2f;

    void Update()
    {
        Gamepad pad = Gamepad.current;
        if (pad == null || rods == null || rods.Length == 0) return;

        // Switch rod with LB and RB
        if (pad.rightShoulder.wasPressedThisFrame)
            activeRod = Mathf.Min(activeRod + 1, rods.Length - 1);
        if (pad.leftShoulder.wasPressedThisFrame)
            activeRod = Mathf.Max(activeRod - 1, 0);

        // Read sticks & ignore drift inside deadzone
        float lsY = pad.leftStick.ReadValue().y;
        float rsX = pad.rightStick.ReadValue().x;

        if (Mathf.Abs(lsY) < deadzone) lsY = 0f;
        if (Mathf.Abs(rsX) < deadzone) rsX = 0f;

        // Move active rod, reset others to upright (0)
        for (int i = 0; i < rods.Length; i++)
        {
            if (rods[i] == null) continue;

            if (i == activeRod)
                rods[i].MoveAndRotate(lsY, rsX, slideSpeed, maxAngle, snapSpeed);
            else
                rods[i].MoveAndRotate(0f, 0f, slideSpeed, maxAngle, snapSpeed);
        }
    }
}