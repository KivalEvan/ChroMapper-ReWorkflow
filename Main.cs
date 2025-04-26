using UnityEngine.SceneManagement;

namespace ReWorkflow;

[Plugin("ReWorkflow")]
public class Main
{
    [Init]
    private void Init()
    {
        SelectionControllerPatch.PatchShitUp();
    }

    [Exit]
    private void Exit()
    {
    }
}