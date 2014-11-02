﻿using UnityEngine;
using System.Collections;

/**
 * The AntUnit with the unique ability to walk on top of food in order to pick them up.
 * Once food is picked up, they will move at half their speed, and they can press a small
 * button displayed above them in order to drop their food
 */
public class GathererUnit : AntUnit {

	private bool droppedFood = true;
	
	//To be displayed on the GUI
	public override string getDescription() {
		if (isNeutralOrFriendly())
			return "Able to pick up fruit and carry it back to your Anthill.";
		else
			return "Helps your enemy's colony grow larger if it can find food!";
	}
	
	public override string getName() {
		return "Gatherer";
	}
	
	protected override void Start () {
		base.Start();
	}
	
	// Update is called once per frame
	protected override void Update () {
		// Leave the movement up to the AntUnit class
		base.Update();
		
		// If we have stopped moving and have landed on a food item, pick it up if we aren't already carrying some
		if (!droppedFood && this.gameObject.GetComponentInChildren<Food>() == null && getCurrentTile() == getTargetTile() && targetPath != null && targetPath.getTilePath().Count == 0) {
			Collider2D[] itemsOnSameTile = Physics2D.OverlapPointAll(transform.position);
			foreach (Collider2D col in itemsOnSameTile) {
				if (col.gameObject.GetComponent<Food>() != null) {
					Debug.Log("Detected food on current tile, picking up");
					pickUpFood(col.gameObject);
				}
			}
		}
		
		// Determine what animation we should be playing
		if (GetComponentsInChildren<Food>().Length > 0) animator.SetInteger("STATE", 2);
		else if (getCurrentTile() != getTargetTile()) animator.SetInteger("STATE", 1);
		else animator.SetInteger("STATE", 0);
		
		// If the player is moving, then clear our 'dropped food' flag so that the unit can pick up food again
		if (getCurrentTile() != getTargetTile()) droppedFood = false;
	}
	
	protected override void loadAnimator() {
		if (animator) return; 
		Debug.Log("Loading animator");
		animator = gameObject.AddComponent("Animator") as Animator;
		animator.runtimeAnimatorController = getAnimatorFromPlayer("gathererAnimator");
	}
	
	protected override void loadDisplayImage() {
		displayImage = getTextureFromPlayer("gathererDisplay");
	}
	
	public void pickUpFood(GameObject gameObj) {
		// Set the parent to our unit so that it is 'carried' when the unit is moving
		gameObj.transform.parent = this.gameObject.transform;
		
		// Also set the local position's z value to -1 to ensure it is visible above the unit
		Vector3 tempPos = gameObj.transform.localPosition;
		tempPos.z = -1; 
		gameObj.transform.localPosition = tempPos;
		
		// Re-enable the selectable script so that we can select it again
		gameObj.GetComponent<Selectable>().enabled = false;
		gameObj.GetComponent<CircleCollider2D>().enabled = false;
		
		// When gatherers are carrying food, they move at half their original speed
		speed /= 2f;
	}
	
	public void dropFood() {
		// To avoid automatically picking the food back up again, set a flag
		droppedFood = true;
		
		// Reassign the food back to the 'Objects' sprite in the map
		Transform foodTransform = this.gameObject.GetComponentInChildren<Food>().gameObject.transform;
		foodTransform.parent = GameObject.Find("Objects").transform;
		
		// Make sure when dropped it snaps to the appropriate tile
		foodTransform.position = mapManager.getTileAtPosition(foodTransform.position).transform.position;
		
		// Check to see if the new position would be in range of a friendly anthill
		Anthill anthill = getNearbyAnthill(foodTransform.position);
		if (anthill) {
			anthill.addFoodPoints(foodTransform.gameObject.GetComponent<Food>().getFoodValue());
			Destroy(foodTransform.gameObject);
			return;
		}
		
		// Reset the z back to 0 to force the food back underneath the unit
		Vector3 tempPos = foodTransform.localPosition;
		tempPos.z = 0;
		foodTransform.localPosition = tempPos;
		
		// Disable the selectable script so that it doesn't interfere with selecting the underlying unit
		foodTransform.gameObject.GetComponent<Selectable>().enabled = true;
		foodTransform.gameObject.GetComponent<Collider2D>().enabled = true;
		
		// When gatherers are carrying food, they move at half their original speed, so when they drop food, fix the speed stat
		speed *= 2f;
	}
	
	// Gatherers can walk on tiles and food items (but only if they aren't already carrying food themselves)
	protected override bool canWalkOn(GameObject gameObj) {
		// If this object is a child of us, we can safely ignore it
		if (gameObj.transform.parent == transform) return true;
		
		// If it is an unoccupied tile, we can walk on it
		if ((gameObj.GetComponent<Tile>() != null && !gameObj.GetComponent<Tile>().occupiedBy) ||
			gameObj.GetComponent<Scentpath>() != null) {
			return true;
		}
		
		// If it isn't a tile, and also isn't food, then we can't walk on it
		if (gameObj.GetComponent<Food>() == null) return false;
		
		// If it is food, we can only walk on it if we aren't already carrying food
		if (this.gameObject.GetComponentInChildren<Food>() != null) return false;
		
		// If none of the above conditions are met, we can safely walk on it
		return true;
	}
}
