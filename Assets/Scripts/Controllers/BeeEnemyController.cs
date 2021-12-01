using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BeeEnemyController : Singleton<BeeEnemyController>
{
    [SerializeField]
    Stinger stingerPrefab;

    ObjectPool<Stinger> stingerPool;
    List<Bee> bees;
    List<Hive> hives;
    public int TotalBees { get { return bees.Count + hives.Count; } }

    public List<string> BeesKilled { get; set; }

    public delegate void BeeAction(Bee bee);
    BeeAction OnBeeKilledDelegates;

    private void Start()
    {
        bees = FindObjectsOfType<Bee>().ToList();
        hives = FindObjectsOfType<Hive>().ToList();

        if (BeesKilled == null)
            BeesKilled = new List<string>();
        stingerPool = new ObjectPool<Stinger>(CreateStinger, OnStingerGet, OnStingerRelease);
    }

    public void RegisterOnBeeKilled(BeeAction action) => OnBeeKilledDelegates += action;
    public void UnregisterOnBeeKilled(BeeAction action) => OnBeeKilledDelegates -= action;

    public void OnLevelStartFromCheckpoint(Checkpoint checkpoint)
    {
        var point = checkpoint.transform.position;

        // This is dirty solution seeing that we assume the direction we move is always positive on X
        // but it is the quickest solution to goal at this time
        foreach (var bee in bees)
        {
            if (bee.transform.position.x < point.x)
                bee.DestroyBee();
        }

        // Grab remaining ones since we removed some
        bees = FindObjectsOfType<Bee>().ToList();

        // Remove the ones already killed that were not removed 
        // Since their position is after the checkpoint
        foreach (var bee in bees)
            if (BeesKilled.Contains(bee.ID))
                bee.DestroyBee();
    }

    public void OnBeeKilled(Bee bee)
    {
        if (!BeesKilled.Contains(bee.ID))
        {
            bees.Remove(bee);
            BeesKilled.Add(bee.ID);
            GameManager.instance.BeesKilled++;            
            OnBeeKilledDelegates?.Invoke(bee);
        }
    }

    Stinger CreateStinger()
    {
        var stinger = Instantiate(stingerPrefab, transform).GetComponent<Stinger>();
        stinger.transform.forward = transform.forward;
        stinger.transform.localScale = Vector3.one;

        return stinger;
    }

    void OnStingerGet(Stinger stinger) => stinger.SetState(true);
    void OnStingerRelease(Stinger stinger) => stinger.SetState(false);
    public Stinger GetStinger(Transform spawnXForm) 
    { 
        var stinger = stingerPool.Get();
        stinger.transform.position = spawnXForm.position;
        return stinger;
    }
    public void Release(Stinger stinger) => stingerPool.Release(stinger);
}
