﻿using UnityEngine;
using System.Collections;

public class Player : WorldObject
{
    float angle;
    float coolDown = 0.6f;
    float maxCool = 0.4f;
    private Vector2 lastPos = Vector2.zero;
    //private members
    private int strikes = 3;
    private int score = 0;
    private float hori;
    private float verti;

    [SerializeField] private Animator anim = null;
    private PlayerState state = PlayerState.ACTIVE;

    float deathCool = 0;
    float maxDeathcool = 3;

    //public members
    public int Score
    {
        get { return score; }
        set { score = value; }
    }
    public int Strikes
    {
        get { return strikes; }
        set { strikes = value; }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == PlayerState.ACTIVE)
        {
            ConvertToPos();
            Movement();
            MoveCooldown();
        }
        DeathCooler();
    }

    void Movement()
    {
        float moveX = 0;
        float moveY = 0;

        if (hori > verti)
        {
            if (Input.GetAxis("Horizontal") != 0 && MoveCooldown())
            {
                moveX = Input.GetAxis("Horizontal") > 0 ? 1 : -1;
                if (lastPos.x != moveX)
                {
                    Tile _tile = TileManager.instance.GetTile(new Vector2((transform.position.x + moveX), transform.position.y));
                    if (_tile.CheckMovement(this))
                    {
                        transform.position = _tile.transform.position;
                        RemoveFromWorld();
                        AddToWorld();
                        _tile.Interaction(this);
                    }

                 //   SoundManager.instance.playSound(0);
                    angle = Input.GetAxis("Horizontal") > 0 ? 270 : 90;
                    coolDown = 0;
                    lastPos.x = moveX;
                }
            }
        }
        else
        {
            if (Input.GetAxis("Vertical") != 0 && MoveCooldown())
            {
                moveY = Input.GetAxis("Vertical") > 0 ? 1 : -1;
                if (lastPos.y != moveY)
                {
                    Tile _tile = TileManager.instance.GetTile(new Vector2(transform.position.x, (transform.position.y + moveY)));
                    if (_tile.CheckMovement(this))
                    {
                        transform.position = _tile.transform.position;
                        RemoveFromWorld();
                        AddToWorld();
                        _tile.Interaction(this);
                    }
                 //   SoundManager.instance.playSound(0);
                    angle = Input.GetAxis("Vertical") > 0 ? 0 : 180;
                    coolDown = 0;
                    lastPos.y = moveY;
                }
            }
        }
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        if (Input.GetAxis("Horizontal") == 0)
        {
            lastPos.x = 0;
        }
        if (Input.GetAxis("Vertical") == 0)
        {
            lastPos.y = 0;
        }

        anim.SetBool("PlayerWalk", ((Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0) && !MoveCooldown()) ? true : false);

    }

    bool MoveCooldown()
    {
        if (coolDown < maxCool)
        {
            coolDown += Time.deltaTime;
            return false;
        }
        return true;
    }

    //movement related function
    void ConvertToPos()
    {
        hori = Input.GetAxis("Horizontal");
        verti = Input.GetAxis("Vertical");
        if (hori < 0)
        {
            hori -= hori * 2;
        }
        if (verti < 0)
        {
            verti -= verti * 2;
        }
    }

    public void Die()
    {
        state = PlayerState.DEAD;
        anim.SetBool("Dead", true);
        //Rather than this leave behind a corpse call remove from world, move position then add to world immediately
        strikes -= 1;
    }

    public override void Interaction(WorldObject _obj)
    {
        if (_obj.tag == "Worker")
        {
            Die();
        }
    }

    void DeathCooler()
    {
        if (state == PlayerState.DEAD)
        {
            if (deathCool < maxDeathcool)
            {
                deathCool += Time.deltaTime;
            }
            else
            {
                anim.SetBool("Dead", false);
                state = PlayerState.ACTIVE;
                deathCool = 0;
                transform.position = new Vector2(-0.5f, -1.0f);
                angle = 0;
                RemoveFromWorld();
                AddToWorld();
            }
        }
    }
}


enum PlayerState
{
    ACTIVE,
    DEAD,
}
