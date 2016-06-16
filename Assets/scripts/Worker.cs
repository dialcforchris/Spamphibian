﻿using UnityEngine;
using System.Collections;

public class Worker : WorldObject, IPoolable<Worker>
{
    #region IPoolable
    public PoolData<Worker> poolData { get; set; }
    #endregion
    private void Awake()
    {

    }
    public void Initialise()
    {
        gameObject.SetActive(true);
    }
    //The behavior of an object when something tries to interact with it
    public override void Interaction(WorldObject _obj)
    {

    }

    //Whether an object can move to the sam eposition as another object
    public override bool CheckMovement(WorldObject _obj)
    {
        return true;
    }

    public void Reset()
    {
        poolData.ReturnPool(this);
        gameObject.SetActive(false);
    }
}