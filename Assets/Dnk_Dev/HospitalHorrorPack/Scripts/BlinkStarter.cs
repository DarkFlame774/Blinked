using System.Diagnostics;
using UnityEngine;


public class BlinkStarter : MonoBehaviour
{
    void Start()
    {
        string path = Application.streamingAssetsPath + "/blink_detector.exe";
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = path,
            CreateNoWindow = true,
            UseShellExecute = false
        };
        Process.Start(psi);
    }

    private void OnApplicationQuit()
    {
        foreach (var process in Process.GetProcessesByName("blink_detector"))
        {
            process.Kill();
        }
    }
}