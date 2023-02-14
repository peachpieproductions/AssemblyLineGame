using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public static AudioManager inst;

    [Header("Sounds")]
    public Sound[] snds;
    public Dictionary<string, Sound> soundLibrary = new Dictionary<string, Sound>();
    public List<AudioSource> audioSourcePool;
    public List<AudioSource> audioSourcePool3D;

    [Header("Music")]
    public bool musicOn;
    public bool dontShuffleFirstSong = true;
    public AudioClip currentSong;
    public List<AudioClip> songs;

    [Header("Refs")]
    public AudioSource musicPlayer;

    public int currentSongIndex;
    bool playingMusic;
    float songWaitTimer;
    int poolIndex;
    int pool3DIndex;
    Dictionary<AudioClip, int> soundPlayedRecently = new Dictionary<AudioClip, int>();

    private void Awake() {
        inst = this;
        SpawnAudioSourcePools();
    }

    private void Start() {
        foreach (var snd in snds) {
            soundLibrary.Add(snd.key, snd);
        }
        StartCoroutine(Music());
        StartCoroutine(SlowUpdate());
        StartCoroutine(SoundPlayedRecentlyClearRoutine());
    }

    IEnumerator SlowUpdate() {
        while (true) {

            if (!musicOn && playingMusic) {
                StopCoroutine(Music());
                musicPlayer.Stop();
                playingMusic = false;
            }
            if (musicOn && !playingMusic) StartCoroutine(Music());

            yield return new WaitForSecondsRealtime(1f);
        }
    }

    public void PlaySound(string soundKey) {
        if (soundLibrary.ContainsKey(soundKey)) {
            var sound = soundLibrary[soundKey];
            var clip = sound.clips[Random.Range(0, sound.clips.Count)];
            if (CheckIfClipPlayedTooMuch(clip)) return;
            var AS = GetNextAudioSourceInPool();
            AS.pitch = 1f;
            AS.pitch += Random.Range(-sound.pitchChange, sound.pitchChange);
            AS.PlayOneShot(clip, sound.vol);
        }
    }

    public void PlayClip(AudioClip ac, bool pitchShift = true, float vol = 1, float customPitch = 0) {
        if (ac == null) return;
        if (CheckIfClipPlayedTooMuch(ac)) return;
        var AS = GetNextAudioSourceInPool();
        if (pitchShift) AS.pitch = Random.Range(.8f, 1.2f);
        else AS.pitch = 1;
        if (customPitch > 0) AS.pitch = customPitch;
        AS.PlayOneShot(ac, vol);
    }

    public void Play3DSound(Vector3 pos, string soundKey) {
        var sound = soundLibrary[soundKey];
        var clip = sound.clips[Random.Range(0, sound.clips.Count)];
        if (CheckIfClipPlayedTooMuch(clip)) return;
        var AS = GetNext3DAudioSourceInPool();
        AS.transform.position = pos;
        AS.pitch = 1f;
        AS.pitch += Random.Range(-sound.pitchChange, sound.pitchChange);
        AS.PlayOneShot(clip, sound.vol);
        AddClipToPlayedRecently(clip);
        
    }

    public AudioSource GetNextAudioSourceInPool() {
        poolIndex++;
        if (poolIndex >= audioSourcePool3D.Count) poolIndex = 0;
        return audioSourcePool[poolIndex];
    }

    public AudioSource GetNext3DAudioSourceInPool() {
        pool3DIndex++;
        if (pool3DIndex >= audioSourcePool3D.Count) pool3DIndex = 0;
        return audioSourcePool3D[pool3DIndex];
    }

    IEnumerator Music() {

        //Shuffle songs, except first song
        List<AudioClip> tempSongList = new List<AudioClip>(songs);
        songs.Clear();
        int listLength = tempSongList.Count;
        for (var i = 0; i < listLength; i++) {
            if (i == 0  && dontShuffleFirstSong) {
                songs.Add(tempSongList[0]);
                tempSongList.RemoveAt(0);
            } else {
                var index = Random.Range(0, tempSongList.Count);
                songs.Add(tempSongList[index]);
                tempSongList.RemoveAt(index);
            }
        }

        while (musicOn) {
            playingMusic = true;

            musicPlayer.clip = songs[currentSongIndex];
            currentSong = musicPlayer.clip;

            yield return StartCoroutine(PlaySong());

            currentSongIndex++;
            if (currentSongIndex == songs.Count) currentSongIndex = 0;
        }
    }

    IEnumerator PlaySong() {
        musicPlayer.Play();
        songWaitTimer = musicPlayer.clip.length + Random.Range(20, 40);
        while (songWaitTimer > 0) {
            songWaitTimer -= Time.unscaledDeltaTime;
            yield return null;
        }
        musicPlayer.Stop();
    }

    public void SkipSong() {
        songWaitTimer = 0;
    }

    public void SpawnAudioSourcePools() {
        //2D Sound Pool
        for (int i = 0; i < 100; i++) {
            var newGo = new GameObject();
            newGo.transform.parent = transform;
            var AS = newGo.AddComponent<AudioSource>();
            //AS.outputAudioMixerGroup = soundEffectsAudioMixerGroup;
            audioSourcePool.Add(AS);
        }
        //3D Sound Pool
        for (int i = 0; i < 100; i++) {
            var newGo = new GameObject();
            newGo.transform.parent = transform;
            var AS = newGo.AddComponent<AudioSource>();
            AS.spatialBlend = 1f;
            //AS.outputAudioMixerGroup = soundEffectsAudioMixerGroup;
            audioSourcePool3D.Add(AS);
        }
    }

    public void AddClipToPlayedRecently(AudioClip clip) {
        if (soundPlayedRecently.ContainsKey(clip)) {
            soundPlayedRecently[clip]++;
        } else {
            soundPlayedRecently.Add(clip, 1);
        }
    }

    public bool CheckIfClipPlayedTooMuch(AudioClip clip) {
        if (soundPlayedRecently.ContainsKey(clip)) {
            return soundPlayedRecently[clip] > 0;
        } else {
            return false;
        }
    }

    IEnumerator SoundPlayedRecentlyClearRoutine() {
        while (true) {
            soundPlayedRecently.Clear();
            yield return new WaitForSeconds(.5f);
        }
    }

}

[System.Serializable]
public struct Sound {
    public string key;
    public List<AudioClip> clips;
    public float vol;
    public float pitchChange;
}
