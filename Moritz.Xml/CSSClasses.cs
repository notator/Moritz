namespace Moritz.Xml
{
    /// <summary>
    /// This enum contains _all_ the element classes that are written to SVG scores.
    /// Not all these classes will have CSS style definitions.
    /// The style definitions are minimized, taking into account the way the elements nest.
    /// </summary>
    public enum CSSClass
    {
        /// This class must exist, but it is never used as an element class and never given a CSS definition!
        none,

        frame,       // Used and defined on non-scroll pages. Not recorded.

        timeStamp,  // Used and defined on every page. Not recorded.        

        mainTitle,  // Used and defined on every page. Not recorded. 
        author,     // Used and defined on every page. Not recorded. 

        staffName,      // Used, defined if recorded.
        inputStaffName, // Used, defined if recorded.

        barNumber,      // Used and recorded. Never defined. If it exists, barNumberNumber and barNumberFrame are defined.
        barNumberFrame, // Used, not recorded. Defined if barNumber is recorded.
        barNumberNumber,// Used, not recorded, Defined if barNumber is recorded.

        system,
        staff, inputStaff,
        voice, inputVoice, // no Metrics

        stafflines, inputStafflines,
        staffline, inputStaffline,

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

        chord, inputChord, cautionaryChord, cautionaryInputChord,
        #region chord components
        notehead,       // Used, recorded.
        inputNotehead,  // written, recorded, not yet defined
        accidental,     // Used, recorded.
        inputAccidental,//  written, recorded, not yet defined
        stem,           // Used, recorded, defined.
        inputStem,      // written, not recorded, not defined
        rest,           // Used, recorded.
        inputRest,      //  written, recorded, not yet defined
        dynamic,        // Used, recorded.
        inputDynamic,   //  written, recorded, not yet defined
        lyric,          // Used, recorded.
        inputLyric,     //  written, recorded, not yet defined
        ledgerlines,        // Used, not recorded. Exists (and is given a definition) if ledgerline has been recorded.
        inputLedgerlines,   // written, not recorded, not defined
        ledgerline, // Used, recorded. If it exists, ledgerlines is defined, and ledgerline inherits from that.
        inputLedgerline,    // written, not recorded, not defined
        // Flags whose ID (e.g. "left3Flags") appears in the MetricsForUse.UsedIDs list will be written to the defs.
        // Flag does not need a CSS definition because it uses path's default settings.
        flag, // written and recorded
        // inputFlags whose ID (e.g. "inputLeft3Flags") appears in the MetricsForUse.UsedIDs list will be written to the defs.
        // inputFlags does not need a CSS definition because it uses path's default settings.
        inputFlag,          // written and recorded.
        ornament,       // Used, recorded.
        noteExtender,       // Used, recorded, defined.
        cautionaryBracket,  // Used, recorded, defined.
        cautionaryNotehead,     // Used, recorded. (A reminder at the beginning of a staff.)
        cautionaryAccidental,   // Used, recorded. (A reminder at the beginning of a staff.)
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