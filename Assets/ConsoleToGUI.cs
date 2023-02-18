using UnityEngine;

public class ConsoleToGUI : MonoBehaviour {
    string myLog = "*begin log";
    string filename = "";
    bool doShow = false;
    int kChars = 700;
    void OnEnable() { Application.logMessageReceived += Log; }
    void OnDisable() { Application.logMessageReceived -= Log; }
    void Update() { if (Input.GetKeyDown(KeyCode.Space)) { doShow = !doShow; } }
    public void Log(string logString, string stackTrace, LogType type) {
        // for onscreen...
        string log_brf = logString + "\n";
        string log_dtl = log_brf + "------------" + stackTrace + "------------\n";
        myLog = log_brf + myLog;
        if (myLog.Length > kChars) { myLog = myLog.Substring(0, kChars); }

        // for the file ...
        if (filename == "") {
            string d;
#if UNITY_EDITOR
            d = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/YOUR_LOGS";

#elif UNITY_STANDALONE
            d = Application.dataPath + "/YOUR_LOGS";
#endif
            System.IO.Directory.CreateDirectory(d);
            string r = Random.Range(1000, 9999).ToString();
            filename = d + "/log-" + r + ".txt";
        }
        try { System.IO.File.AppendAllText(filename, log_brf); } catch { }
    }

    void OnGUI() {
        if (!doShow) { return; }
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
        new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));
        GUI.TextArea(new Rect(10, 10, 540, 370), myLog);
    }
}