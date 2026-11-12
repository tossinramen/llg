using UnityEditor;

namespace LLG.EditorTools
{
    /// <summary>
    /// Makes Unity wait until you press Stop before loading changed scripts.
    /// Without this, editing code while the game is playing reloads it mid-game and breaks it
    /// (that is what caused the "DivideByZeroException" on the start screen).
    /// Same as Edit > Preferences > General > Script Changes While Playing > Recompile After Finished Playing.
    /// </summary>
    [InitializeOnLoad]
    static class EditorSetup
    {
        const int RecompileAfterFinishedPlaying = 1;

        static EditorSetup()
        {
            if (EditorPrefs.GetInt("ScriptCompilationDuringPlay", 0) == 0)
                EditorPrefs.SetInt("ScriptCompilationDuringPlay", RecompileAfterFinishedPlaying);
        }
    }
}
