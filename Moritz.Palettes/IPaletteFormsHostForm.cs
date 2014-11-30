
namespace Moritz.Palettes
{
    public interface IPaletteFormsHostForm
    {
        /// <summary>
        /// Called whenever a setting changes in a palette or ornament form.
        /// </summary>
        void UpdateForChangedPaletteForm();
        /// <summary>
        /// Used when reverting palette or ornament forms.
        /// </summary>
        string SettingsPath { get; }
        /// <summary>
        /// Used by the audio demo buttons
        /// </summary>
        string LocalScoreAudioPath { get; }
        /// <summary>
        /// Called when a PaletteChordForm is created or closed.
        /// When a PaletteChordForm is open, all other forms are disabled.
        /// Otherwise, they are all enabled.
        /// </summary>
        void SetAllFormsExceptChordFormEnabledState(bool enabledState);
    }
}
