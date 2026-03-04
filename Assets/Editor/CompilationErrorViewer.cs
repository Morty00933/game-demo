using UnityEditor;
using UnityEngine;
using System.Linq;

public class CompilationErrorViewer : EditorWindow
{
    [MenuItem("Tools/Show Compilation Errors")]
    public static void ShowErrorsWindow()
    {
        string[] errorMessages = GetCompilationErrors();

        if (errorMessages.Length == 0)
        {
            EditorUtility.DisplayDialog(" омпил€ци€", "ќшибок компил€ции не найдено!", "ќк");
        }
        else
        {
            string message = string.Join("\n\n", errorMessages.Take(20)); // максимум 20 строк, чтобы не захламл€ть
            EditorUtility.DisplayDialog("ќшибки компил€ции", message, "ќк");
        }
    }

    private static string[] GetCompilationErrors()
    {
        return LogEntriesWrapper.GetConsoleErrors();
    }
}

internal static class LogEntriesWrapper
{
    private static System.Type logEntriesType = System.Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
    private static System.Type logEntryType = System.Type.GetType("UnityEditor.LogEntry,UnityEditor.dll");
    private static object logEntryInstance = System.Activator.CreateInstance(logEntryType);

    private static int GetCount()
    {
        return (int)logEntriesType.GetMethod("GetCount").Invoke(null, null);
    }

    private static void GetEntryInternal(int index, object entry)
    {
        logEntriesType.GetMethod("GetEntryInternal").Invoke(null, new object[] { index, entry });
    }

    public static string[] GetConsoleErrors()
    {
        int count = GetCount();
        System.Collections.Generic.List<string> errors = new();

        var conditionField = logEntryType.GetField("condition");
        var modeField = logEntryType.GetField("mode");

        for (int i = 0; i < count; i++)
        {
            GetEntryInternal(i, logEntryInstance);
            int mode = (int)modeField.GetValue(logEntryInstance);
            string message = conditionField.GetValue(logEntryInstance) as string;

            // mode: 2 Ч Error, 4 Ч Warning, 1 Ч Log
            if ((mode & 2) != 0) // is error
            {
                errors.Add(message);
            }
        }

        return errors.ToArray();
    }
}
