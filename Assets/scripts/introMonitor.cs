﻿using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class introMonitor : MonoBehaviour
{
    public static introMonitor instance;
    
    public SpriteRenderer emailContent;
    public Animator monitorAnimator, miniEmailAnimator;
    public Camera monitorCamera, mainCam;
    public SpriteRenderer mainCamTransition, monCamTransition;
    public Texture[] gradients;
    public AudioClip[] keypressSounds;
    public AudioSource computerSounds;

    public mail currentMail;

    //An email object, might need variables for score and such in the future
    [System.Serializable]
    public struct mail
    {
        public Sprite image;
        public bool isJunk;
    }

    void Awake()
    {
        instance = this;
    }

    public void enterView()
    {
        //should probably change game manager state to email viewing
        GameStateManager.instance.ChangeState(GameStates.STATE_EMAIL);
        emailPos = 0;

        //Change cameras over
        StartCoroutine(camTransition(true));
    }

    IEnumerator camTransition(bool InOut) //True for entering email mode, false for exiting it
    {
        computerSounds.DOFade((InOut) ? SoundManager.instance.volumeMultiplayer : 0, 2);
        Random.seed = System.DateTime.Now.Millisecond;
        mainCamTransition.material.SetTexture("_SliceGuide", gradients[Random.Range(0, gradients.Length)]);
        monCamTransition.material.SetTexture("_SliceGuide", gradients[Random.Range(0, gradients.Length)]);

        float lerpy = 1;
        while (lerpy > 0)
        {
            lerpy -= Time.deltaTime * 2;

            if (!InOut)
                monCamTransition.material.SetFloat("_SliceAmount", lerpy);
            else
                mainCamTransition.material.SetFloat("_SliceAmount", lerpy);

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.1f);
        mainCam.enabled = !InOut;
        monitorCamera.enabled = InOut;

        lerpy = 0;
        while (lerpy < 1)
        {
            lerpy += Time.deltaTime;

            if (InOut)
                monCamTransition.material.SetFloat("_SliceAmount", lerpy);
            else
                mainCamTransition.material.SetFloat("_SliceAmount", lerpy);

            yield return new WaitForEndOfFrame();
        }

        if (InOut)
        {
            //Intro animation
            monitorAnimator.Play("mail_intro");
            emailContent.sprite = currentMail.image;

            StopCoroutine("zoomInOut");
            StartCoroutine(zoomInOut(7.5f));
        }
        else
            GameStateManager.instance.ChangeState(GameStates.STATE_GAMEPLAY);
    }

    int nonFrogMail = 0;
    bool frogStory = true;
    IEnumerator zoomInOut(float newSize)
    {
        yield return new WaitForSeconds(1);
        while (Mathf.Abs(monitorCamera.orthographicSize - newSize) > 0.1f)
        {
            monitorCamera.orthographicSize = Mathf.Lerp(monitorCamera.orthographicSize, newSize, Time.deltaTime * 2);
            if (GameStateManager.instance.GetState() == GameStates.STATE_GAMEPLAY)
            {
                monitorCamera.orthographicSize = 11;
                break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void exitView()
    {
        //Change cameras over
        StartCoroutine(camTransition(false));
    }

    int emailPos = 0;
    public void moveEmail(int dir)
    {
        if (emailPos == 0) //Email is in the middle of the screen
        {
            if (dir > 0)
                monitorAnimator.Play("mail_right");
            else
                monitorAnimator.Play("mail_left");

            emailPos += dir;

            if (!soundPlaying)
            {
                SoundManager.instance.playSound(keypressSounds[Random.Range(0, keypressSounds.Length)]);
                soundPlaying = true;
                Invoke("allowSounds", .5f);
            }
        }
        else if (emailPos > 0) //Email currently sits at the right of the screen
        {
            if (dir < 0)//Move left
            {
                monitorAnimator.Play("mail_right_reverse");
                emailPos = 0;//Email is back in the middle now
                if (!soundPlaying)
                {
                    SoundManager.instance.playSound(keypressSounds[Random.Range(0, keypressSounds.Length)]);
                    soundPlaying = true;
                    Invoke("allowSounds", .5f);
                }
            }
        }
        else //Email must be at the left hand side of the monitor
        {
            if (dir > 0)//Move right
            {
                monitorAnimator.Play("mail_left_reverse");
                emailPos = 0;//Email is back in the middle now
                if (!soundPlaying)
                {
                    SoundManager.instance.playSound(keypressSounds[Random.Range(0, keypressSounds.Length)]);
                    soundPlaying = true;
                    Invoke("allowSounds", .5f);
                }
            }
        }
    }

    void Update()
    {
        InputRelated();
    }

    void allowSounds()
    {
        soundPlaying = false;
    }

    bool soundPlaying;

    void InputRelated()
    {
        //Only allow the player to move the email if it's not moving
        if (monitorAnimator.GetCurrentAnimatorStateInfo(0).IsName("mail_idle_middle")
            || monitorAnimator.GetCurrentAnimatorStateInfo(0).IsName("mail_idle_left")
            || monitorAnimator.GetCurrentAnimatorStateInfo(0).IsName("mail_idle_right"))
        {

            if (Input.GetAxis("Horizontal") != 0)
            {
                moveEmail(Input.GetAxis("Horizontal") > 0 ? 1 : -1);
            }

            if (Input.GetButtonDown("Fire1") && emailPos != 0)
            {
                if (!soundPlaying)
                {
                    SoundManager.instance.playSound(keypressSounds[Random.Range(0, keypressSounds.Length)]);
                    soundPlaying = true;
                    Invoke("allowSounds", .5f);
                }
                if (emailPos > 0) //If email is in the JUNK zone
                {
                    //Do animation for email being destroyed
                    monitorAnimator.Play("mail_junk");

                    if (currentMail.isJunk)
                    {
                        StopCoroutine("zoomInOut");
                        StartCoroutine(zoomInOut(11));

                        Invoke("exitView", 3.5f);
                    }
                    else
                    {
                        StopCoroutine("zoomInOut");
                        StartCoroutine(zoomInOut(11));

                        Invoke("exitView", 3.5f);
                    }
                }
                else if (emailPos < 0) //If email is in the SAFE zone
                {
                    //Do animation for email being marked as safe
                    monitorAnimator.Play("mail_safe");
                    miniEmailAnimator.Play("email_leave 0");

                    if (currentMail.isJunk)
                    {
                        StopCoroutine("zoomInOut");
                        StartCoroutine(zoomInOut(11));

                        Invoke("exitView", 2.5f);
                    }
                    else
                    {
                        StopCoroutine("zoomInOut");
                        StartCoroutine(zoomInOut(11));

                        Invoke("exitView", 2.5f);
                    }
                }
            }
        }
    }
}

[CustomEditor(typeof(introMonitor))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (EditorApplication.isPlaying)
        {
            //mailOpener myScript = (mailOpener)target;
            if (GUILayout.Button("Enter Monitor View"))
            {
                introMonitor.instance.enterView();
            }
        }
    }
}