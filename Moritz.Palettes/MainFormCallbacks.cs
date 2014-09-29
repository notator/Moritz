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
    public delegate void SetSettingsHaveBeenSaved();
    public delegate void SetSettingsHaveNotBeenSaved();
    public delegate void SetSaveAndCreateButtons(bool enabled);
    public delegate void MainFormBringToFront();
    public delegate void SaveSettings();
    public delegate bool MainFormHasBeenSaved();
    public delegate string SettingsFolderPath();

    public class MainFormCallbacks
    {
        public SetSettingsHaveBeenSaved SetSettingsHaveBeenSaved;
        public SetSettingsHaveNotBeenSaved SetSettingsHaveNotBeenSaved;
        public SetSaveAndCreateButtons SetSaveAndCreateButtons;
        public MainFormBringToFront MainFormBringToFront;
        public SaveSettings SaveSettings;
        public MainFormHasBeenSaved MainFormHasBeenSaved;
        public SettingsFolderPath SettingsFolderPath;
    }
}
