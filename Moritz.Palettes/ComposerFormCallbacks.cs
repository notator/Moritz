﻿using System.Windows.Forms;

namespace Moritz.Palettes
{
    public delegate void SetAllFormsExceptChordFormDisEnabled(bool enabled);
    public delegate void BringMainFormToFront();
    public delegate string SettingsPath();
    public delegate string LocalAudioFolderPath();
    public delegate bool APaletteChordFormIsOpen();

    public class ComposerFormCallbacks
    {
        public SetAllFormsExceptChordFormDisEnabled SetAllFormsExceptChordFormEnabledState;
        public BringMainFormToFront BringMainFormToFront;
        public SettingsPath SettingsPath;
        public LocalAudioFolderPath LocalScoreAudioPath;
        public APaletteChordFormIsOpen APaletteChordFormIsOpen;    
    }
}