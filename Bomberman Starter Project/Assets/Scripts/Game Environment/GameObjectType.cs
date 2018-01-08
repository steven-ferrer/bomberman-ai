public sealed class GameObjectType {

	private readonly string name;
	private readonly int value;
	private readonly string tag;

	public static readonly GameObjectType DESTRUCTIBLE_WALL = new GameObjectType (1, "Destructible Walls","Destructible"); 
	public static readonly GameObjectType INDESTRUCTIBLE_WALL = new GameObjectType (2, "Indestructible Walls","Indestructible"); 
	public static readonly GameObjectType OUTER_WALL = new GameObjectType (3, "Outer Walls","Indestructible"); 
	public static readonly GameObjectType FLOOR = new GameObjectType (4, "Floors","Floor"); 
	public static readonly GameObjectType BLOCKS_LAYER = new GameObjectType (5, "Blocks"); 
	public static readonly GameObjectType GENERATED_MAP = new GameObjectType (6, "Generated Map"); 
	public static readonly GameObjectType AGENT = new GameObjectType (7, "Agent","Agent"); 
	public static readonly GameObjectType EXPLOSION = new GameObjectType (8, "Explosion","Explosion"); 
	public static readonly GameObjectType BOMB = new GameObjectType (9, "Bomb","Bomb"); 


	public static readonly GameObjectType PLAYER = new GameObjectType (10, "Player",GameObjectType.AGENT.GetTag()); 
	public static readonly GameObjectType AGGRESSIVE_AI = new GameObjectType (11, "Aggressive AI",GameObjectType.AGENT.GetTag()); 


	private GameObjectType(int value, string name,string tag = ""){
		this.name = name;
		this.value = value;
		this.tag = tag;
	}


	public override string ToString(){
		return name;
	}

	public int GetValue(){
		return value;
	}

	public string GetTag(){
		return tag;
	}

}