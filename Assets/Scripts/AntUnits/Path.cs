﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
		
/**
 * A path made up of tiles that also tracks the cumulative terrain values 
 * within the path and the heuristic value of the lastTileInPath
 * Note: Inheriting from the 'ScriptableObject' class allows us access to the GetInstanceID method
 *		 which we will be using when AntUnits must 'select' their determined paths route
 */
public class Path : ScriptableObject, IComparable<Path> {
	private float summedPathValue = 0;
	private float heuristicValue = 0;
	private Tile lastTileInPath;
	private Queue<Tile> tilePath = new Queue<Tile>();
	
	public void init(Tile startingTile, float startingHeuristic, float startingCost) {
		this.add(startingTile, startingHeuristic, startingCost);
	}
	
	public void init(Path originalPath) {
		float cost = originalPath.getSummedPathValue();
		foreach (Tile tile in originalPath.getTilePath()) {
			this.add(tile, originalPath.getHeuristicValue(), cost);
			cost = 0;
		}
	}
	
	public void setNewTileQueue(Queue<Tile> queue) {
		deselectPath();
		tilePath = queue;
	}
	
	public void add(Tile tile, float hValue, float cost) {
		if (!tile) return;
		summedPathValue += cost;
		heuristicValue = hValue;
		lastTileInPath = tile;
		tilePath.Enqueue(tile);
	}
	
	public Tile pop() {
		Tile tile = tilePath.Dequeue();
		tile.deselect(GetInstanceID());
		return tile;
	}
	
	public float getFullValue() {
		return summedPathValue + heuristicValue;
	}
	
	public float getHeuristicValue() {
		return heuristicValue;
	}
	
	public float getSummedPathValue() {
		return summedPathValue;
	}
	
	public Queue<Tile> getTilePath() {
		return tilePath;
	}
	
	public Tile getLastTileInPath() {
		return lastTileInPath;
	}
	
	public Tile clearPath() {
		Tile lastTile = null;
		while (tilePath.Count > 0) {
			lastTile = pop();
		}
		return lastTile;
	}
	
	public void selectPath() {
		foreach (Tile tile in tilePath) {
			tile.select(GetInstanceID());
		}
	}
	
	public void deselectPath() {
		foreach (Tile tile in tilePath) {
			tile.deselect(GetInstanceID());
		}
	}
		
	public int CompareTo(Path other) {
		if (getFullValue() < other.getFullValue()) return -1;
		if (getFullValue() == other.getFullValue()) return 0;
		if (getFullValue() > other.getFullValue()) return 1;
		return 0;
	}
	
	public void printPath() {
		Debug.Log("Path: ");
		foreach (Tile tile in tilePath) {
			Debug.Log(tile.ToString());
		}
	}
	
	~Path() {
		this.deselectPath();
	}
}
