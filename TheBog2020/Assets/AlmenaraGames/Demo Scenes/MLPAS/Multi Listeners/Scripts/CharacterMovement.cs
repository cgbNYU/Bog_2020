using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlmenaraGames.Demo{
public class CharacterMovement : MonoBehaviour {

	private Rigidbody rb;

	public int playerId=1;
	public float speed=500f;

	private string[] inputs = new string[]{"w","s","d","a"};

	private string[] p1Inputs = new string[]{"w","s","d","a"};
	private string[] p2Inputs = new string[]{"up","down","right","left"};
	private string[] p3Inputs = new string[]{"[8]","[5]","[6]","[4]"};
	private string[] p4Inputs = new string[]{"u","j","k","h"};

	private Vector3 vel;
	private Vector3 lastVel;

	public GameObject pauseCanvas;

	void Start()
	{

	//Get Rigid Body
	rb=GetComponent<Rigidbody>();

	// Assign the inputs according to the player ID
	switch (playerId) {

		case 1:
			inputs = p1Inputs;
			break;
		case 2:
			inputs = p2Inputs;
			break;
		case 3:
			inputs = p3Inputs;
			break;
		case 4:
			inputs = p4Inputs;
			break;
		}

	}

	void Update () {

			// Simple test movement code

			vel = Vector3.zero;

			if (Input.GetKey (inputs [0])) {
				vel += Vector3.forward;
				lastVel = vel;
			}
			if (Input.GetKey (inputs [1])) {
				vel += -Vector3.forward;
				lastVel = vel;
			}
			if (Input.GetKey (inputs [2])) {
				vel += Vector3.right;
				lastVel = vel;
			}
			if (Input.GetKey (inputs [3])) {
				vel += -Vector3.right;
				lastVel = vel;
			}

			Vector3 finalVel = (vel.normalized * speed) * Time.deltaTime;
			finalVel.y = 0;
			rb.velocity=finalVel;

			// Simple test rotation code

			if (lastVel!=Vector3.zero)
			transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (lastVel), Time.deltaTime * 15f);

			//Pause the Game
			if (playerId == 1 && Input.GetKeyDown ("return")) {

                //Pause the Audio System
				MultiAudioManager.Paused = !MultiAudioManager.Paused;
				if (MultiAudioManager.Paused) {
					//Play Music via pooled system
					MultiAudioSource pauseBgm = MultiAudioManager.PlayAudioObjectByIdentifier ("pause music", 6, transform.position);
                    //Sets the Multi Audio Source to ignore the Pause state of the Audio System
                    pauseBgm.IgnoreListenerPause = true;
					pauseCanvas.SetActive (true);
					Time.timeScale = 0;
				} else {
					MultiAudioManager.StopAudioSource (6);
					pauseCanvas.SetActive (false);
					Time.timeScale = 1;
				}

			}

		}

	}
}
