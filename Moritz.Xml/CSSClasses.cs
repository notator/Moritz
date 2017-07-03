namespace Moritz.Xml
{
    /// <summary>
    /// This enum contains _all_ the element classes that are written to SVG scores.
    /// Not all these classes will have CSS style definitions.
    /// The style definitions are minimized, taking into account the way the elements nest.
    /// </summary>
    public enum CSSClass
    {
        #region complete (recorded and used 03.07.2017)
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
        voice, inputVoice, // used, but has no Metrics, so is never defined

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
        // Neither needs a CSS definition because they use path's default settings.
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
        #endregion complete (recorded and used 03.07.2017)

        //===========================================

        #region clefs
        /// Clefs whose ID (e.g. "trebleClef8") appears in the ClefMetrics.UsedClefIDs list will be written to the defs.
        /// The following classes are given CSS definitions if they exist in the UsedClefIDs.
        clef,   // Used, recorded. (A normal Clef)
        smallClef,   // Used, recorded. (A clef change in a voice.)
        clefOctaveNumber,    // Used, recorded. (the number(s) above or below a normal clef.)
        smallClefOctaveNumber, // Used, recorded. (the number(s) above or below a small clef.)
        clefX, // Used, recorded. (the 'x' above or below a normal clef.) (Arial)
        smallClefX,  // Used, recorded. (the 'x' above or below a small clef.) (Arial)
        #region TODO
        /// inputClefs (these are currently written as normal clefs.)
        inputClef,
        inputClefOctaveNumber,
        inputClefX,
        inputSmallClef,
        inputSmallClefOctaveNumber,
        inputSmallClefX,
        #endregion
        #endregion
        #region beams
        beamBlock,      // Used, recorded, defined.
        inputBeamBlock, // TODO
        beam,           // Used, not recorded, never defined. Exists if beamBlock is defined. Inherits the beamBlock definition.
        inputBeam,      // TODO
        /// Used with a (constant) CSS definition if the class beamBlock exists in the static LineStyle.CSSClasses list.
        opaqueBeam,  // Used, not recorded. Exists if beamBlock is recorded (in Text recording).
        inputOpaqueBeam,    // TODO
        #endregion
        #region barlines
        // These classes are all written in the score, but never recorded.
        // Their existence can be inferred from the system structure. 
        barline, staffConnector,
        endBarlineLeft, endBarlineLeftConnector,
        endBarlineRight, endBarlineRightConnector,
        #endregion
    };

    public enum CSSLineCap { butt, round, square };
}