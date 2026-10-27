using UnityEngine;

public enum AssignedInput
{
    CPU,            // Default when side is left blank
    Gamepad,
    KeyboardWASD,
    KeyboardArrows
}

public static class MatchAllocation
{
    public static AssignedInput BlueSide = AssignedInput.CPU;
    public static AssignedInput RedSide = AssignedInput.CPU;
}

public class TeamInputSwitcher : MonoBehaviour
{
    public bool isBlueTeam = true;

    [Header("Drag the 3 Scripts on this Team Here")]
    public TeamGamepadController gamepadScript;
    public TeamKeyboardController keyboardScript;
    public TeamAIController aiScript;

    void Start()
    {
        AssignedInput chosenInput = isBlueTeam ? MatchAllocation.BlueSide : MatchAllocation.RedSide;
        ApplyInput(chosenInput);
    }

    public void ApplyInput(AssignedInput input)
    {
        // First disable all 3 so they don't fight over the rods
        if (gamepadScript != null) gamepadScript.enabled = false;
        if (keyboardScript != null) keyboardScript.enabled = false;
        if (aiScript != null) aiScript.enabled = false;

        // Enable only the assigned one
        switch (input)
        {
            case AssignedInput.Gamepad:
                if (gamepadScript != null) gamepadScript.enabled = true;
                break;

            case AssignedInput.KeyboardWASD:
                if (keyboardScript != null)
                {
                    keyboardScript.layout = TeamKeyboardController.KeyboardLayout.WASD;
                    keyboardScript.enabled = true;
                }
                break;

            case AssignedInput.KeyboardArrows:
                if (keyboardScript != null)
                {
                    keyboardScript.layout = TeamKeyboardController.KeyboardLayout.Arrows;
                    keyboardScript.enabled = true;
                }
                break;

            case AssignedInput.CPU:
            default:
                if (aiScript != null) aiScript.enabled = true;
                break;
        }
    }
}