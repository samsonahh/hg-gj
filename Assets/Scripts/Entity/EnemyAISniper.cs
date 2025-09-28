using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAISniper : EnemyAIBase
{
    protected override void Awake()
    {
        base.Awake();
        if (agent != null)
        {
            agent.speed = 0f;
            agent.isStopped = true;
        }
    }

    protected override void Update()
    {
        if (agent == null)
            return;

        RefreshTargetIfNeeded();
        SenseTarget();
        UpdateTimers();
        StateLoop(); // Use the base class state machine for all logic
    }

    // Prevent any movement logic from base
    protected override void MoveToTarget() { }
    protected override void Patrol() { }
}