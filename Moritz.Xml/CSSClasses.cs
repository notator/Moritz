namespace Moritz.Xml
{
    /// <summary>
    /// This enum contains _all_ the element classes that are written to SVG scores.
    /// Not all these classes will have CSS style definitions.
    /// The style definitions are minimized, taking into account the way the elements nest.
    /// </summary>
    public enum CSSClass
    {
        none,    // Exists and is recorded, but is never used as an element class and never given a CSS definition!
        frame,       // Used and defined on non-scroll pages. Not recorded.
        timeStamp,  // Used and defined on every page. Not recorded.
        titles,  // Used and defined on page 1. Not recorded.
        mainTitle,  // Used and defined on page 1. Not recorded. 
        author,     // Used and defined on page 1. Not recorded.
        systems, // Used and defined on every page. Not recorded.
        system, // Used and defined on every page. Not recorded.
        staffName, inputStaffName, // recorded and used
        staff, inputStaff, // recorded and used
        stafflines, inputStafflines, // used but not recorded, inherits definition
        staffline, inputStaffline, // used but not recorded, inherits definition
        voice, inputVoice, // used, but has no Metrics, inherits definition from staff or inputStaff

        graphics, // all event symbol graphics are contained in a graphics group. Never defined? CLicht?

        #region clefs
        /// Clefs whose ID (e.g. "trebleClef8") is recorded in the ClefMetrics.UsedClefIDs list will be written to the defs.
        clef, inputClef,    // Used, recorded. (A normal clef at the beginning of a staff)
        smallClef, inputSmallClef,  // Used, recorded. (A clef change in a voice.)
        /// The following classes are not recorded, but will be defined if they are used by the recorded clefs.
        /// These classes are used in the SVG defs element.
        clefOctaveNumber, inputClefOctaveNumber,   // A number above or below a normal clef.
        smallClefOctaveNumber, inputSmallClefOctaveNumber, // A number above or below a small clef.
        clefX, inputClefX,      // The 'x' above or below a normal clef. (Arial)
        smallClefX, inputSmallClefX, // The 'x' above or below a small clef. (Arial)
        #endregion

        #region barNumbers
        barNumber,      // recorded and used but never defined. If it exists, both barNumberNumber and barNumberFrame must be defined.
        barNumberFrame, // not recorded, but used if barNumber exists (it is part of barNumber). Must be defined if barNumber exists. 
        barNumberNumber,// recorded and used if barNumber exists. 
        #endregion barNumbers

        #region chord classes
        chord,
        inputChord,
        cautionaryChord,
        cautionaryInputChord,
        #endregion chord classes

        #region chord components
        stem, inputStem,
        // flags and inputFlags whose ID (e.g. "inputLeft3Flags") has been recorded in FlagIDs will be written to the defs.
        // Neither needs a CSS definition because they use path's default settings (unless path is re-classed for beams!).
        flag, inputFlag,
        notehead, inputNotehead, cautionaryNotehead,
        accidental, inputAccidental, cautionaryAccidental,
        ledgerlines, inputLedgerlines,   // there are no cautionaryLedgerlines
        ledgerline, inputLedgerline,    // used, but not recorded, and not defined. These classes inherit definition from the enclosing ledgerlines group (or line defined in system?)
        ornament,       // there are no input or cautionary ornaments
        lyric, inputLyric,   // there are no cautionaryLyrics
        dynamic, inputDynamic,   // there are no cautionaryDynamics
        cautionaryBracket,  // there are no inputCautionaryBrackets
        noteExtender,       // there are no inputNoteExtenders
        #endregion chord components

        #region rest
        rest, inputRest,
        #endregion rest

        #region beams
        beamBlock,      // Used, recorded, defined if used.
        inputBeamBlock, // Used, recorded, defined if used.
        beam,           // Used, not recorded. Inherits the beamBlock definition.
        inputBeam,      // Used, not recorded. Inherits the inputBeamBlock definition.
        opaqueBeam,     // Used, not recorded. Defined if beamBlock is recorded.
        inputOpaqueBeam, // Used, not recorded. Defined if inputBeamBlock is recorded.
        #endregion

        #region barlines
        barline, // always used, recorded and defined. A normal barline and endBarline component.
        endBarline, // always used and recorded. A barline + thickBarline group. Never defined.
        thickBarline, // always used and recorded. An endBarline component. Always defined.
        staffConnectors, // A group of barline (and maybe endBarline) in a system. Never defined. 
        #endregion barlines
    };

    public enum CSSLineCap { butt, round, square };
}