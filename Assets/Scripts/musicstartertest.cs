using UnityEngine;

public class musicstartertest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject musicplayer = GameObject.Find("bgmusic");
        BGmusic MusicComponent = musicplayer.GetComponent<BGmusic>();
        MusicComponent.Start_music();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
