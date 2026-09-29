using UnityEngine;

public class SFXManager : MonoBehaviour
{

    public static SFXManager instance;

    [SerializeField] private AudioSource SFXObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        if (audioClip == null) return;

        // spawn sound
        AudioSource audioSource = Instantiate(SFXObject, spawnTransform.position, Quaternion.identity);

        //assign audio
        audioSource.clip = audioClip;

       
        // play sound
        audioSource.Play();

        // get length of sfx
        float clipLength = audioSource.clip.length;
        // destroy clip after its done
        Destroy(audioSource.gameObject, clipLength);
    }
}
