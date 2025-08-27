namespace ReWorkflow.UI;

internal class UI
{
   private readonly ExtensionButton _extensionBtn = new();
   private readonly Main _main;

   internal UI(Main main)
   {
      _main = main;

      _extensionBtn.Icon = Utils.LoadSpriteFromResources("ReWorkflow.Icon.png");
      _extensionBtn.Tooltip = "Switch Edit Mode";
      ExtensionButtons.AddButton(_extensionBtn);
      _extensionBtn.Click = _main.SwitchToggle;
   }
}