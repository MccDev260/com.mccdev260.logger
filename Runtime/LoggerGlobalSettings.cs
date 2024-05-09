using UnityEngine;

[CreateAssetMenu(fileName = "LoggerSettings", menuName = "MccDev260/LoggerSettings", order = 100)]
public class LoggerGlobalSettings : ScriptableObject
{
    [SerializeField] bool recordInEditor;

    public bool CanRecord 
    { 
        get 
        {
            if (recordInEditor)
            {
                return true;
            }

            return !Application.isEditor;
        } 
    }
}
