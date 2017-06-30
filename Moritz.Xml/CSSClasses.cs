namespace Moritz.Xml
{
    public enum CSSLineCap { butt, round, square };
    public enum CSSClass
    {
        /// This class exists for technical reasons, but it is never used or added to a
        /// used class list, so is never given a CSS definition!
        none,

        #region text
        // (Recordings are in the TextStyle.CSSClasses list.)

        timeStamp,  // Written, not recorded but appears on every page.        
       
        mainTitle,  // Written, not recorded but always appears on first page
        author,     // Written, not recorded but always appears on first page
     
        barNumberNumber,    // Written, not recorded, exists if barNumber is recorded.

        staffName,  // Written, recorded.
        notehead,   // Written, recorded.
        accidental, // Written, recorded.
        rest,       // Written, recorded.
        dynamic,    // Written, recorded.
        lyric,      // Written, recorded.
        ornament,   // Written, recorded.
 
        cautionaryNotehead,     // Written, recorded. (A reminder at the beginning of a staff.)
        cautionaryAccidental,   // Written, recorded. (A reminder at the beginning of a staff.)

        #region input classes
        /// chord components
        inputNotehead, // written, recorded, not yet defined
        inputAccidental, //  written, recorded, not yet defined
        inputRest, //  written, recorded, not yet defined
        inputDynamic,  //  written, recorded, not yet defined
        inputLyric,  //  written, recorded, not yet defined
        #region TODO
        /// inputClefs (these are currently written as normal clefs.)
        inputClef,
        inputClefOctaveNumber,
        inputClefX,
        inputSmallClef,
        inputSmallClefOctaveNumber,
        inputSmallClefX,
        #endregion
        /// input texts
        inputStaffName,     // written, recorded, not yet defined
        /// input lines
        inputStafflines,    // TODO (currently written as normal stafflines.)
        inputStaffline,     // TODO (currently written as normal stafflines.)
        inputLedgerlines,   // written, not recorded, not defined
        inputLedgerline,    // written, not recorded, not defined
        inputStem,          // written, not recorded, not defined
        inputBeamBlock,     // TODO
        inputBeam,          // TODO
        inputOpaqueBeam,    // TODO
        // inputFlags whose ID (e.g. "inputLeft3Flags") appears in the MetricsForUse.UsedIDs list will be written to the defs.
        // inputFlags does not need a CSS definition because it uses path's default settings.
        inputFlag,          // written and recorded.
        #endregion

        #region clefs
        /// Clefs whose ID (e.g. "trebleClef8") appears in the MetricsForUse.UsedClefIDs list will be written to the defs.
        /// The following classes are given CSS definitions if they exist in the defs clefs.
        clef,   // Written, recorded. (A normal Clef)
        smallClef,   // Written, recorded. (A clef change in a voice.)
        clefOctaveNumber,    // Written, recorded. (the number(s) above or below a normal clef.)
        smallClefOctaveNumber, // Written, recorded. (the number(s) above or below a small clef.)
        clefX, // Written, recorded. (the 'x' above or below a normal clef.) (Arial)
        smallClefX,  // Written, recorded. (the 'x' above or below a small clef.) (Arial)
        #endregion
        #endregion

        #region lines
        // (Recordings are in the LineStyle.CSSClasses list.)
        stafflines,      // written, recorded, defined

        // These classes are all written in the score, but never recorded.
        // Their existence can be inferred from the system structure. 
        barline, staffConnector,
        endBarlineLeft, endBarlineLeftConnector,
        endBarlineRight, endBarlineRightConnector,

        barNumberFrame,    // Written, not recorded. Exists if barNumber is recorded (in Text recording).

        ledgerlines,        // Written, not recorded. Exists (and is given a definition) if ledgerline has been recorded.
        stem,               // Written, recorded, defined.
        beamBlock,          // Written, recorded, defined.
        cautionaryBracket,  // Written, recorded, defined.
        noteExtender,       // Written, recorded, defined.

        /// Written with a (constant) CSS definition if the class beamBlock exists in the static LineStyle.CSSClasses list.
        opaqueBeam,  // Written, not recorded. Exists if beamBlock is recorded (in Text recording).
        beam,        // Written, not recorded, never defined. Exists if beamBlock is defined. Inherits the beamBlock definition.
        frame,       // Written on non-scroll pages. Not recorded. Defined if it exists.

        /// Written without a CSS definition (a stafflines group always exists).
        /// (Every staffline is enclosed by a stafflines group whose class has a CSS definition.)
        staffline,  // Written, recorded. If it exists, stafflines is defined, and staffline inherits from that.
        ledgerline, // Written, recorded. If it exists, ledgerlines is defined, and ledgerline inherits from that.

        barNumber, // Written, recorded. Never defined. If it exists, barNumberNumber and barNumberFrame are defined.

        // Flags whose ID (e.g. "left3Flags") appears in the MetricsForUse.UsedIDs list will be written to the defs.
        // Flag does not need a CSS definition because it uses path's default settings.
        flag // written and recorded

        #endregion
    };
}