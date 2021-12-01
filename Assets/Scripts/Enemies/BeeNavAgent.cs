using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BeeNavAgent : MonoBehaviour
{
    [SerializeField]
    NavMeshAgent agent;
    NavMeshAgent Agent
    {
        get
        {
            if (agent == null)
                agent = GetComponent<NavMeshAgent>();
            return agent;
        }
    }

    public void Move(Vector3 destination)
    {
        // We will always ignore "y" 
        destination.y = 0;
        Agent.isStopped = false;
        Agent.SetDestination(destination);
    }

    public void Stop()
    {
        Agent.isStopped = true;
    }
}
