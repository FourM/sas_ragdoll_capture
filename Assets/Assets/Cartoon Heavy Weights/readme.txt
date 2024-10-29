Models were created using Blender.
Textures were created using GIMP and MapZone.

Using the BlockBreaker script:

Variables in Inspector:
  Debris Prefabs  => 		A list of the prefabs to use as debris after breaking the object
  
  Debris Spawn Points => 	A list of transforms that determine where the debris will appear
  
  Break Force Minimum => 	The minimum amount of force needed on impact to break the object
  
  Enable Break => 		Setting this to FALSE will inhibit ALL breaks
  
  Copy Parent =>		A true value will cause the debris objects to have the same parent as the original
  
  Copy Velocity =>		A true value will cause the debris objects to have the same velocity as the original
  						A false value will mean debris starts with zero velocity (idk when you would use this)
  						
  Random Velocity =>	A vector which will be used to randomize the velocity of the debris.  This is useful
  						for insuring that the block does behave "exactly" the same every time it falls.
  
  Inhibit Break On Bounce => 	Setting this to true will cause the script to check the other
								objects material against Bounciness Min Limit before breaking
  
  Bounciness Min Limit => 		The minimum amount of bounce in the other object that will inhibit the
								breaking of this object.  Only used if Inhibit Break On Bounce = TRUE


There should be as many Debris Spawn Points as there are Debris Prefabs.
  
  If you want to set up different spawn points here is the way I did it.
	1. Drag the "breakable" object prefab into the scene
	2. Add an empty object as a child
	3. Add the debris prefab as a child of the empty
	4. Repeat 2 & 3 for each desired debris object
	5. Adjust the empty(s) so that the debris is in the proper position, rotation and scale to be a 
	   piece of the original object.
	6. Delete the debris prefab(s)
	7. Drag the empty transform(s) into a slot in the Debris Spawn Point array


If the debris prefabs have colliders and rigid-bodies, having them overlap will likely make them fly 
apart when spawned.  So you may have to position them with slight gaps between the colliders.  