using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using TaskShape;

/// <summary>
/// Listens to a bunch of events and writes data to a CSV when they are called.
/// </summary>
public class ManipulationDataCollector : MonoBehaviour
{
    public GameObject leftController;
    public GameObject rightController;
    public string dataDir = @"C:\ExperimentData\Manip";
    private StreamWriter ofile;
    private ProjectInputActions controls;

    long GetTimestamp()
    {
        return System.DateTime.Now.Ticks / 10000000;
    }

    // left controller file
    // right controller file
    void OnEnable()
    {
        Directory.CreateDirectory(dataDir);
        string dataFileName = $"manip_{GetTimestamp()}.csv";
        ofile = File.CreateText($"{dataDir}\\{dataFileName}");

        GrabEventSystem.OnGrab.AddListener(GrabCallback);
        GrabEventSystem.OnRelease.AddListener(ReleasedCallback);
        ItemEventSystem.ItemPlaced.AddListener(ItemPlacedCallback);
        ItemEventSystem.GoalReached.AddListener(GoalReachedCallback);

        // set up the button listeners (this is a real pain in the ass)
        controls = new ProjectInputActions();
        InputAction leftGripAction = controls.XRILeftLocomotion.GrabMove;

        leftGripAction.started += LeftPressedCallback;
        leftGripAction.canceled += LeftReleasedCallback;

        InputAction rightGripAction = controls.XRIRightLocomotion.GrabMove;

        rightGripAction.started += RightPressedCallback;
        rightGripAction.canceled += RightReleasedCallback;

        controls.Enable();
    }

    void OnDisable()
    {
        ofile.Flush();
        ofile.Close();
    }

    // grab event callback
    void GrabCallback(GameObject grabbedObj, string hand)
    {
        long epoch = GetTimestamp();
        string line;
        // construct the line
        if (hand == "Right")
        {
            line = $"GrabRight,{rightController.transform.position},{epoch}";
        } else
        {
            line = $"GrabLeft,{leftController.transform.position},{epoch}";
        }

        ofile.WriteLineAsync(line);
    }

    void ReleasedCallback(GameObject grabbedObj, string hand)
    {
        long epoch = GetTimestamp();
        string line;
        // construct the line
        if (hand == "Right")
        {
            line = $"ReleasedRight,{rightController.transform.position},{epoch}";
        } else
        {
            line = $"ReleasedLeft,{leftController.transform.position},{epoch}";
        }

        ofile.WriteLineAsync(line);
    }

    void ItemPlacedCallback(GameObject item, GameObject goal)
    {
        long epoch = GetTimestamp();
        string line = $"ItemPlaced,{item.transform.position},{epoch},{goal.transform.position}";
        ofile.WriteLineAsync(line);
    }
    void GoalReachedCallback(GameObject item, GameObject goal)
    {
        long epoch = GetTimestamp();
        string line = $"GoalReached,{item.transform.position},{epoch},{goal.transform.position}";
        ofile.WriteLineAsync(line);
    }

    void LeftPressedCallback(InputAction.CallbackContext ctx)
    {
        long epoch = GetTimestamp();
        string line = $"PressedLeft,{leftController.transform.position},{epoch}";
        ofile.WriteLineAsync(line);
    }

    void RightPressedCallback(InputAction.CallbackContext ctx)
    {
        long epoch = GetTimestamp();
        string line = $"PressedRight,{rightController.transform.position},{epoch}";
        ofile.WriteLineAsync(line);
    }

    void LeftReleasedCallback(InputAction.CallbackContext ctx)
    {
        long epoch = GetTimestamp();
        string line = $"ReleasedLeft,{leftController.transform.position},{epoch}";
        ofile.WriteLineAsync(line);
    }

    void RightReleasedCallback(InputAction.CallbackContext ctx)
    {
        long epoch = GetTimestamp();
        string line = $"ReleasedRight,{rightController.transform.position},{epoch}";
        ofile.WriteLineAsync(line);
    }
}
