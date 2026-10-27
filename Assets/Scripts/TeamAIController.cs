using System.Collections.Generic;
using UnityEngine;

public class TeamAIController : MonoBehaviour
{
    [Header("Assign References")]
    public FoosballRod[] rods;
    public Transform ball;
    private Rigidbody ballRb;

    [Header("Movement Settings")]
    public float slideSpeed = 2.0f;
    public float predictionTime = 0.08f;
    public float deadzone = 0.015f;

    [Header("Kick & Rotation Settings")]
    public float maxAngle = 80f;
    public float snapSpeed = 30f;
    public float kickRange = 0.16f;
    public float windUpRange = 0.35f;
    [Tooltip("Set to 1 for Blue Team, -1 for Red Team")]
    public float kickDirection = -1f;

    private List<Transform>[] rodPlayers;
    private float[] kickTimers;

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

        Vector3 predictedBallPos = ball.position;
        if (ballRb != null)
        {
            predictedBallPos += ballRb.linearVelocity * predictionTime;
        }

        for (int i = 0; i < rods.Length; i++)
        {
            if (rods[i] == null) continue;

            // --- 1. SLIDE LOGIC ---
            float targetX = predictedBallPos.x;
            float closestPlayerX = rods[i].transform.position.x;

            if (rodPlayers != null && rodPlayers[i].Count > 0)
            {
                float minDist = float.MaxValue;
                foreach (Transform player in rodPlayers[i])
                {
                    if (player == null) continue;
                    float dist = Mathf.Abs(targetX - player.position.x);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestPlayerX = player.position.x;
                    }
                }
            }

            float xDiff = targetX - closestPlayerX;
            float lsY = 0f;

            if (Mathf.Abs(xDiff) > deadzone)
            {
                lsY = Mathf.Clamp(xDiff * 6f, -1f, 1f);
            }

            // --- 2. ROTATION / KICK LOGIC ---
            float zDiff = ball.position.z - rods[i].transform.position.z;
            float zDist = Mathf.Abs(zDiff);
            float xDistToPlayer = Mathf.Abs(ball.position.x - closestPlayerX);

            float rsX = 0f;

            if (zDist < kickRange && xDistToPlayer < 0.22f)
            {
                kickTimers[i] += Time.deltaTime * 16f;
                rsX = Mathf.Sin(kickTimers[i]) > -0.2f ? kickDirection : -kickDirection * 0.6f;
            }
            else if (zDist < windUpRange)
            {
                kickTimers[i] = 0f;
                rsX = -kickDirection * 0.45f;
            }
            else
            {
                kickTimers[i] = 0f;
                rsX = 0f;
            }

            rods[i].MoveAndRotate(lsY, rsX, slideSpeed, maxAngle, snapSpeed);
        }
    }
}