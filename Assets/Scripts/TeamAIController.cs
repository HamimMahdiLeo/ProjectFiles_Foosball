using System.Collections.Generic;
using UnityEngine;

public class TeamAIController : MonoBehaviour
{
    [Header("Assign References")]
    public FoosballRod[] rods;
    public Transform ball;
    private Rigidbody ballRb;

    [Header("Movement Settings")]
    public float slideSpeed = 2.4f;
    public float predictionTime = 0.14f;
    public float deadzone = 0.012f;

    [Header("Kick & Rotation Settings")]
    public float maxAngle = 80f;
    public float snapSpeed = 35f;
    public float kickRange = 0.18f;
    public float windUpRange = 0.35f;
    [Tooltip("Set to 1 for Blue Team, -1 for Red Team")]
    public float kickDirection = -1f;

    private List<Transform>[] rodPlayers;
    private float[] kickTimers;
    private float[] cooldownTimers;
    private bool[] isKicking;

    // Smooth wander offset that updates slowly (never per-frame!) so rods don't vibrate
    private float aimOffset;
    private float offsetTimer;

    void Start()
    {
        if (ball == null)
        {
            BallBooster foundBall = FindAnyObjectByType<BallBooster>();
            if (foundBall != null) ball = foundBall.transform;
        }

        if (ball != null)
        {
            ballRb = ball.GetComponent<Rigidbody>();
        }

        if (rods != null)
        {
            rodPlayers = new List<Transform>[rods.Length];
            kickTimers = new float[rods.Length];
            cooldownTimers = new float[rods.Length];
            isKicking = new bool[rods.Length];

            for (int i = 0; i < rods.Length; i++)
            {
                rodPlayers[i] = new List<Transform>();
                if (rods[i] == null) continue;

                Collider[] colliders = rods[i].GetComponentsInChildren<Collider>();
                foreach (Collider col in colliders)
                {
                    if (col.transform != rods[i].transform)
                    {
                        rodPlayers[i].Add(col.transform);
                    }
                }

                if (rodPlayers[i].Count == 0)
                {
                    foreach (Transform child in rods[i].transform)
                    {
                        rodPlayers[i].Add(child);
                    }
                }
            }
        }
    }

    void Update()
    {
        if (ball == null || rods == null || rods.Length == 0) return;

        // Change slight aim variation only twice a second (prevents frame-by-frame jitter!)
        offsetTimer -= Time.deltaTime;
        if (offsetTimer <= 0f)
        {
            aimOffset = Random.Range(-0.025f, 0.025f);
            offsetTimer = 0.5f;
        }

        Vector3 ballPos = ball.position;
        Vector3 ballVel = ballRb != null ? ballRb.linearVelocity : Vector3.zero;
        float ballSpeed = ballVel.magnitude;

        for (int i = 0; i < rods.Length; i++)
        {
            if (rods[i] == null) continue;

            cooldownTimers[i] -= Time.deltaTime;

            Vector3 rodPos = rods[i].transform.position;
            float zDiff = ballPos.z - rodPos.z;
            float zDist = Mathf.Abs(zDiff);
            bool ballMovingTowardRod = Mathf.Abs(ballVel.z) > 0.1f && Mathf.Sign(ballVel.z) != Mathf.Sign(zDiff);

            // --- 1. TRAJECTORY PREDICTION (SLIDE) ---
            float targetX = ballPos.x;

            if (ballMovingTowardRod)
            {
                // Predict exact X where the ball will cross this rod's Z line
                float timeToRod = Mathf.Clamp(Mathf.Abs(zDiff / ballVel.z), 0f, 0.45f);
                targetX = ballPos.x + ballVel.x * timeToRod;
            }
            else
            {
                targetX = ballPos.x + ballVel.x * predictionTime;
            }

            targetX += aimOffset;

            // Keep Goalie/Defense rod (rod 0) closer to the center of the goal when ball is far away
            if (i == 0 && zDist > 0.8f)
            {
                targetX = Mathf.Lerp(targetX, 0f, 0.45f);
            }

            // Pick the closest player figure on this rod to intercept targetX
            float closestPlayerX = rodPos.x;
            if (rodPlayers != null && rodPlayers[i].Count > 0)
            {
                float minDist = float.MaxValue;
                foreach (Transform player in rodPlayers[i])
                {
                    if (player == null) continue;

                    float distToTarget = Mathf.Abs(targetX - player.position.x);
                    if (distToTarget < minDist)
                    {
                        minDist = distToTarget;
                        closestPlayerX = player.position.x;
                    }
                }
            }

            float xDiff = targetX - closestPlayerX;
            float lsY = 0f;

            if (Mathf.Abs(xDiff) > deadzone)
            {
                lsY = Mathf.Clamp(xDiff * 9f, -1f, 1f);
            }

            // --- 2. KICK & DEFENSIVE BLOCK LOGIC ---
            float xDistToPlayer = Mathf.Abs(ballPos.x - closestPlayerX);
            float rsX = 0f;

            if (isKicking[i])
            {
                kickTimers[i] += Time.deltaTime;

                if (kickTimers[i] < 0.11f)
                {
                    // Full-power forward kick snap
                    rsX = kickDirection;
                }
                else if (kickTimers[i] < 0.19f)
                {
                    // Quick reset back
                    rsX = -kickDirection * 0.35f;
                }
                else
                {
                    isKicking[i] = false;
                    kickTimers[i] = 0f;
                    rsX = 0f;
                }
            }
            else if (zDist < kickRange && xDistToPlayer < 0.22f && cooldownTimers[i] <= 0f)
            {
                // Trigger kick!
                isKicking[i] = true;
                kickTimers[i] = 0f;
                cooldownTimers[i] = 0.22f;
                rsX = kickDirection;

                // If ball is stalled right under the rod, nudge rod sideways to unstick it
                if (ballSpeed < 0.15f)
                {
                    lsY = (ballPos.x >= 0f ? -1f : 1f) * 0.7f;
                }
            }
            else if (ballMovingTowardRod && ballSpeed > 1.6f && zDist < windUpRange * 1.4f)
            {
                // FAST INCOMING SHOT: Keep feet flat/braced to block! Do NOT lift feet up!
                rsX = kickDirection * 0.15f;
            }
            else if (zDist < windUpRange && xDistToPlayer < 0.30f && ballSpeed < 1.6f)
            {
                // Slow/controllable ball nearby: slight wind-up to prepare a stronger shot
                rsX = -kickDirection * 0.4f;
            }
            else
            {
                // Neutral flat-footed stance
                rsX = 0f;
            }

            rods[i].MoveAndRotate(lsY, rsX, slideSpeed, maxAngle, snapSpeed);
        }
    }
}