namespace Moritz.Xml
{
    public enum CSSLineCap { butt, round, square };
    public enum CSSClass
    {
        #region text
        // Always written on every page with a CSS definition
        timeStamp,
        staffName,

        // Always written on the first page with a CSS definition
        mainTitle,
        author,

        // Written with a CSS definition if there is more than one system (TextStyle.CSSClasses contains CSSClass.barNumber).
        barNumberNumber, 

        // Written with a CSS definition if the class exists in the static TextStyle.CSSClasses list.
        notehead,
        accidental,
        rest,
        dynamic,
        lyric,
        ornament,
        // These are used in reminders at the beginnings of staves. 
        cautionaryNotehead,
        cautionaryAccidental,

        // These dont exist, because ordinary grace notes dont exist. 
        //cautionaryRest,
        //cautionaryDynamic,
        //cautionaryLyric,
        //cautionaryOrnament,
        //cautionaryStem,
        //cautionaryFlag,

        #region input classes
        // chord components
        inputNotehead,
        inputAccidental,
        inputRest,
        inputDynamic,
        inputLyric,
        // inputClefs (test changing clef on an inputVoice)
        inputClef,
        inputClefOctaveNumber,
        inputClefX,
        inputCautionaryClef,
        inputCautionaryClefOctaveNumber,
        inputCautionaryClefX,
        // input lines
        inputStafflines,
        inputLedgerlines,
        inputStem,
        inputBeamBlock,
        inputOpaqueBeam,
        inputFlag,
        #endregion

        #region clefs
        // If the static MetricsForUse.UsedIDs list contains any clef IDs (e.g. "trebleClef8"), they will be written to the defs.
        //
        // Written with a CSS definition if any normal clef exists in the static TextStyle.UsedIDs list.
        clef, // Clicht
        // Written with a CSS definition if any cautionary clef exists in the static TextStyle.UsedIDs list.
        cautionaryClef, // CLicht
        // Written with a CSS definition if any normal octaved clef exists in the static TextStyle.UsedIDs list.
        clefOctaveNumber, // CLicht
        // Written with a CSS definition if any cautionary octaved clef exists in the static TextStyle.UsedIDs list.
        cautionaryClefOctaveNumber, // CLicht
        // Written with a CSS definition if any normal multiply octaved clef exists in the static TextStyle.UsedIDs list.
        clefX, // Arial
        // Written with a CSS definition if any cautionary multiply octaved clef exists in the static TextStyle.UsedIDs list.
        cautionaryClefX, // Arial
        #endregion
        #endregion

        #region lines
        // Written with a CSS definition if LineStyle.CSSClasses contains CSSClass.staffline. This should always be the case.
        stafflines,

        // Written with a CSS definition if each exists.
        // The existence of each of the six CSS barline classes in the score can be inferred from the system structure.
        // These classes are only used in Barline.WriteSVG(...) and when writing the CSS definitions.
        barline, staffConnector,
        endBarlineLeft, endBarlineLeftConnector,
        endBarlineRight, endBarlineRightConnector,

        // Written with a CSS definition if there is more than one system (TextStyle.CSSClasses contains CSSClass.barNumber).
        barNumberFrame,

        // Written with a CSS definition if the class exists in the static LineStyle.CSSClasses list.
        ledgerlines,
        stem,
        beamBlock,
        cautionaryBracket,
        noteExtender,

        // Written with a (constant) CSS definition if the class beamBlock exists in the static LineStyle.CSSClasses list.
        opaqueBeam,

        // Written without a CSS definition if beamBlock exists. The definition is inherited from beamBlock.
        beam,

        // Written with a constant CSS definition if the frame exists
        frame,

        // Written without a CSS definition (a stafflines group always exists).
        // (Every staffline is enclosed by a stafflines group whose class has a CSS definition.)
        staffline,
        // Written without a CSS definition if a ledgerlines goup exists.
        // (Every ledgerline is enclosed by a ledgerlines group whose class has a CSS definition.)
        ledgerline,

        // Written without a CSS definition if there is more than one system.
        // The components (barNumberFrame and barNumberNumber) will have CSS defintions.
        barNumber,

        // If the static MetricsForUse.UsedIDs list contains any flag IDs, they will be written to the defs.
        // The flag CSS definition is not written because it only uses path with its default attributes.
        flag

        #endregion
    };
}