
namespace Moritz.Palettes
{
    /// <summary>
    /// This interface is implemented by
    ///     AssistantComposerMainForm
    ///     DimensionsAndMetadataForm
    ///     PaletteForm
    ///     PaletteChordForm
    ///     OrnamentSettingsForm
    /// using an embedded RevertableFormFunctions object containing the common code.
    /// </summary>
    public interface IRevertableForm
    {
        /// <summary>
        /// One or more of the settings in the form contains an error
        /// </summary>
        bool HasError { get; }
        /// <summary>
        /// The settings have been changed, but the form's OkayToSaveButton has not yet been clicked.
        /// </summary>
        bool NeedsReview { get; }
        /// <summary>
        /// The settings have been changed and checked in the form, but the settings have not yet been saved to a file on disk.
        /// </summary>
        bool HasBeenChecked { get; }
    }
}
