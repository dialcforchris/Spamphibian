﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MenuSelect 
{
    [SerializeField]
    Slider soundfxSlider,musicSlider;
	
	// Update is called once per frame
	protected override void Update()
    {
        base.Update();
        SetFunctionToButton();
    }

    protected override void SetFunctionToButton()
    {
        if (selectBox == 0)
        {
            if (Input.GetButtonDown("Fire1"))
                GameStateManager.instance.ChangeState(GameStateManager.instance.previousState);
        }
        else if (selectBox == 1)
        {
            DoAction(ChangeVolumeSlide);
        }
        else if (selectBox == 2)
        {
            DoAction(ChangeMusicSlide);
        }
        if (Input.GetButtonDown("Fire1"))
        {
            if (selectBox == 3)
            {
                DoAction(QuitToMain);
            }
        }
    }

    void QuitToMain()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(2);
    }

    void ChangeVolumeSlide()
    {
        if (soundfxSlider.value > 0 || soundfxSlider.value < 1)
        {
            if (Input.GetJoystickNames().Length > 0)
            {
                if (Input.GetJoystickNames()[0] == "")
                    soundfxSlider.value += Input.GetAxis("Horizontal") * 10;
                else
                    soundfxSlider.value += Input.GetAxis("Horizontal") / 100;
            }
            else
                soundfxSlider.value += Input.GetAxis("Horizontal") * 10;

            SoundManager.instance.changeVolume(soundfxSlider.value);
        }
    }
    void ChangeMusicSlide()
    {
        if (musicSlider.value > 0 || musicSlider.value < 1)
        {
            if (Input.GetJoystickNames().Length > 0)
            {
                if (Input.GetJoystickNames()[0] == "")
                    musicSlider.value += Input.GetAxis("Horizontal") * 10;
                else
                    musicSlider.value += Input.GetAxis("Horizontal") / 100;
            }
            else
                musicSlider.value += Input.GetAxis("Horizontal") * 10;

            SoundManager.instance.music.volume = musicSlider.value;
            SoundManager.instance.musicVolume = musicSlider.value;

        }
    }
}
