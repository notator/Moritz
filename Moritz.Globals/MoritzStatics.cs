using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Moritz.Globals
{
    public delegate void SetDialogStateDelegate(TextBox textBox, bool success);
    /// <summary>
    /// The static class M contains solution-wide constants and generally useful functions.
    /// </summary>
    public static class M
    {
        static M()
        {
            //Creates and initializes the CultureInfo which uses the international sort.
            CultureInfo ci = new CultureInfo("en-US", false);
            En_USNumberFormat = ci.NumberFormat;

            SetMidiPitchDict();

            MoritzPerformanceOptionsExtension = ".mpox";
            MoritzKrystalScoreSettingsExtension = ".mkss";

            string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //MoritzFolder = myDocuments + @"Moritz";
            // MoritzFolder moved to C:, 9th September 2013
            MoritzFolder = @"C:\Moritz";

            MoritzXMLSchemasFolder = "http://james-ingram-act-two.de/open-source/XMLSchemas";
            MoritzFlashPlayerFolder = "http://james-ingram-act-two.de";

            Preferences = new Preferences(MoritzFolder);
        }

        /// <summary>
        /// These values are used to populate ComboBoxes (see NewScoreDialog).
        /// </summary>
        public static List<string> Algorithms = new List<string>()
        {
            "Study 2c",
            "Song Six",
            "Study 3 sketch"
        };

        /// <summary>
        /// These values are used to populate ComboBoxes (see AssistantComposerMainForm).
        /// The value "none" is used to indicate that a score has no folder, no graphics and no (.mkss) settings file.
        /// "none" is used by the temporary PaletteDemoScore which is created by audio demo buttons in palettes.
        /// </summary>
        public static List<string> ChordTypes = new List<string>()
        {
            "standard",
            "2b2"
        };

        /// <summary>
        /// The clefs are:
        ///     four treble clefs (0, 1, 2, 3 octaves higher),
        ///     four bass clefs (0, 1, 2, 3 octaves lower)
        ///     no clef 
        /// </summary>
        public static List<string> Clefs = new List<string>() { "t", "t1", "t2", "t3", "b", "b1", "b2", "b3", "n" };

        /// <summary>
        /// Returns the names of all scores which have a .mkss file in
        /// the folders below the baseFolderPath(without the .mkss extension).
        /// </summary>
        /// <param name="baseFolderPath"></param>
        /// <returns></returns>
        public static List<string> ScoreNames(string baseFolderPath)
        {
            List<string> scoreNames = new List<string>();
            DirectoryInfo directoryInfo = new DirectoryInfo(baseFolderPath);
            IEnumerable<FileInfo> fileInfos =
                directoryInfo.EnumerateFiles("*" + M.MoritzKrystalScoreSettingsExtension, SearchOption.AllDirectories);
            foreach(FileInfo fileInfo in fileInfos)
            {
                scoreNames.Add(Path.GetFileNameWithoutExtension(fileInfo.FullName));
            }
            return scoreNames;
        }

        public static void PopulateComboBox(ComboBox comboBox, List<string> items)
        {
            comboBox.SuspendLayout();
            comboBox.Items.Clear();
            foreach(string item in items)
            {
                comboBox.Items.Add(item);
            }
            comboBox.ResumeLayout();
            comboBox.SelectedIndex = 0;
        }

        public static void CreateDirectoryIfItDoesNotExist(string directoryPath)
        {
            if(!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                while(!Directory.Exists(directoryPath))
                    Thread.Sleep(100);
            }
        }

        public static string ScoreFolderName(string scoreName)
        {
            return scoreName + " score";
        }

        /// <summary>
        /// The AlgorithmFolder contains:
        ///     a. 'xxx score' folders containing the various scores associated with the algorithm.
        ///     b. performance options files (.mpox)
        ///     c. a 'midi' folder containing midi recordings created by the AssistantPerformer
        /// </summary>
        public static string AlgorithmFolder(string scorePathname)
        {
            string folder = Path.GetDirectoryName(scorePathname);
            folder = folder.Remove(folder.LastIndexOf(@"\"));
            return folder;
        }

        /// <summary>
        /// Adapted from CapXML Utilities.
        /// Reads to the next start or end tag having a name which is in the parameter list.
        /// When this function returns, XmlReader.Name is the name of the start or end tag found.
        /// If none of the names in the parameter list is found, an exception is thrown with a diagnostic message.
        /// </summary>
        /// <param name="r">XmlReader</param>
        /// <param name="possibleElements">Any number of strings, separated by commas</param>
        public static void ReadToXmlElementTag(XmlReader r, params string[] possibleElements)
        {
            List<string> elementNames = new List<string>(possibleElements);
            do
            {
                r.Read();
            } while(!elementNames.Contains(r.Name) && !r.EOF);
            if(r.EOF)
            {
                StringBuilder msg = new StringBuilder("Error reading Xml file:\n"
                    + "None of the following elements could be found:\n");
                foreach(string s in elementNames)
                    msg.Append(s.ToString() + "\n");

                MessageBox.Show(msg.ToString(), "Title", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                throw new ApplicationException(msg.ToString());
            }
        }
        /// <summary>
        /// If r.AttributeCount > 0, this function returns
        /// a dictionary containing all the attribute name-value pairs.
        /// Otherwise it does not move the reader, and returns null.
        /// </summary>
        /// <param name="r"></param>
        public static Dictionary<string, string> AttributesDict(XmlReader r)
        {
            Dictionary<string, string> rDict = null;
            int nAttributes = r.AttributeCount;
            if(nAttributes > 0)
            {
                rDict = new Dictionary<string, string>();
                for(int i = 0; i < nAttributes; i++)
                {
                    r.MoveToAttribute(i);
                    rDict.Add(r.Name, r.Value);
                }
            }
            return rDict;
        }
        /// <summary>
        /// The current date. (Written to XML files.)
        /// </summary>
        public static string NowString
        {
            get
            {
                CultureInfo ci = new CultureInfo("en-US", false);
                return DateTime.Today.ToString("dddd dd.MM.yyyy", ci.DateTimeFormat) + ", " + DateTime.Now.ToLongTimeString();
            }
        }
        /// <summary>
        /// The current time measured in milliseconds.
        /// </summary>
        public static int NowMilliseconds
        {
            get
            {
                return (int)DateTime.Now.Ticks / 10000;
            }
        }
        /// <summary>
        /// Returns the name of an enum field as a string, or (if the field has a Description Attribute)
        /// its Description attribute.
        /// </summary>
        /// <example>
        /// If enum Language is defined as follows:
        /// 
        /// 	public enum Language
        /// 	{
        ///										Basic,
        /// 		[Description("Kernigan")]   C,
        /// 		[Description("Stroustrup")]	CPP,
        /// 		[Description("Gosling")]	Java,
        /// 		[Description("Helzberg")]	CSharp
        /// 	}
        /// 
        ///		string languageDescription = GetEnumDescription(Language.Basic);
        /// sets languageDescription to "Basic"
        /// 
        ///		string languageDescription = GetEnumDescription(Language.CPP);
        /// sets languageDescription to "Stroustrup"
        /// </example>
        /// <param name="field">A field of any enum</param>
        /// <returns>
        /// If the enum field has no Description attribute, the field's name as a string.
        /// If the enum field has a Description attribute, the value of that attribute.
        /// </returns>
        public static string GetEnumDescription(Enum field)
        {
            FieldInfo fieldInfo = field.GetType().GetField(field.ToString());
            DescriptionAttribute[] attribs =
                (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attribs.Length == 0 ? field.ToString() : attribs[0].Description);
        }

        public static void SetToWhite(TextBox textBox)
        {
            if(textBox != null)
            {
                textBox.ForeColor = Color.Black;
                textBox.BackColor = Color.White;
            }
        }

        #region lists of lists of bytes
        /// <summary>
        /// Expects a string in which byte lists are separated by commas, and their components are separated by colons.
        /// </summary>
        /// <param name="text"></param>
        public static List<List<byte>> StringToByteLists(string text)
        {
            string[] envelopes = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<List<byte>> envelopesAsByteLists = new List<List<byte>>();
            foreach(string envelope in envelopes)
            {
                envelopesAsByteLists.Add(M.StringToByteList(envelope, ':'));
            }
            return envelopesAsByteLists;
        }
        #endregion

        #region int lists
        /// <summary>
        /// SetDialogState sets the text box to an error state (usually by setting its background colour to pink) if:
        ///     textBox.Text is empty, or
        ///     textBox.Text contains anything other than numbers, commas and whitespace or
        ///     count != uint.MaxValue and there are not count values or the values are outside the given range.
        /// </summary>
        public static void LeaveIntRangeTextBox(TextBox textBox, bool canBeEmpty, uint count, int minVal, int maxVal,
                                                    SetDialogStateDelegate SetDialogState)
        {
            if(textBox.Text.Length > 0)
            {
                List<string> checkedIntStrings = GetCheckedIntStrings(textBox, count, minVal, maxVal);
                if(checkedIntStrings != null)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach(string intString in checkedIntStrings)
                    {
                        sb.Append(",  " + intString);
                    }
                    sb.Remove(0, 3);
                    textBox.Text = sb.ToString();
                    SetDialogState(textBox, true);
                }
                else
                {
                    SetDialogState(textBox, false);
                }
            }
            else
            {
                if(canBeEmpty)
                    SetDialogState(textBox, true);
                else
                    SetDialogState(textBox, false);
            }
        }
        /// <summary>
        /// Returns null if
        ///     textBox.Text is empty, or
        ///     textBox.Text contains anything other than numbers, commas and whitespace or
        ///     count != uint.MaxValue && there are not count values or
        ///     the values are outside the given range.
        /// </summary>
        private static List<string> GetCheckedIntStrings(TextBox textBox, uint count, int minVal, int maxVal)
        {
            List<string> strings = new List<string>();
            bool okay = true;
            if(textBox.Text.Length > 0)
            {
                foreach(Char c in textBox.Text)
                {
                    if(!(Char.IsDigit(c) || c == ',' || Char.IsWhiteSpace(c)))
                        okay = false;
                }
                if(okay)
                {
                    List<int> ints = new List<int>();
                    ints = StringToIntList(textBox.Text, ',');

                    if(CheckIntList(ints, count, minVal, maxVal))
                    {
                        foreach(int i in ints)
                            strings.Add(i.ToString(M.En_USNumberFormat));
                    }
                }
            }

            if(strings.Count > 0)
                return strings;
            else return null;
        }
        /// <summary>
        /// Returne false if
        ///     intList == null
        ///     count != uint.MaxValue && intList.Count != count
        ///     or any value is less than minVal, 
        ///     or any value is greater than maxval.
        /// </summary>
        private static bool CheckIntList(List<int> intList, uint count, int minVal, int maxVal)
        {
            bool OK = true;
            if(intList == null || (count != uint.MaxValue && intList.Count != count))
                OK = false;
            else
            {
                foreach(int value in intList)
                {
                    if(value < minVal || value > maxVal)
                    {
                        OK = false;
                        break;
                    }
                }
            }
            return OK;
        }
        /// <summary>
        /// Converts a string containing integers separated by whitespace and the character in arg2
        /// to the corresponding list of integers.
        /// Throws an exception if the string contains anything other than 
        /// positive or negative integers, the separator or white space. 
        /// </summary>
        public static List<int> StringToIntList(string s, char separator)
        {
            List<int> rval = new List<int>();
            char[] delimiter = { separator };
            string[] integers = s.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                foreach(string integer in integers)
                {
                    string i = integer.Trim();
                    if(!string.IsNullOrEmpty(i))
                    {
                        rval.Add(int.Parse(i));
                    }
                }
            }
            catch
            {
                throw new ApplicationException("Error in Moritz.Globals.StringToIntList()");
            }
            return rval;
        }

        #endregion

        #region float lists
        /// <summary>
        /// SetDialogState sets the text box to an error state (usually by setting its background colour to pink) if:
        ///     textBox.Text is empty, or
        ///     textBox.Text contains anything other than numbers, commas and whitespace or
        ///     count != uint.MaxValue && there are not count values or
        ///     the values are outside the given range.
        /// Float values use the '.' character as decimal separator, and are separated by ','s.
        /// </summary>
        public static void LeaveFloatRangeTextBox(TextBox textBox, bool canBeEmpty, uint count, float minVal, float maxVal,
                                                    SetDialogStateDelegate SetDialogState)
        {
            if(textBox.Text.Length > 0)
            {
                List<string> checkedFloatStrings = GetCheckedFloatStrings(textBox, count, minVal, maxVal);
                if(checkedFloatStrings != null)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach(string floatString in checkedFloatStrings)
                    {
                        sb.Append(",  " + floatString);
                    }
                    sb.Remove(0, 3);
                    textBox.Text = sb.ToString();
                    SetDialogState(textBox, true);
                }
                else
                {
                    SetDialogState(textBox, false);
                }
            }
            else
            {
                if(canBeEmpty)
                    SetDialogState(textBox, true);
                else
                    SetDialogState(textBox, false);
            }
        }
        /// <summary>
        /// Returns null if textBox.Text is empty, or the contained values are outside the given range.
        /// </summary>
        private static List<string> GetCheckedFloatStrings(TextBox textBox, uint count, float minVal, float maxVal)
        {
            List<string> strings = new List<string>();
            bool okay = true;
            if(textBox.Text.Length > 0)
            {
                try
                {
                    List<float> floats = M.StringToFloatList(textBox.Text, ',');
                    okay = CheckFloatList(floats, (int)count, minVal, maxVal);
                    if(okay)
                    {
                        foreach(float f in floats)
                            strings.Add(f.ToString(M.En_USNumberFormat));
                    }
                }
                catch
                {
                    okay = false;
                }
            }

            if(strings.Count > 0)
                return strings;
            else
                return null;
        }
        /// <summary>
        /// Returne false if
        ///     count != float.MaxValue && floatList.Count != count
        ///     or any value is less than minVal, 
        ///     or any value is greater than maxval.
        /// </summary>
        private static bool CheckFloatList(List<float> floatList, int count, float minVal, float maxVal)
        {
            bool OK = true;
            if(floatList == null || count != float.MaxValue && floatList.Count != count)
                OK = false;
            else
            {
                foreach(float value in floatList)
                {
                    if(value < minVal || value > maxVal)
                    {
                        OK = false;
                        break;
                    }
                }
            }
            return OK;
        }

        /// <summary>
        /// Converts a string containing integers separated by whitescape and the character in arg2
        /// to the corresponding list of floats.
        /// Throws an exception if the string contains anything other than 
        /// positive or negative float values, the separator or white space.
        /// </summary>
        public static List<float> StringToFloatList(string s, char separator)
        {
            List<float> rval = new List<float>();
            char[] delimiter = { separator };
            string[] floats = s.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                foreach(string fl in floats)
                {
                    string f = fl.Trim();
                    if(!string.IsNullOrEmpty(f))
                    {
                        rval.Add(float.Parse(f, M.En_USNumberFormat));
                    }
                }
            }
            catch
            {
                throw new ApplicationException("Error in Moritz.Globals.StringToFloatList()");
            }
            return rval;
        }

        #endregion float lists

        /// <summary>
        /// Converts a string containing byte values separated by whitescape and the character in arg2
        /// to the corresponding list of bytes.
        /// Calls StringToIntList() with the same parameters before converting the integers to bytes.
        /// Throws an exception if the integer values are outside the range 0..127.
        /// </summary>
        public static List<byte> StringToByteList(string s, char separator)
        {
            List<int> rIntVal = StringToIntList(s, separator); // can throw an exception
            List<byte> rval = new List<byte>();
            foreach(int val in rIntVal)
            {
                if(val < 0 || val > 127)
                    throw (new ApplicationException("Value out of range in Moritz.Globals.StringToByteList()"));
                rval.Add((byte)val);
            }
            return (rval);
        }
        /// <summary>
        /// Converts a string containing 0s and 1s separated by the character in arg2
        /// to the corresponding list of bools.
        /// Calls StringToIntList() with the same parameters before converting the integers to bool.
        /// Throws an exception if the integer values are outside the range 0..1.
        /// </summary>
        public static List<bool> StringToBoolList(string s, char separator)
        {
            List<int> rIntVal = StringToIntList(s, separator); // can throw an exception
            List<bool> rval = new List<bool>();
            foreach(int val in rIntVal)
            {
                if(val < 0 || val > 1)
                    throw (new ApplicationException("Value out of range in Moritz.Globals.StringToBoolList()"));
                if(val == 1)
                    rval.Add(true);
                else
                    rval.Add(false);
            }
            return (rval);
        }
        /// <summary>
        /// Converts a string containing strings separated by whitespace and the character in arg2
        /// to a list of strings.
        /// </summary>
        public static List<string> StringToStringList(string s, char separator)
        {
            char[] delimiter = { separator };
            string[] strings = s.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

            List<string> returnList = new List<string>();
            foreach(string a in strings)
            {
                returnList.Add(a.Trim());
            }
            return returnList;
        }
        /// <summary>
        /// Converts the argument to a string of numbers separated by spaces
        /// </summary>
        /// <param name="byteList"></param>
        /// <returns></returns>
        public static string ByteListToString(IEnumerable<byte> byteList)
        {
            StringBuilder sb = new StringBuilder();
            foreach(byte i in byteList)
            {
                sb.Append(" ");
                sb.Append(i.ToString());
            }
            sb.Remove(0, 1);
            return sb.ToString();
        }

        public static Color TextBoxErrorColor = Color.FromArgb(255, 220, 220);

        public struct PaperSize
        {
            public PaperSize(float shortDimension, float longDimension)
            {
                ShortDimension_MM = shortDimension;
                LongDimension_MM = longDimension;
            }
            public float ShortDimension_MM;
            public float LongDimension_MM;
        }
        public static Dictionary<string, PaperSize> PaperSizes = new Dictionary<string, PaperSize>()
        {
            { "A4", new PaperSize(210F, 297F)},
            { "A5", new PaperSize(148F, 210F)},
            { "A3", new PaperSize(297F, 420F)},
            { "B4", new PaperSize(250F, 354F)},
            { "B5", new PaperSize(182F, 257F)},
            { "Letter", new PaperSize(216F, 279F)},
            { "Legal", new PaperSize(216F, 356F)},
            { "Tabloid", new PaperSize(279F, 432F)},
        };

        public readonly static Preferences Preferences;
        public readonly static IFormatProvider En_USNumberFormat;
        /// <summary>
        /// If the preferences file cannot be found, a new one is created.
        /// The default user folder in the new preferences file is set to the following path.
        /// This path, but not the location of the preferences, can be changed in the preferences dialog.
        /// </summary>
        public readonly static string MoritzFolder;
        public readonly static string MoritzXMLSchemasFolder;
        public readonly static string MoritzFlashPlayerFolder;

        public readonly static string MoritzPerformanceOptionsExtension;
        public readonly static string MoritzKrystalScoreSettingsExtension;

        public static readonly int DefaultMinimumBasicMidiChordMsDuration = 1;

        #region MIDI
        /// <summary>
        /// Called in the above constructor
        /// </summary>
        private static void SetMidiPitchDict()
        {
            string[] alphabet = { "C", "D", "E", "F", "G", "A", "B" };
            int midiPitch = 0;
            for(int octave = 0; octave < 11; octave++)
            {
                foreach(string letter in alphabet)
                {
                    switch(letter)
                    {
                        case "A":
                            midiPitch = 9;
                            break;
                        case "B":
                            midiPitch = 11;
                            break;
                        case "C":
                            midiPitch = 0;
                            break;
                        case "D":
                            midiPitch = 2;
                            break;
                        case "E":
                            midiPitch = 4;
                            break;
                        case "F":
                            midiPitch = 5;
                            break;
                        case "G":
                            midiPitch = 7;
                            break;
                    }

                    midiPitch += (octave * 12); // C5 (middle letter) is 60
                    if(midiPitch >= 0 && midiPitch <= 119)
                    {
                        string pitchString = letter + octave.ToString();
                        MidiPitchDict.Add(pitchString, midiPitch);
                    }
                    else if(midiPitch > 119 && midiPitch < 128)
                    {
                        string pitchString = letter + ":";
                        MidiPitchDict.Add(pitchString, midiPitch);
                    }
                }
            }
        }
        /// <summary>
        /// capella pitch strings and their equivalent midi pitch numbers.
        /// the strings are absolute diatonic pitch. middle C (c'): "C5"
        /// Chromatic pitches are found using the head.alteration field (-2..+2)
        /// This dictionary is
        /// </summary>
        static public Dictionary<string, int> MidiPitchDict = new Dictionary<string, int>();

        /// <summary>
        /// The following values are (supposed to be) set by AllControllersOff.
        /// These values are omitted from SVG-MIDI files since they are set by default when reading the files.
        /// </summary>
        public static readonly byte DefaultBankIndex = 0;
        public static readonly byte DefaultVolume = 100;
        public static readonly byte DefaultExpression = 127;
        public static readonly byte DefaultPitchWheelDeviation = 2;
        public static readonly byte DefaultPitchWheel = 64;
        public static readonly byte DefaultPan = 64;
        public static readonly byte DefaultModulationWheel = 0;
        #endregion MIDI
    }

}
