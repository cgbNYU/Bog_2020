using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlmenaraGames;

namespace AlmenaraGames.Demo{
public class PickableCoin : MonoBehaviour {

	private MeshRenderer meshRender;
	private string playerTag = "Player";

	void Start()
	{

		// Get The Mesh Renderer
		meshRender = GetComponent<MeshRenderer> ();
		
		//Start CheckForPlayer Coroutine
		StartCoroutine (CheckForPlayer ());

	}

	void Update () {

		//Simple Rotate Animation
		transform.eulerAngles += Vector3.up * Time.deltaTime * 100f;

	}

	//Coroutine that checks if the coin has been picked by a player
	IEnumerator CheckForPlayer()
	{
		while (true) {
			bool picked = false;
			Collider[] players = Physics.OverlapSphere (transform.position, 1f);

			foreach (var item in players) {

					if (item.CompareTag (playerTag)) {
					Pickup ();
					picked = true;
				}

			}


			if (!picked)
				yield return new WaitForEndOfFrame ();
			else {

				yield return new WaitForSeconds (3f);
				meshRender.enabled = true;

			}


		}

	}

	void Pickup()
	{

		//Plays a Global Audio Object via its Identifier
		MultiAudioManager.PlayAudioObjectByIdentifier ("coin sfx", transform.position);

		meshRender.enabled = false;

	}

}
}