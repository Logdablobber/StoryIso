namespace StoryIso.Enums;

public enum FunctionType
{
	Print, // prints the following args as a string
	SetPlayerPos, // sets the player's position, takes in 2 ints as tile position
	Move, // moves the player at a constant speed, takes in 2 floats as the x and y speed
	MoveTo, // moves the player to a position over time, takes in a position as 2 floats and a float as speed
	SetPlayerDirection, // sets the player's direction, takes in the direction as an enum
	LoadMap, // loads a map, takes in the map name as a string and the starting position as 2 ints
	SetTile, // sets a tile, takes in the layer name as an enum, the tile guid as an int, and the x and y as 2 ints
	SetRow, // sets a row of tiles, takes in the layer name as an enum, the starting x and y as ints, and ints for the row
	SetCol, // sets a column of tiles, takes in the layer name as an enum, the starting x and y as ints, and ints for the column
	RemoveTile, // removes a tile, takes in the layer name as an enum and the x and y as 2 ints
	ToggleCollider, // toggles a collider, takes in the collider name as a string
	SetCollider, // sets the state of a collider (enabled/disable), takes in the colliders name as a string and new state as a boolean
	RefreshMap, // refreshes tilemap
	RunDialogue, // runs dialogue sequence, takes in the name of the dialogue as a string
	EndDialogue, // ends currently running dialogue sequence
	RunScene, // runs scene, takes in scene name as a string
	DefineVar, // defines a variable, takes in the type, name of the variable as an object, and the value of that type
	SetVar, // sets a variable, takes in the variable's name as an object & new value
	GOTO, // goes to a line, takes in an int as the line to go to
	GOTOIF, // cannot be used in code, only for if statements
	SetVisible, // sets character visibility, takes in a string for the character name and a bool for the new value
	MoveCharacter, // moves a character to a tile, takes in a string for the character name, two floats for the tile x and y, and a float for speed
	SetCharacterPos, // sets a character's position, takes in a string for the character name and two floats for the tile x and y
	SetCharacterDirection, // sets a character's direction, takes in a string for the character and the direction as an enum
	SetCharacterRoom, // sets the room a character is in, takes in a string for the character and a string for the room ('#any#' makes the character render in ALL rooms)
	Wait, // waits a set amount of time, takes in the amount of time in seconds as a float (DO NOT USE ON SYNCHRONOUS SCRIPTS!!!)
	UnlockPlayerMovement, // unlocks the player's movement. It is locked by default when a script starts.
	LockPlayerMovement, // locks a player's movement. It is locked by default when a script starts.
	None
}