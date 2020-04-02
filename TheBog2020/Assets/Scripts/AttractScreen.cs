﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class AttractScreen: MonoBehaviour {
    public RawImage rawImage;
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;
    // Use this for initialization
    void Start () {
        StartCoroutine(PlayVideo());
    }
    IEnumerator PlayVideo()
    {
        videoPlayer.Prepare();
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);
        while (!videoPlayer.isPrepared)
        {
            yield return waitForSeconds;
            break;
        }
        rawImage.texture = videoPlayer.texture;
        videoPlayer.Play();
        audioSource.Play();
    }

    public void StopVideo()
    {
        rawImage.CrossFadeAlpha(0f,3f,true);
        rawImage.color = Color.black;
        videoPlayer.Stop();
        audioSource.Stop();
    }
}