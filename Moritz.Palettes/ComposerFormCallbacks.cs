using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Midi;
using Moritz.Symbols;
using Moritz.Spec;

namespace Moritz.Palettes
{
    public delegate void MainFormBringToFront();
    public delegate string LocalAudioFolderPath();
    public delegate bool APaletteChordFormIsOpen();
    public delegate void SetSettingsHaveChanged();

    public class ComposerFormCallbacks
    {
        public MainFormBringToFront MainFormBringToFront;
        public LocalAudioFolderPath LocalScoreAudioPath;
        public APaletteChordFormIsOpen APaletteChordFormIsOpen;
        public SetSettingsHaveChanged SetSettingsHaveChanged;
    }
}
