using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drips : MonoBehaviour
{

    private GameObject dripSpawn;
    private GameObject puddle;
    private GameObject player;

    public Transform drip;
    public AudioClip dripSound;

    // Start is called before the first frame update
    void Start()
    {
        dripSpawn = GameObject.FindGameObjectWithTag("dripSpawn");
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        //conditions here?? call the drip function when the player is nearby
            Drip();
    }

    // Update is called once per frame
    private void Drip()
    {

        AudioSource audioSource = gameObject.AddComponent<AudioSource>() as AudioSource;

        Instantiate(drip, dripSpawn.transform.position, dripSpawn.transform.rotation);

        if (drip.gameObject.CompareTag("puddle"))
        {
            audioSource.PlayOneShot(dripSound);
            Destroy(drip.gameObject);
        }
    }
}
