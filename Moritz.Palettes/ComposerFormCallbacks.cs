using System.Windows.Forms;

namespace Moritz.Palettes
{
    public delegate void SetAllFormsExceptChordFormDisEnabled(bool enabled);
    public delegate void BringMainFormToFront();
    public delegate string SettingsPath();
    public delegate string LocalAudioFolderPath();
    /// <summary>
    /// Called whenever a form changes. Updates the main AssistantComposerForm.
    /// </summary>
    public delegate void SettingsHaveChangedDelegate();

    public class ComposerFormCallbacks
    {
        public SetAllFormsExceptChordFormDisEnabled SetAllFormsExceptChordFormEnabledState;
        public BringMainFormToFront BringMainFormToFront;
        public SettingsPath SettingsPath;
        public LocalAudioFolderPath LocalScoreAudioPath;
        public SettingsHaveChangedDelegate UpdateMainForm;
    }
}
