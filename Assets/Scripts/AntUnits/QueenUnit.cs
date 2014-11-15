using UnityEngine;

public class QueenUnit : AntUnit {

	public GameObject anthillPrefab;
	
	//To be displayed on the GUI
	public override string getDescription() {
		if (isNeutralOrFriendly()) 
			return "The Queen can be used to create one new Anthill on top of old Anthill ruins!";
		else
			return "Oh no!  The enemy's Queen is trying to build a new Anthill for them, stop her!";
	}
	
	public override string getName() {
		return "Queen";
	}
	
	// Use this for initialization
	protected override void Start() {
		base.Start();
	}
	
	protected override void loadDisplayImage() {
		displayImage = getTextureFromPlayer("queenDisplay");
	}
	
	public override Sprite getFightSprite() {
		return getSpriteFromPlayer("queenSprite");
	}
	
	protected override string getAnimationName() {
		return "queenAnimator";
	}
	
	// Update is called once per frame
	protected override void Update() {
		base.Update();
		
		// If our queen lands on a DeadAnthill, create a new Anthill
		if (getCurrentTile() == getTargetTile() && targetPath != null && targetPath.getTilePath().Count == 0) {
			Collider2D[] itemsOnSameTile = Physics2D.OverlapPointAll(transform.position);
			foreach (Collider2D col in itemsOnSameTile) {
				if (col.gameObject.GetComponent<DeadAnthill>() != null) {
					Debug.Log("Detected dead anthill on current tile, creating new anthill");
					createNewAnthill(col.gameObject);
				}
			}
		}
		
		// Determine what animation we should be playing
		if (getCurrentTile() != getTargetTile()) setAnimation(1);
		else setAnimation(0);
	}
	
	private void createNewAnthill(GameObject deadAnthillObject) {
		// Create the new Anthill object under the same ownership as the queen unit
		GameObject anthillObject = GameObject.Instantiate(
			anthillPrefab,
			mapManager.getTileAtPosition(transform.position).gameObject.transform.position,
			Quaternion.identity
		) as GameObject;
		anthillObject.GetComponent<Ownable>().ownedBy = this.ownedBy;
		anthillObject.transform.parent = GameObject.Find("Objects").transform;
		anthillObject.transform.localPosition = new Vector3(
			anthillObject.transform.localPosition.x,
			anthillObject.transform.localPosition.y,
			0
		);
		
		// Delete both the queen unit and the dead anthill
		GameObject.Destroy(deadAnthillObject);
		this.kill();
	}
	
	// Warriors can walk on tiles and food items (but only if they aren't already carrying food themselves)
	protected override bool canWalkOn(GameObject gameObj) {
		if (gameObj.GetComponent<Scentpath>() != null) return true;
		
		if (gameObj.GetComponent<Tile>() != null) {
			if (gameObj.GetComponent<Tile>().occupiedBy != null) return false;
			return true;
		}
		
		if (gameObj.GetComponent<DeadAnthill>() != null) return true;
		
		return false;
	}
}
