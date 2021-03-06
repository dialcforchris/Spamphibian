﻿using UnityEngine;
using System.Collections.Generic;

public class Cubicle : WorldObject
{
    private enum Positions
    {
        OPENING = 0,
        TABLE,
        EMPTY,
        CHAIR,
        COUNT
    }

    public SpriteRenderer deskFodder;
    public Sprite[] tidyDesk;
    public Sprite[] messyDesk;

    private int currentDesk;

    [SerializeField]
    private Transform opening = null;
    [SerializeField]
    private Transform table = null;
    [SerializeField]
    private Transform empty = null;
    [SerializeField]
    private Transform[] chairs = null;
    [SerializeField]
    private Transform[] chairPivots = null;
    //public bool[] filledChairs;

    private int deskId;
    public int cubicleId
    {
        get { return deskId; }
        set { deskId = value; }
    }

    [SerializeField] private Spawner spawner = null;
    public Spawner GetAssociatedSpawner() { return spawner; }

    private bool isMessy = false;

    // Use this for initialization
    protected override void Awake()
    {
        //filledChairs = new bool[chairs.Length];
        //for (int i = 0; i < filledChairs.Length; i++)
        //{
        //    filledChairs[i] = false;
        //}

        tiles = new Tile[3 + chairs.Length];
        currentDesk = Random.Range(0, tidyDesk.Length);
        deskFodder.sprite = tidyDesk[currentDesk];

    }

    protected override void Start()
    {
        //if (transform.parent)
        //{
        //    Cubicle _o = (Cubicle)Instantiate(TileManager.instance.c, transform.position, transform.rotation);
        //    _o.name = "2X2Cubicle" + TileManager.instance.GetTile(transform.position).IndexName();
        //}
        Tile _tile = TileManager.instance.GetTile(opening.position);
        _tile.Place(this);
        tiles[(int)Positions.OPENING] = _tile;

        _tile = TileManager.instance.GetTile(table.position);
        _tile.Place(this);
        tiles[(int)Positions.TABLE] = _tile;

        _tile = TileManager.instance.GetTile(empty.position);
        _tile.Place(this);
        tiles[(int)Positions.EMPTY] = _tile;

        for (int i = 0; i < chairs.Length; ++i)
        {
            _tile = TileManager.instance.GetTile(chairs[i].position);
            _tile.Place(this);
            tiles[(int)Positions.CHAIR + i] = _tile;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int GetChairs()
    {
        return chairs.Length;
    }

    public void AssignWorkerPositionData(Worker _worker)
    {
        _worker.AssignCubiclePivots(opening.position, empty.position, chairs[_worker.chairId].position, chairPivots[_worker.chairId].position);
    }

    public void AssignWorkerImmediately(Worker _worker)
    {
        //filledChairs[_worker.chairId] = true;
        _worker.transform.position = chairPivots[_worker.chairId].position;
        _worker.RemoveFromWorld();
        _worker.AddToWorld();

        float lookAngle = Mathf.Atan2((chairs[_worker.chairId].position.y - chairPivots[_worker.chairId].position.y), 
            (chairs[_worker.chairId].position.x - chairPivots[_worker.chairId].position.x)) * Mathf.Rad2Deg;
        Quaternion newRot = new Quaternion();
        newRot.eulerAngles = new Vector3(0, 0, lookAngle + 90);
        _worker.transform.rotation = newRot;
    }

    //The behavior of an object when something tries to interact with it
    public override void Interaction(WorldObject _obj)
    {
        if (_obj.tag == "Worker")
        {
            if (_obj.GetTile(0) == tiles[(int)Positions.OPENING])
            {
                if (((Worker)_obj).cubicleId == deskId && !((Worker)_obj).hasEnteredCubicle)
                {
                    ((Worker)_obj).MoveToChair();
                }
            }
        }
        else if (_obj.tag == "Player") //if the player attempts to interact with a tile...
        {
            if (_obj.GetTile(0) == tiles[(int)Positions.TABLE]) //Is the frog on a table?
            {
                if (!isMessy) //If we haven't already, knock things off the desk
                {
                    SoundManager.instance.playSound(1);
                    StatTracker.instance.messyDesks[multiplayerManager.instance.currentActivePlayer]++;
                    isMessy = true;
                    deskFodder.sprite = messyDesk[currentDesk];
                }
               
                    Phishing();
            }
            else if (_obj.GetTile(0) == tiles[(int)Positions.EMPTY])
            {
                    Phishing();
            }
        }
    }

    private void Phishing()
    {
        for (int i = 0; i < chairs.Length; i++)
        {
            Tile _tile = TileManager.instance.GetTile(chairs[i].position);
            WorldObject[] onTile = _tile.GetObjects();

            foreach (WorldObject wo in onTile)
            {
                if (wo.tag == "Worker")
                {
                    if (((Worker)wo).helpNeeded && !GameStateManager.instance.bossTransitioning)
                    {
                        mailOpener.instance.enterView();
                        ((Worker)wo).FinishedHelping();
                        return;
                     }
                }
            }
        }
    }

    //Whether an object can move to the same position as another object
    public override bool CheckMovement(WorldObject _obj)
    {
        return true;
    }

    public List<Tile> GetTileRoute(Tile _tile)
    {
        List<Tile> _tiles = new List<Tile>();

        _tiles.Add(tiles[(int)Positions.OPENING]);
        if (_tile == tiles[(int)Positions.OPENING])
        {
            return _tiles;
        }

        _tiles.Add(tiles[(int)Positions.EMPTY]);
        if (_tile == tiles[(int)Positions.EMPTY])
        {
            return _tiles;
        }

        _tiles.Add(_tile);
        return _tiles;
    }

    public override void Reset()
    {
        isMessy = false;
        deskFodder.sprite = tidyDesk[currentDesk];
    }
}
