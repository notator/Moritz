
using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
    ///<summary>
    /// IUniqueDef is implemented by all objects that can be added to a VoiceDefs.UniqueDefs list.
    /// Currently (11.9.2014) these are:
    ///     MidiChordDef
    ///     InputChordDef
    ///     RestDef
    ///     CautionaryChordDef
    ///     ClefChangeDef
    /// These objects must implement DeepClone() so that VoiceDefs.DeepClone() can be implemented.
    /// VoiceDefs is used for composition. When complete, the UniqueDefs list is transferred to
    /// Voice before the definitions are converted to the objects themselves.
    ///</summary>
    public interface IUniqueDef
    {
        string ToString();

        /// <summary>
        /// Returns a deep clone (a unique object)
        /// </summary>
        IUniqueDef DeepClone();

        void AdjustMsDuration(double factor);

        int MsDuration { get; set; }
        int MsPosition { get; set; }
    }
}
