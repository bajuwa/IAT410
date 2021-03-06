﻿using UnityEngine;
using System.Collections;

/**
 * This script should be attached to a GameObject that can be treated as a button.
 * When clicked, that object will then redirect the player to the configured scene.
 */
public class ClickToLoadNextScene : MonoBehaviour {

	// This keeps track of all the scenes in our game that we can navigate to
	// First is the actual name of the scene that has meaning to players/developers (MAIN_MENU, CREATE_GAME, INTRO_MAP, etc)
	// Next is the number value that Unity uses to load a new scene, these should correspond to the scene numbers in the Build Settings
	public enum sceneName {
		MAIN_MENU = 0,
		CREATE_GAME = 1,
		JOIN_GAME = 2,
		TUTORIAL_BASICS = 3,
		WIN_RESULTS = 4,
		MULTI_PLAYER_LOAD = 5,
		MULTI_PLAYER_GAME = 6,
		TUTORIAL_GATHERER = 7,
		TUTORIAL_WARRIOR = 8,
		TUTORIAL_SCOUT = 9,
		TUTORIAL_ANTHILL = 10,
		TUTORIAL_QUEEN = 11,
		MULTI_PLAYER_GAME_MAZE = 12,
		LOSE_RESULTS = 13
	}

	// Here is where we actually set which scene we want to go to when an object with this script is pressed
	// Set this using the unity editor for each sprite object that uses this script!
	public sceneName nextScene;
	
	// This function will be called every frame that the cursor is hovered over the object
	void OnMouseOver() {
		// If you want any hover effects, put them here!
	
		// If the user presses the button, load the next scene that is configured with this object
		if (Input.GetMouseButtonDown(0)) { 
			Application.LoadLevel((int)nextScene);
		}
	}
}
