namespace Moritz.Spec
{
    ///<summary>
    /// IUniqueDef is implemented by all objects that can be added to a ChannelDefs.UniqueDefs list.
    /// Currently (11.9.2014) these are:
    ///     MidiChordDef
    ///     MidiRestDef
    ///     CautionaryChordDef
    ///     ClefDef
    /// These objects must implement DeepClone() so that ChannelDefs.DeepClone() can be implemented.
    /// ChannelDefs is used for composition. When complete, the UniqueDefs list is used to create
    /// NoteObjects (in Voice objects) that also contain their (graphic) representations.
    ///</summary>
    public interface IUniqueDef : System.ICloneable
    {
        string ToString();

        /// <summary>
        /// Returns a deep clone (a unique object)
        /// </summary>
        new object Clone();

        int MsDuration { get; set; }
        int MsPositionReFirstUD { get; set; }
    }
}
