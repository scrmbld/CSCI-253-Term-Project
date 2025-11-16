# Undo/Redo Documentation

## For undo/redo to work, the Undo Manager needs to be configured:
- UndoManager: Either the UndoManagerControl or UndoManagerExperimental prefab must exist in the scene hierarchy
    - Only one version of Undo is meant to exist at a time, so if both are in the hierarchy, disable or delete one 
        - UndoManagerControl enables traditional undo: Immediately restoring object's previous state
        - UndoManagerExperimental enables "scrubbing" undo: Move forwards or backwards through object history
    - Both UndoManagers have been saved as prefabs. If it is missing from the hierarchy, add it from the prefab folder

## For an object to be undone/redone, it must have the following scripts attached:
- Manipulation Control (Script): Allows the object to be moved/rotated
    - Left Controller set to Left Controller
    - Right Controller set to Right Controller
- Undoable Object (Script): Tells the UndoManager that the object state can be saved/restored

## To use undo/redo in the scene:
- Simulator (keyboard controls)
    - z button: Undo (control) or Move backwards in object history (experimental)
    - x button: Redo (control) or Move forwards in object history (experimental)
- Meta Quest (VR Controller)
    - X button: Undo (control) or Move backwards in object history (experimental)
    - Y button: Redo (control) or Move forwards in object history (experimental)

Keyboard mappings exist in UndoManagerControl.cs/UndoManagerExperimental.cs

VR Controller mappings exist in ButtonMapping.cs
