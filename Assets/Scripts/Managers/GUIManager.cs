﻿using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {
	//MapUIManager object
	public MapUIManager mUM;
	
	public GUITexture uiHead;
	public GUITexture uiStatus;
	//Empty GUITexture that will display what is selected in the bottom left
	public GUITexture headDisplay;
	//Empty GUIText that will display the description of the unit/tile
	public GUIText statusDisplay;
	// Use this for initialization
	public GUIStyle descriptionStyle;
	void Start () {
		//Set it to null on runtime because the default is
		//the unity logo.
		headDisplay.texture = null;
		statusDisplay.text = null;
		
	}
	
	// Update is called once per frame
	void OnGUI () {
		// Get the currently selected object that we will be displaying information about
		Selectable currentlySelected = mUM.getCurrentlySelectedObject();
		if (currentlySelected) {
			// Assign the texture a new texture based on what is selected and display it in the bottom left
			headDisplay.texture = currentlySelected.getDisplayImage();
			// Assign the GUIText new text based on what is selected and display it next to the head image
			//statusDisplay.text = currentlySelected.description;
			GUI.Label (new Rect (300,730,200,uiStatus.texture.height), currentlySelected.description, descriptionStyle);
		}
	}

}