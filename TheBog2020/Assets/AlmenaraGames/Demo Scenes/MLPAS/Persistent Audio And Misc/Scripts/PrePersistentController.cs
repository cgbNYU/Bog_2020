using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlmenaraGames.Demo{
	public class PrePersistentController : MonoBehaviour {

        
		public AudioObject audioObject;

        public AudioObject musicAudioObject;

        public AudioClip clipForOverride;

		public static bool sceneReloaded = false;
		public static float sceneReloadedTimer=0f;

		//Plays an Audio Object
		void PlayAudioObject () {

			MultiAudioManager.PlayAudioObject (audioObject,transform.position);

		}

		//Plays an Audio Object with another Audio Clip
		void PlayAudioObjectOverride () {

			MultiAudioManager.PlayAudioObjectOverride (clipForOverride,audioObject,transform.position);

		}

		//Plays an Audio Object by its identifier
		void PlayAudioObjectByIdentifier () {

			MultiAudioManager.PlayAudioObjectByIdentifier ("coin sfx",transform.position);

		}

		//Using Channels to play a sound and avoid audio overlapping
		void PlayVoiceAtChannel () {

			MultiAudioManager.PlayAudioObjectByIdentifier ("hello",1,transform.position);

		}

		//Plays a sound at no channel
		void PlayVoice () {

			MultiAudioManager.PlayAudioObjectByIdentifier ("hello",transform.position);
             
		}

		//Plays an Audio Object overriting some of its parameters
		void PlaySoundOverrideParameters()
		{
			MultiAudioSource sound = MultiAudioManager.PlayAudioObject (audioObject,transform.position);
			//Play the audio backwards
			sound.PitchOverride = -1; 
			//Change the spatial mode to 2D
			sound.SpatialMode2DOverride = true;

		}

		//Plays a Persistent Audio
		void PlayMusic () {

			MultiAudioSource persistentAudio = MultiAudioManager.PlayAudioObject(musicAudioObject, 3,transform.position);
			//Makes the sound persistent
			persistentAudio.PersistsBetweenScenes = true;

		}

		//Fade In a Normal Music
		void FadeInNonPersistentMusic () {

			// Checks if an Multi Audio Source is playing at Channel 3 to fade in correctly the new audio without immediately stop the previous one
			if (MultiAudioManager.GetAudioSourceAtChannel (3) != null) {
				MultiAudioManager.FadeOutAudioSource (3, 1f); //Fade out previous audio
				MultiAudioManager.FadeInDelayedAudioObject (musicAudioObject, 1.25f, 3, transform.position, 2f); //Fade in new audio 0.25 seconds after the fade out of the previous audio
			} else {
				MultiAudioManager.FadeInAudioObject (musicAudioObject, 3, transform.position, 2f); //Fade in new audio
			}

		}

		//Plays a Normal Music
		void PlayNonPersistentMusic () {

			MultiAudioManager.PlayAudioObject (musicAudioObject, 3,transform.position);

		}

		//Stop Audio at Channel 3
		void StopMusic()
		{

			MultiAudioManager.StopAudioSource (3);

		}

		//Fade Audio at Channel 3
		void FadeOutMusic()
		{

			MultiAudioManager.FadeOutAudioSource (3,2f);

		}

		void ReloadScene()
		{

			sceneReloaded = true;
			UnityEngine.SceneManagement.SceneManager.LoadScene (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

		}

		// UI Code - IGNORE
		public void OnGUI()
		{

			int xpos = 40;
			int ypos = 20;
			int spacing = 24;

			if (GUI.Button(new Rect(xpos, ypos, 200, 40), "Play Audio Object with \nmultiple clips"))
			{
				PlayAudioObject();
			}

			ypos += spacing+20;

			if (GUI.Button(new Rect(xpos, ypos, 200, 40), "Play Audio Object with \nanother Audio Clip"))
			{
				PlayAudioObjectOverride();
			}

			ypos += spacing + 20;

			if (GUI.Button(new Rect(xpos, ypos, 280, 40), "Play AudioObject By Identifier \n\"coin sfx\""))
			{
				PlayAudioObjectByIdentifier();
			}

			ypos += spacing+20;

			if (GUI.Button(new Rect(xpos, ypos, 300, 40), "Play Sound Overriting some of its parameters\n-Plays Backwards and 2D-"))
			{
				PlaySoundOverrideParameters();
			}

			ypos += spacing+20;

			GUI.Label (new Rect (xpos, ypos, 256, 64),Resources.Load ("MLPASImages/logoSmall") as Texture);
				
			ypos = 20;

			if (GUI.Button(new Rect(xpos+220, ypos, 300, 20), "Play Voice at Channel 1 to avoid overlapping"))
			{
				PlayVoiceAtChannel();
			}

			ypos += spacing;

			if (GUI.Button(new Rect(xpos+220, ypos, 200, 20), "Play Voice with no channel"))
			{
				PlayVoice();
			}

			ypos =20;

			if (GUI.Button(new Rect(xpos+540, ypos, 250, 20), "Play Normal Music at Channel 3"))
			{
				PlayNonPersistentMusic();
			}

			ypos += spacing;

			if (GUI.Button(new Rect(xpos+540, ypos, 250, 20), "Fade In Normal Music at Channel 3"))
			{
				FadeInNonPersistentMusic();
			}

			ypos += spacing;

			if (GUI.Button(new Rect(xpos+540, ypos, 250, 20), "Play Persistent Audio at Channel 3"))
			{
				PlayMusic();
			}

			ypos += spacing;

			if (GUI.Button(new Rect(xpos+540, ypos, 200, 20), "Fade Out Audio at Channel 3"))
			{
				FadeOutMusic();
			}

			ypos += spacing;

			if (GUI.Button(new Rect(xpos+540, ypos, 200, 20), "Stop Audio at Channel 3"))
			{
				StopMusic();
			}

			ypos += spacing;

			GUI.color = Color.green;
			if (GUI.Button(new Rect(xpos+540, ypos, 300, 20), "Reload Scene for test Presistent Audio"))
			{
				ReloadScene();
			}
				
			ypos += spacing;

			if (sceneReloaded) {
				GUI.color = Color.yellow;
				GUI.Label (new Rect (xpos + 540, ypos, 300, 20), "SCENE RELOADED");

				sceneReloadedTimer += Time.deltaTime;

				if (sceneReloadedTimer > 2f) {
					sceneReloadedTimer = 0f;
					sceneReloaded = false;
				}
			}

		}

	}
}