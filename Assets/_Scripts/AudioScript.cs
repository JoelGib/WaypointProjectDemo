using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioScript : MonoBehaviour
{
    // Start is called before the first frame update
    private AudioSource audioSource;
    [SerializeField]private AudioClip introClip;
    [SerializeField]private AudioClip BGMusic;
    [SerializeField]private AudioClip playerRunningAudio;
    [SerializeField]private AudioClip navigationEndAudio;
    [SerializeField]private AudioClip naviagtionStartAudio;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = introClip;
        audioSource.Play();
        StartCoroutine(PlayBGMusic());
    }

    IEnumerator PlayBGMusic(){
        yield return new WaitForSeconds(audioSource.clip.length + 0.25f);
        audioSource.clip = BGMusic;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void PlayRunningFootstepAudio(AudioSource _as){
        _as.clip = playerRunningAudio;
        _as.loop = true;
        _as.Play();
    }

    public void PlayNavigationStartAudio(AudioSource _as){
        _as.clip = naviagtionStartAudio;
        _as.loop = false;
        _as.Play();
    }

    public void PlayNavigationEndAudio(AudioSource _as){
        _as.clip = navigationEndAudio;
        _as.loop = false;
        _as.Play();
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}
