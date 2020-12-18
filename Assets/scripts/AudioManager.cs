using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PostProcessing;

public class AudioManager : MonoBehaviour {
    public static AudioManager instance;
    AudioSource audioSourceComp;
    [SerializeField] Text songName, songTime;
    [SerializeField] Image pausePlayButton;
    [SerializeField] Sprite pauseImage, playImage;
    [SerializeField] AudioClip[] songs;
    [SerializeField] PostProcessingProfile bloomProf;
    [SerializeField] float bloomMin, bloomMax,bloomMult,bloomSpeed,volumeSmoothAmount;
    [Header("crazy effects")]
    [SerializeField] float runBackTime, skippingTime;
    int index=-1;
    bool paused;
    string currentSongLength;
    float volume;

    BloomModel.Settings newMod;
    BloomModel.BloomSettings newModBloom;
    float[] sampleArray = new float[128];
    // Use this for initialization
    void Start () {
        if (instance == null)
        {
            instance = this;

            audioSourceComp = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
            PlayNextSong();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (audioSourceComp.isPlaying)
        {
            UpdateSongTime();
            VFX();
        }
        else if(!paused)
        {
            PlayNextSong();
        }
    }
    void VFX()
    {
        
       newMod= bloomProf.bloom.settings;
        newModBloom = newMod.bloom;
        audioSourceComp.GetSpectrumData(sampleArray, 0,FFTWindow.Rectangular);
        float  newVolume = 0;
        for(int i = 0; i < sampleArray.Length; i++)
        {
            newVolume += sampleArray[i];
        }
        
        newVolume /= sampleArray.Length;
        newVolume*= bloomMult;
        volume = Mathf.MoveTowards(volume, newVolume, volumeSmoothAmount * Time.deltaTime);
        
        newModBloom.intensity =Mathf.MoveTowards(newModBloom.intensity, Mathf.Clamp(volume, bloomMin, bloomMax),bloomSpeed*Time.deltaTime);
        newMod.bloom = newModBloom;
        bloomProf.bloom.settings = newMod;
    }
    public void PlayNextSong()
    {
        index++;
        if (index >= songs.Length)
        {
            index = 0;
        }
        audioSourceComp.clip = songs[index];
        audioSourceComp.time = 0;
        audioSourceComp.Play();

        //do ui stuff
        UpdateSongName();
    }
    public void PlayPrevSong()
    {
        index--;
        if (index < 0)
        {
            index = songs.Length - 1;
        }

        audioSourceComp.clip = songs[index];
        audioSourceComp.time = 0;
        audioSourceComp.Play();

        //do ui stuff
        UpdateSongName();
    }
    public void PausePlay()
    {
        if (!paused)
        {
            Pause();
            paused = true;
        }
        else
        {
            paused = false;
            UnPause();
        }
    }
    void Pause()
    {
        audioSourceComp.Pause();
        pausePlayButton.sprite = playImage;
    } 
    void UnPause()
    {
        audioSourceComp.UnPause();
        pausePlayButton.sprite = pauseImage;
    }
    
    public void UpdateSongName()
    {
        songName.text = audioSourceComp.clip.name;

        //also get the length
        int minutesPlayed = Mathf.FloorToInt(audioSourceComp.clip.length / 60F);
        int secondsPlayed = Mathf.FloorToInt(audioSourceComp.clip.length - minutesPlayed * 60);
        currentSongLength = string.Format("{0:0}:{1:00}", minutesPlayed, secondsPlayed);

    }
    public void UpdateSongTime()
    {
        int minutesPlayed = Mathf.FloorToInt(audioSourceComp.time / 60F);
        int secondsPlayed = Mathf.FloorToInt(audioSourceComp.time - minutesPlayed * 60);
        string currentTime = string.Format("{0:0}:{1:00}", minutesPlayed, secondsPlayed);

        
        songTime.text = currentTime + "/" + currentSongLength;
    }
    public void RunItBack()
    {
        if (!paused)
        {
            //rewind the song a bit for resets
            StartCoroutine(RunBackRewind());
            //audioSourceComp.time -= runBackTime;
        }
    }
    IEnumerator RunBackRewind()
    {
        audioSourceComp.pitch = -audioSourceComp.pitch;
        yield return new WaitForSeconds(runBackTime);
        audioSourceComp.pitch = -audioSourceComp.pitch;
    }
    public void Skip(float duration)
    {
        if (!paused)
        {
            StartCoroutine(SkipForABit(duration));
        }
    }
    IEnumerator SkipForABit(float duration)
    {
        int counter = 0;
        while (counter < (int) duration/skippingTime)
        {
            audioSourceComp.time -= skippingTime;
            yield return new WaitForSeconds(skippingTime);
            counter++;
        }
        
    }
    public void Quit()
    {
        Application.Quit();
    }
}
