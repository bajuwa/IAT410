﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WarriorUnit : AntUnit {

	public GameObject combatCloud;
	
	private EventManager eventManager;
	
	//To be displayed on the GUI
	public override string getDescription() {
		if (isNeutralOrFriendly()) 
			return "A good offensive unit that is able to attack enemy ants and anthills.";
		else
			return "Danger! This ant can kill your ants and destroy your anthill, attack it before it attacks you!";
	}
	
	public override string getName() {
		return "Warrior";
	}
	
	private Attackable attackTarget;
	private Tile attackTargetLastKnowTileLocation;
	
	// Since we can cancel our own battle with an anthill by 'walking away', use a flag to detect this scenario and break up the battle
	private bool hasSetNewPath = false;

	// Use this for initialization
	protected override void Start() {
		base.Start();
	}
	
	protected override void loadDisplayImage() {
		if (currentHp/maxHp <= .33f) {
			displayImage = getTextureFromPlayer("warriorDisplayDying");
		} else if (currentHp/maxHp <= .66f) {
			displayImage = getTextureFromPlayer("warriorDisplayDamaged");
		} else {
			displayImage = getTextureFromPlayer("warriorDisplayHealthy");
		}
	}
	
	protected override string getAnimationName() {
		return "warriorAnimator";
	}
	
	// Update is called once per frame
	protected override void Update() {
		base.Update();
		
		if (!eventManager) eventManager = GameObject.Find("EventManager").GetComponent<EventManager>() as EventManager;
		
		// Base our movement/pathfinding off of our attack target (if any)
		if (attackTarget != null) {
			Tile attackTargetCurrentLocation = mapManager.getTileAtPosition(attackTarget.transform.position);
			
			// If we have landed on the target, commence a 'battle', otherwise keep moving towards the target
			if (mapManager.getTileAtPosition(transform.position) == attackTargetCurrentLocation) {
				Debug.Log("Found target!");
				StartCoroutine(commenceBattle(attackTarget));
				clearAttackTarget();
			} else if (attackTargetLastKnowTileLocation != attackTargetCurrentLocation || 
					   (attackTargetLastKnowTileLocation == attackTargetCurrentLocation && targetPath.getTilePath().Count == 0)) {
				Debug.Log("Calculating route to target!");
				attackTargetLastKnowTileLocation = attackTargetCurrentLocation;
				StopCoroutine("moveToTarget");
				StartCoroutine(moveToTarget(attackTargetCurrentLocation, true));
			}
			
			// If the next tile in our path has an anthill we are attacking on it, stop early and attack from afar
			if (targetPath.getTilePath().Count > 0) {
				Tile nextTile = targetPath.getTilePath().Peek();
				Collider2D anthillCollider = Physics2D.OverlapPoint(
					nextTile.transform.position,
					anthillMask
				);
				if (anthillCollider) {
					Debug.Log("Attack the anthill!");
					targetPath.setNewTileQueue(new Queue<Tile>());
					StartCoroutine(commenceBattle(attackTarget));
					clearAttackTarget();
				}
			}
		}
		
		// Determine what animation we should be playing
		if (attackTarget) setAnimation(2);
		else if (getCurrentTile() != getTargetTile()) setAnimation(1);
		else setAnimation(0);
	}
	
	public IEnumerator commenceBattle(Attackable opponent) {
		// If we are already in a battle, we should abort the current commenceBattle request
		Debug.Log(isInBattle);
		if (isInBattle) yield break;
		Debug.Log("Commencing attack!");
		eventManager.addEvent(opponent.gameObject.transform.position);
		
		// If we are attacking an anthill, the battle will behave somewhat differently
		Anthill anthill = opponent.GetComponent<Anthill>();
		hasSetNewPath = false;
		
		// Set both units to be in 'attack mode' and generate a 'combat cloud'
		GameObject cloud = null;
		opponent.startBattle();
		if (!anthill) {
			isInBattle = true;
			Vector3 pos = mapManager.getTileAtPosition(transform.position).transform.position;
			if (Network.isClient || Network.isServer) {
				cloud = Network.Instantiate(combatCloud,
											new Vector3(pos.x, pos.y, transform.position.z - 1),
											Quaternion.identity, 0) as GameObject;
				networkView.RPC("fixCombatCloud", RPCMode.All, cloud.networkView.viewID);
			}
			else {
				cloud = GameObject.Instantiate(combatCloud, 
										   new Vector3(pos.x, pos.y, transform.position.z - 1), 
										   Quaternion.identity) as GameObject;
				}
			if (getPlayerId() == 1) {
				cloud.transform.Find("RedHead").gameObject.GetComponent<SpriteRenderer>().sprite = this.getFightSprite();
				cloud.transform.Find("BlueHead").gameObject.GetComponent<SpriteRenderer>().sprite = opponent.getFightSprite();
			} else {
				cloud.transform.Find("RedHead").gameObject.GetComponent<SpriteRenderer>().sprite = opponent.getFightSprite();
				cloud.transform.Find("BlueHead").gameObject.GetComponent<SpriteRenderer>().sprite = this.getFightSprite();
			}
			cloud.transform.parent = GameObject.Find("Objects").transform;
			gameObject.renderer.enabled = false;
			opponent.gameObject.renderer.enabled = false;
		} else {
			// If we are attacking an anthill, make sure to 'face' it
			if (transform.position.x < opponent.transform.position.x) transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
		}
		
		// Every second, battle calculations should take place only if we haven't started moving again
		while (true) {
			// Do one round of battle calculations
			exchangeBlows(this, opponent);
			
			// Wait for 1 seconds
			Debug.Log("AttackableOne: " + this.currentHp + ", AttackableTwo: " + opponent.currentHp);
			yield return new WaitForSeconds(1);
			
			// If we are fighting an anthill, we can be interrupted by another battle during our yield
			if (anthill && isInBattle) {
				Debug.Log("Interrupted by another battle!");
				break;
			}
			
			// If we are fighting an anthill, can interrupt our own battle by leaving
			if (anthill && hasSetNewPath) {
				Debug.Log("Cancelled attack on anthill due to moving away!");
				break;
			}
			
			// Check if one of the units has died
			if (this.currentHp <= 0 || opponent.currentHp <= 0) {
				Debug.Log("Cancelled attack since at least one of us is dead!");
				break;
			}
		}
		
		hasSetNewPath = false;
		
		// After the battle, 'cleanup' the unit(s) and the combat cloud
		if (cloud) {
			if (Network.isClient || Network.isServer) {
				Network.Destroy(cloud);
			}
			else {
				GameObject.Destroy(cloud);
			}
		}
		if (transform.localScale.x < 0) transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
		gameObject.renderer.enabled = true;
		opponent.gameObject.renderer.enabled = true;
		this.removeFromBattle();
		opponent.removeFromBattle();
		if (opponent.currentHp <= 0) opponent.kill();
		// Note: if this unit dies, make sure to delete it at the end!
		if (this.currentHp <= 0) this.kill();
	}
	
	[RPC] public void fixCombatCloud(NetworkViewID combatCloudNetViewID) {
		NetworkView combatCloudNetView = NetworkView.Find(combatCloudNetViewID);
		GameObject combatCloud = combatCloudNetView.gameObject;
		combatCloud.transform.parent = GameObject.Find("Objects").transform;
	}
	
	public override Sprite getFightSprite() {
		return getSpriteFromPlayer("warriorSprite");
	}
	
	private void exchangeBlows(Attackable antOne, Attackable antTwo) {
		// Calculate each ant's attack based on their remaining health and base attack stat
		Debug.Log(antOne.currentHp / antOne.maxHp);
		float antOneAttack = antOne.attack * (antOne.currentHp / antOne.maxHp); //TODO: dynamic attack calculations
		float antTwoAttack = antTwo.attack * (antTwo.currentHp / antTwo.maxHp); //TODO: dynamic attack calculations
		
		// Calculate damage dealt by factoring in eachothers defenses and apply to current hp
		float hpOne = Mathf.Max(Random.Range(antTwoAttack*0.5f, antTwoAttack*1.5f) - antOne.defense, 0);
		float hpTwo = Mathf.Max(Random.Range(antOneAttack*0.5f, antOneAttack*1.5f) - antTwo.defense, 0);
		if (Network.isClient || Network.isServer) {
			NetworkView networkAntOne = antOne.networkView;
			NetworkView networkAntTwo = antTwo.networkView;
			networkView.RPC("setHPNetwork", RPCMode.All, hpOne, hpTwo, networkAntOne.viewID, networkAntTwo.viewID);
		}
		else {
		antOne.currentHp -= hpOne;
		antTwo.currentHp -= hpTwo;
		}
	}
	
	[RPC] private void setHPNetwork(float hpOne, float hpTwo, NetworkViewID antOne, NetworkViewID antTwo) {
		NetworkView networkAntOne = NetworkView.Find(antOne);
		NetworkView networkAntTwo = NetworkView.Find(antTwo);
		GameObject gameObjectAntOne = networkAntOne.gameObject;
		GameObject gameObjectAntTwo = networkAntTwo.gameObject;
		Attackable attAntOne = gameObjectAntOne.GetComponent<Attackable>();
		Attackable attAntTwo = gameObjectAntTwo.GetComponent<Attackable>();
		attAntOne.currentHp -= hpOne;
		attAntTwo.currentHp -= hpTwo;
	}
	
	// If a warrior comes in to contact with it's target, interrupt its movement so that 
	// they will eventually overlap to start a 'battle'
	void OnTriggerEnter2D(Collider2D other) {
		if (attackTarget != null && other.gameObject.GetComponent<Attackable>() == attackTarget) {
			Debug.Log("Collided with target!");
			attackTarget.interrupt();
		}
	}
	
	public void setTarget(Attackable target) {
		Debug.Log("New target: " + target.ToString());
		// Store data regarding our new target
		attackTarget = target;
		attackTargetLastKnowTileLocation = mapManager.getTileAtPosition(target.transform.position);
	}
	
	private void clearAttackTarget() {
		attackTarget = null;
		attackTargetLastKnowTileLocation = null;
	}
	
	// If a warrior is given a move command, we need to clear the attack target or else the unit will keep moving towards teh target
	public override IEnumerator moveTo(Tile tileToMoveTo, bool activelySetNewTarget = false) {
		Debug.Log("calling moveTo");
		hasSetNewPath = activelySetNewTarget;
		clearAttackTarget();
		return base.moveTo(tileToMoveTo, activelySetNewTarget);
	}
	
	// Move to a target (without clearing attack target like moveTo)
	private IEnumerator moveToTarget(Tile tileToMoveTo, bool activelySetNewTarget = false) {
		hasSetNewPath = activelySetNewTarget;
		Debug.Log("calling moveToTarget");
		return base.moveTo(tileToMoveTo, activelySetNewTarget);
	}
	
	// Warriors can walk on tiles and food items (but only if they aren't already carrying food themselves)
	protected override bool canWalkOn(GameObject gameObj) {
		if (gameObj.GetComponent<Scentpath>() != null) {
			return true;
		}
		
		if (gameObj.GetComponent<Tile>() != null) {
			if (attackTarget && gameObj.GetComponent<Tile>().occupiedBy != null && gameObj.GetComponent<Tile>().occupiedBy != attackTarget.gameObject) return false;
			return true;
		}
		
		if (attackTarget) {
			if (gameObj == attackTarget.gameObject) return true;
		
			// Since food can be carried by units, check for that too
			if (gameObj.transform.parent != null && gameObj.transform.parent == attackTarget.gameObject.transform) return true;
		}
		
		return false;
	}
}
