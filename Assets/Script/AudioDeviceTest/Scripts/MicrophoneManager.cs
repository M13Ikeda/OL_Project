using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneManager : MonoBehaviour {

    public List<RecSource> RecSources;

    [System.Serializable]
    public class RecSource {
        public string device;
        public AudioSource audioSource = new AudioSource();

        public void Rec(){
            //ここのloop,lengthSec,frequencyの値は暫定です。普段のOVRLipSyncで指定されている際の値に修正してください。
            //参考URL：https://docs.unity3d.com/ja/current/ScriptReference/Microphone.Start.html
            bool loop = true;
            int lengthSec = 600;
            int frequency = 48000;
            audioSource.clip = Microphone.Start(device, loop, lengthSec, frequency);
            audioSource.clip.name = "Input for "+device;
        }
        public void Play(){
            audioSource.Play();
        }

    }


    void Start() {
        var i = 0;
        foreach (string device in Microphone.devices) {
            var source = RecSources[i];
            source.device = device;
            i++;
        }

        foreach (RecSource source in RecSources) {
            source.Rec();
            source.Play();
        }

    }

}
