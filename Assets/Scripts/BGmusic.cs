using Unity.VisualScripting;
using UnityEngine;

public class BGmusic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Start_music()
    {
        AudioSource bgmusic = GetComponent<AudioSource>();
        bgmusic.Play();
    }


}
