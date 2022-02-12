# ComplexGameSystem
This system centers around three different types of ScriptableObject: MoveSet, Attack, and DamageObject.

To create these objects right-click in the project window and find them under Create/ComboSystem.

To edit the MoveSet double-click on the object.

Edit the Attack in the inspector right clicking on the ColliderTest bar to add or remove slider handles.

DamageObject will likely require you to create a derived class unless you only intend to do a set amount of unmodifiable damage with every hit. This can be done easily using the DamageOverride template found in Create/ComboSystem.

The last thing you’ll need is to add the ComboController component to your fighter, attach the relevant MoveSet and Animator, then assign the controls the fighter can use in the inputs section. You can also adjust how many colliders your attack can hit at any given frame with the hitColliderCount, how long the player has inbetween inputs before the combo resets with ComboClearTimer and finally what your attack can hit with hitmask. When something does get hit by an attack it sends the collider and damage data to an IAttacker which needs to be assigned in a class inheriting from IAttacker to manage the damage outcome.

All of these scripts can be found in the ComboSystem namespace.
