﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossFace : WorldObject

{
    public static BossFace instance = null;
    public Sprite[] faceList;
    public Sprite bossGone;
    public ParticleSystem steam;
    public GameObject eyes;
    public Player player;
    public Slider XPbar;
    float bossAngerAddition = 0.35f;
    float emailCool = 0;
    float emailMaxCool = 20;
    float playerCool = 0;
    float playerMaxCool= 1.5f;
    Tile playerTile;
    float bossAngerExp = 0;
    float XPtoAdd = 0;
    int bossAngerLevel = 0;
    FaceState faceState;

	// Use this for initialization
	protected override void Awake () 
    {
        if (instance==null)
        {
            instance = this;
        }
        ChangeState(FaceState.UI);
        base.Awake();
	}
	
    public void addEmailAngerXP()
    {
        XPtoAdd += (emailCool/emailMaxCool)*bossAngerAddition;
    }

	// Update is called once per frame
	void Update () 
    {
        if (XPtoAdd > 0 && GameStateManager.instance.GetState() == GameStates.STATE_GAMEPLAY)
        {
            XPtoAdd -= 0.01f;
            bossAngerExp += 0.01f;
        }
        XPbar.value = bossAngerExp;
        MoveEyes();
        ManyFacedBoss();
        AddToAnger();
        CoolDown();
        SteamParticles();
	}

    void CoolDown()
    {
        if (GameStateManager.instance.GetState() == GameStates.STATE_EMAIL)
        {
            playerCool = 0;
            if (emailCool< emailMaxCool)
            {
                if (mailOpener.instance.activeCountdown)
                {
                    emailCool += Time.deltaTime;
                    mailOpener.instance.angerMeter.value = emailCool;
                    ColorBlock cols = mailOpener.instance.angerMeter.colors;
                    cols.disabledColor = Color.Lerp(Color.green, Color.red, (emailCool / (emailMaxCool*.75f)));
                    mailOpener.instance.angerMeter.colors = cols;
                }
            }
        }
        else if (GameStateManager.instance.GetState() == GameStates.STATE_GAMEPLAY&&player.playerState==PlayerState.ACTIVE)
        {
            emailCool = 0;
            if (playerCool < playerMaxCool)
            {
                if (playerCool == 0)
                {
                    playerTile = TileManager.instance.GetTile(player.transform.position);
                }
                playerCool += Time.deltaTime;
            }
            else
            {
                if (playerTile == TileManager.instance.GetTile(player.transform.position))
                {
                    XPtoAdd += bossAngerAddition;
                }
                playerCool = 0;
            }
        }
    }

    void MoveEyes()
    {
        if (GameStateManager.instance.GetState() == GameStates.STATE_GAMEPLAY)
        {
            eyes.transform.position = new Vector2(transform.position.x +
            ((Camera.main.WorldToViewportPoint(player.transform.position).x * 2 - 1) / 10), eyes.transform.position.y);
        }
    }

    void ManyFacedBoss()
    {
        if (bossAngerLevel < faceList.Length)
        {
            spriteRenderer.sprite = faceList[bossAngerLevel];
        }
    }

    void AddToAnger()
    {
        if (bossAngerExp >= 1)
        {
            if (bossAngerLevel < faceList.Length)
            {
                bossAngerLevel++;
                if (bossAngerLevel == faceList.Length)
                {
                    ChangeState(FaceState.CHASE);
                }
                else
                {
                    bossAngerExp -= 1;
                }
            }
        }
    }
   public void AngerLevelAdj(int amount = 1)
    {
        bossAngerLevel += amount;
    }
    void SteamParticles()
   {
      
        if (bossAngerLevel==3&&faceState==FaceState.UI)
        {
            if (!steam.isPlaying)
            steam.Play();
        }
        else
        {
            if (steam.isPlaying)
            {
                steam.Stop();
            }
        }
   }

   public void CheckEmails(bool _correct)
   {
       if (!_correct)
       {
           XPtoAdd += bossAngerAddition;
       }
   }

   void ChangeState(FaceState _state)
   {
        if (faceState != _state)
        {
            faceState = _state;
            switch (faceState)
            {
                case FaceState.UI:
                    {
                        spriteRenderer.sprite = faceList[bossAngerLevel];
                        break;
                    }
                case FaceState.CHASE:
                    {
                        spriteRenderer.sprite = bossGone;
                        Boss.instance.BeginChase();

                        break;
                    }
            }
        }
    }

   public void ChangeStateBack()
   {
        XPtoAdd = 0;
        bossAngerExp = 0;
        bossAngerLevel -= 2;
        ChangeState(FaceState.UI);
   }

   public void NextEmail()
   {
       emailCool = 0;
   }

   public enum FaceState
   {
       UI,
       CHASE,
   }
    public override void Reset()
   {
        XPtoAdd = 0;
       bossAngerExp = 0;
       bossAngerLevel = 0;
        ChangeState(FaceState.UI);
       spriteRenderer.sprite = faceList[bossAngerLevel];
       emailCool = 0;
       playerCool = 0;
       steam.Stop();
   }
}