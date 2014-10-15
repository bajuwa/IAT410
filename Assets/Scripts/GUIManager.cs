﻿using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {
	//MapUIManager object
	public MapUIManager mUM;
	//Empty GUITexture that will display what is selected in the bottom left
	public GUITexture headDisplay;
	//Empty GUIText that will display the description of the unit/tile
	public GUIText statusDisplay;
	// Use this for initialization
	void Start () {
		//Set it to null on runtime because the default is
		//the unity logo.
		headDisplay.texture = null;
		statusDisplay.text = null;
	}
	
	// Update is called once per frame
	void Update () {
		//assign the texture a new texture based on what is selected and display it in the bottom left
		headDisplay.texture = mUM.getCurrentlySelectedObject().displayImage;
		//assign the GUIText new text based on what is selected and display it next to the head image
		statusDisplay.text = mUM.getCurrentlySelectedObject().Description;
		
	}
}
