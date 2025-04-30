using HarmonyLib;

namespace ReWorkflow;

[Plugin("ReWorkflow")]
public class Main
{
    [Init]
    private void Init()
    {
        var harmony = new Harmony("kvl.cm.reworkflow");
        harmony.PatchAll();
    }

    [Exit]
    private void Exit()
    {
    }
}