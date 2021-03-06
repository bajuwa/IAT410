﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Handles the map as a whole entity made up of a collection of tiles
 */
public class MapManager : MonoBehaviour {
		
		private float tileHeight = 0f;
		private float tileWidth = 0f;
		private int tileMask = 0;
		private int scentpathMask = 0;	

		// During startup, make sure all map objects are "snapped" to the closest valid position
		void Start (){
			tileMask =  1 << LayerMask.NameToLayer("Tile");
			scentpathMask = 1 << LayerMask.NameToLayer("Scentpath");
		
			// Get all the tiles on the screen
			List<GameObject> objects = new List<GameObject>();
			string[] mapObjectTags = {"Tile", "AntUnit", "MapObject"};
			foreach (string tag in mapObjectTags) {
				GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(tag);
				Debug.Log("Found " + gameObjects.Length.ToString() + " objects with tag '" + tag + "'");
				foreach (GameObject gameObj in gameObjects) {
					objects.Add(gameObj);
				}
			}
			
			if (objects.Count == 0) return;
			Debug.Log("Found " + objects.Count.ToString() + " Total MapObjects");
			
			// In order to calculate the valid positions, we need to take in to account each tiles height/width
			// (use the first tile as a test, assume all are the same)
			tileHeight = objects[0].renderer.bounds.size.y * 0.75f;
			tileWidth = objects[0].renderer.bounds.size.x;
			
			foreach (GameObject obj in objects) {
				// Move the object to the closest valid position on the map (so that all tiles/objects align properly)
				obj.transform.localPosition = getNearestLocation((Vector2) obj.transform.localPosition);
				
				// If we are moving an AntUnit, then we need to get it to record its position for proper pathfinding
				AntUnit antUnitScript = (AntUnit) obj.GetComponent<AntUnit>();
				if (antUnitScript != null) {
					antUnitScript.recordPosition();
				}
			}
		}
	
		// Update is called once per frame
		void Update () {
		
		}
		
		// Returns an array of tile objects that represent all valid adjacent tiles to the given coordinate
		// The given coordinate will be 'snapped' to the closest valid tile position before calculating adjacent tiles
		public Tile[] getAdjacentTiles(Tile originalTile) {
			// Enforce that the position we were given is a valid one
			Vector2 originTilePosition = getNearestLocation(originalTile.gameObject.transform.localPosition);
			//Debug.Log("Getting adjacent tiles to: " + originTilePosition.ToString());
			
			// Get the tile object that is at the valid position 
			CircleCollider2D tileCollider = (CircleCollider2D) Physics2D.OverlapPoint(
				originTilePosition, 
				tileMask
			);
			
			// Now get all tiles that are adjacent to the given tile
			// Note: this relies on the circle colliders of the tiles being large enough to overlap slightly with adjacent tiles
			// Warning: must temporarily convert to world space in case user tries to pan
			Collider2D[] adjacentTileColliders = Physics2D.OverlapCircleAll(
				originTilePosition, 
				tileCollider.radius, 
				tileMask
			);
			
			// Since the physics will pick up our current tile as well, we need to remove it from 'adjacent tiles' positions
			Tile[] adjacentTilePositions = new Tile[adjacentTileColliders.Length - 1];
			int i = 0;
			foreach (Collider2D collider in adjacentTileColliders) {
				// Since a tile will never share the exact same x value as any of its neighbours, we don't need to compare the full vector
				if (collider.gameObject.transform.localPosition.x != originTilePosition.x) {
					adjacentTilePositions[i++] = (Tile) collider.gameObject.GetComponent(typeof(Tile));
				}
			}
			//Debug.Log("Returning positions for " + adjacentTilePositions.Length.ToString() + " adjacent tiles");
			return adjacentTilePositions;
		}
		
		public Tile getTileAtPosition(Vector2 position) {
			return Physics2D.OverlapPoint(position, tileMask) != null ?
				   (Tile) Physics2D.OverlapPoint(position, tileMask).gameObject.GetComponent(typeof(Tile)) :
				   null;
		}
		
		public Scentpath getScentpathAtPosition(Vector2 position) {
			CircleCollider2D tileCollider = (CircleCollider2D) Physics2D.OverlapPoint(
				getTileAtPosition(position).transform.position, 
				scentpathMask
			);
			if (tileCollider) return tileCollider.gameObject.GetComponent<Scentpath>();
			return null;
		}
		
		private Vector2 getNearestLocation(Vector2 position) {
			// Set the y coord to the closest valid 'row'
			position.y = closestDistance (position.y, minIncrement(position.y, tileHeight), maxIncrement(position.y, tileHeight));
			// If we are on an 'odd' row, then stagger it the x coord by half a width so that the hex tiles mesh together
			int rowNumber = (int)Mathf.Abs(position.y/tileHeight) % 2;
			if (rowNumber % 2 == 1) {
				position.x = closestDistance(position.x, minIncrement(position.x, tileWidth)+tileWidth/2f, maxIncrement(position.x, tileWidth)+tileWidth/2f);
			} else {
				position.x = closestDistance(position.x, minIncrement(position.x, tileWidth), maxIncrement(position.x, tileWidth));
			}
			// Set the tile's position to our modified vector
			return position;
		}

		private float diffBetween (float a, float b) {
			return Mathf.Abs (a - b);
		}

		private float closestDistance (float target, float optionOne, float optionTwo) {
			return diffBetween (target, optionOne) < diffBetween (target, optionTwo) ? optionOne : optionTwo;
		}
		
		private float minIncrement(float x, float increment) {
			return Mathf.Floor (x / increment) * increment;
		}
		
		private float maxIncrement(float x, float increment) {
			return Mathf.Ceil (x / increment) * increment;
		}
}
