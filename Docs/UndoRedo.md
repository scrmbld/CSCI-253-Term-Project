## Undo/Redo Documentation

### For undo/redo to work, the Undo Manager needs to be configured:
- UndoManager: Either the UndoManagerControl or UndoManagerExperimental prefab must exist in the scene hierarchy
    - Only one version of Undo is meant to exist at a time, so if both are in the hierarchy, disable or delete one 
        - UndoManagerControl enables traditional undo: Immediately restoring object's previous state
        - UndoManagerExperimental enables "scrubbing" undo: Move forwards or backwards through object history
    - An UndoManager has been saved as a prefab, containting both the Control and Experimental Manager (disable one) and a metrics tracker script. If it is missing from the hierarchy, add it from the prefab folder
    
### Viewing Quantitative Metrics
- The code will write the metrics to a CSV file at the end of every task, but live metrics are also viewable in Unity
    - In the hierarchy, click on the UndoManager, and select the TestManager
    - The inspector will show live metrics as serializable fields:
        - Time elapsed: total time for the task
        - Scrub time: total time the user spent moving objects through undo/redo (experimental version only)
        - Grab count: total number of times an object was grabbed by the user
        - Undo count: total number of undo actions performed
            - Control: one per button press
            - Experimental: one per rewind (hold and release the button)
        - Redo count: total number of redo actions performed
            - Control: one per button press
            - Experimental: one per fast-forward (hold and release the button)
        - Error correction rate: undo and redo count vs the total number of grabs
            - ERC = (undoCount + redoCount) / grabCount

### For an object to be undone/redone, it must have the following scripts attached:
- Manipulation Control (Script): Allows the object to be moved/rotated
    - Left Controller set to Left Controller
    - Right Controller set to Right Controller
- Undoable Object (Script): Tells the UndoManager that the object state can be saved/restored

### To use undo/redo in the scene:
- Simulator (keyboard controls)
    - z button: Undo (control) or Move backwards in object history (experimental)
    - x button: Redo (control) or Move forwards in object history (experimental)
- Meta Quest (VR Controller)
    - X button: Undo (control) or Move backwards in object history (experimental)
    - Y button: Redo (control) or Move forwards in object history (experimental)

Keyboard mappings exist in UndoManagerControl.cs/UndoManagerExperimental.cs

VR Controller mappings exist in ButtonMapping.cs
