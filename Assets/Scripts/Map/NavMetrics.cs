using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using TaskShape;
using UnityEditor.AnimatedValues; // We need the events from this namespace

/// <summary>
/// Listens to a bunch of events and writes data to a CSV when they are called.
/// </summary>
public class MapMetrics : MonoBehaviour
{
    // file stuff
    public string dataDir = @"C:\ExperimentData\Map";
    private StreamWriter ofile;


    // controller data sources
    public GameObject leftController;
    public GameObject rightController;
    private ProjectInputActions controls;
    public Transform head;

    long GetTimestamp()
    {
        return System.DateTime.Now.Ticks / 10000000;
    }

    // left controller file
    // right controller file
    void OnEnable()
    {
        Directory.CreateDirectory(dataDir);
        string dataFileName = $"map_{GetTimestamp()}.csv";
        // this is how we open the file
        // we use a StreamWriter to write to the file
        ofile = File.CreateText($"{dataDir}\\{dataFileName}");

        // we log the 
        GrabEventSystem.OnGrab.AddListener(GrabCallback);
        GrabEventSystem.OnRelease.AddListener(ReleasedCallback);
        

        //// set up the button listeners (this is a real pain in the ass)
        //controls = new ProjectInputActions();
        //InputAction leftGripAction = controls.XRILeftLocomotion.GrabMove;

        //leftGripAction.started += LeftPressedCallback;
        //leftGripAction.canceled += LeftReleasedCallback;

        //InputAction rightGripAction = controls.XRIRightLocomotion.GrabMove;

        //rightGripAction.started += RightPressedCallback;
        //rightGripAction.canceled += RightReleasedCallback;

        //controls.Enable();
    }

    void OnDisable()
    {
        ofile.Flush();
        ofile.Close();
    }

    private void Update()
    {
        long epoch = GetTimestamp();
        string line = $"Update path,{head.position},{epoch}";
        ofile.WriteLineAsync(line);
        // Get the right controller device
        UnityEngine.XR.InputDevice rightHand =
            UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);

        if (!rightHand.isValid || head)
            return;

        bool bDown = rightHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool bvalue) && bvalue;

        if (bDown)
        {
            long epoch1 = GetTimestamp();
             string line1 = $"MapToggled,{head.position},{epoch1}";
            ofile.WriteLineAsync(line);

        }
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
        }
        else
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
        }
        else
        {
            line = $"ReleasedLeft,{leftController.transform.position},{epoch}";
        }

        ofile.WriteLineAsync(line);
    }

    //void ItemPlacedCallback(GameObject item, GameObject goal)
    //{
    //    long epoch = GetTimestamp();
    //    string line = $"ItemPlaced,{item.transform.position},{epoch},{goal.transform.position}";
    //    ofile.WriteLineAsync(line);
    //}
    //void GoalReachedCallback(GameObject item, GameObject goal)
    //{
    //    long epoch = GetTimestamp();
    //    string line = $"GoalReached,{item.transform.position},{epoch},{goal.transform.position}";
    //    ofile.WriteLineAsync(line);
    //}

    //void LeftPressedCallback(InputAction.CallbackContext ctx)
    //{
    //    long epoch = GetTimestamp();
    //    string line = $"PressedLeft,{leftController.transform.position},{epoch}";
    //    ofile.WriteLineAsync(line);
    //}

    //void RightPressedCallback(InputAction.CallbackContext ctx)
    //{
    //    long epoch = GetTimestamp();
    //    string line = $"PressedRight,{rightController.transform.position},{epoch}";
    //    ofile.WriteLineAsync(line);
    //}

    //void LeftReleasedCallback(InputAction.CallbackContext ctx)
    //{
    //    long epoch = GetTimestamp();
    //    string line = $"ReleasedLeft,{leftController.transform.position},{epoch}";
    //    ofile.WriteLineAsync(line);
    //}

    //void RightReleasedCallback(InputAction.CallbackContext ctx)
    //{
    //    long epoch = GetTimestamp();
    //    string line = $"ReleasedRight,{rightController.transform.position},{epoch}";
    //    ofile.WriteLineAsync(line);
    //}
}