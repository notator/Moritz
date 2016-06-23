using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

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

            Preferences = new Preferences();

            MoritzPerformanceOptionsExtension = ".mpox";
            MoritzKrystalScoreSettingsExtension = ".mkss";
        }

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
        /// If the textBox is disabled, this function does nothing.
        /// SetDialogState sets the text box to an error state (usually by setting its background colour to pink) if:
        ///     textBox.Text is empty, or
        ///     textBox.Text contains anything other than numbers, commas and whitespace or
        ///     count != uint.MaxValue and there are not count values or the values are outside the given range.
        /// </summary>
        public static void LeaveIntRangeTextBox(TextBox textBox, bool canBeEmpty, uint count, int minVal, int maxVal,
                                                    SetDialogStateDelegate SetDialogState)
        {
            if(textBox.Enabled)
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
        }

        /// <summary>
        /// Returns null if
        ///     textBox.Text is empty, or
        ///     textBox.Text contains anything other than numbers, commas and whitespace or
        ///     count is not equal to uint.MaxValue and there are not count values or
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
                    try
                    {
                        List<int> ints = StringToIntList(textBox.Text, ',');

                        if(CheckIntList(ints, count, minVal, maxVal))
                        {
                            foreach(int i in ints)
                                strings.Add(i.ToString(M.En_USNumberFormat));
                        }
                    }
                    catch
                    {
                        // This can happen if StringToIntList(...) throws an exception
                        // -- which can happen if two numbers are separated by whitespace but no comma!)
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
        /// If the textBox is disabled, this function does nothing.
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
            if(textBox.Enabled)
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
        /// to a list of trimmed strings. A string containing a single space character will not be trimmed.
        /// </summary>
        public static List<string> StringToStringList(string s, char separator)
        {
            char[] delimiter = { separator };
            string[] strings = s.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

            List<string> returnList = new List<string>();
            foreach(string a in strings)
            {
                if(a == " ")
                {
                    returnList.Add(" ");
                }
                else
                {
                    returnList.Add(a.Trim());
                }
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

        /// <summary>
        /// This function divides total into divisor parts, returning a List of ints whose:
        ///     * Count is equal to divisor.
        ///     * sum is exactly equal to total
        ///     * members are as equal as possible. 
        /// </summary>
        public static List<int> IntDivisionSizes(int total, int divisor)
        {
            List<int> relativeSizes = new List<int>();
            for(int i = 0; i < divisor; ++i)
            {
                relativeSizes.Add(1);
            }
            return IntDivisionSizes(total, relativeSizes);

        }
        /// <summary>
        /// This function divides total into relativeSizes.Count parts, returning a List whose:
        ///     * Count is relativeSizes.Count.
        ///     * sum is exactly equal to total
        ///     * members have the relative sizes (as nearly as possible) to the values in the relativeSizes argument. 
        /// </summary>
        public static List<int> IntDivisionSizes(int total, List<int> relativeSizes)
        {
            int divisor = relativeSizes.Count;
            int sumRelative = 0;
            for(int i = 0; i < divisor; ++i)
            {
                sumRelative += relativeSizes[i];
            }
            float factor = ((float)total / (float)sumRelative);
            float fPos = 0;
            List<int> intPositions = new List<int>();
            for(int i = 0; i < divisor; ++i)
            {
                intPositions.Add((int)(Math.Floor(fPos)));
                fPos += (relativeSizes[i] * factor);
            }
            intPositions.Add((int)Math.Floor(fPos));

            List<int> intDivisionSizes = new List<int>();
            for(int i = 0; i < divisor; ++i)
            {
                int intDuration = (int)(intPositions[i + 1] - intPositions[i]);
                intDivisionSizes.Add(intDuration);
            }

            int intSum = 0;
            foreach(int i in intDivisionSizes)
                intSum += i;
            Debug.Assert(intSum <= total);
            if(intSum < total)
            {
                int lastDuration = intDivisionSizes[intDivisionSizes.Count - 1];
                lastDuration += (total - intSum);
                intDivisionSizes.RemoveAt(intDivisionSizes.Count - 1);
                intDivisionSizes.Add(lastDuration);
            }
            return intDivisionSizes;
        }

        /// <summary>
        /// Returns the value argument as a byte, coerced to the range [0..127] 
        /// </summary>
        public static byte MidiValue(int value)
        {
            int rval;

            if(value > 127)
                rval = 127;
            else if(value < 0)
                rval = 0;
            else
                rval = value;

            return (byte)rval;
        }

        public static Color TextBoxErrorColor = Color.FromArgb(255, 220, 220);
        public static Color GreenButtonColor = Color.FromArgb(215, 225, 215);
        public static Color LightGreenButtonColor = Color.FromArgb(205, 240, 205);

        public static bool HasError(List<TextBox> allTextBoxes)
        {
            bool hasError = false;
            foreach(TextBox textBox in allTextBoxes)
            {
                if(textBox.Enabled && textBox.BackColor == M.TextBoxErrorColor)
                {
                    hasError = true;
                    break;
                }
            }
            return hasError;
        }

        public static void SetTextBoxErrorColorIfNotOkay(TextBox textBox, bool okay)
        {
            if(okay)
            {
                textBox.BackColor = Color.White;
            }
            else
            {
                textBox.BackColor = M.TextBoxErrorColor;
            }
        }

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

        public enum Dynamic
        {
            none, pppp, ppp, pp, p, mp, mf, f, ff, fff, ffff
        }

        /// <summary>
        /// The key is one of the following strings: "fff", "ff", "f", "mf", "mp", "p", "pp", "ppp", "pppp".
        /// The value is used to determine Moritz' transcription of velocity -> dynamic symbol.
        /// Note that Moritz does not use M.Dynamic.ffff although it is defined in CLicht.
        /// </summary>
        public static Dictionary<M.Dynamic, byte> MaxMidiVelocity = new Dictionary<M.Dynamic, byte>()
        {
            // March 2016:  equal steps between 15 (max pppp) and 127 (max fff)
            { M.Dynamic.fff, 127},
            { M.Dynamic.ff, 113},
            { M.Dynamic.f, 99},
            { M.Dynamic.mf, 85},
            { M.Dynamic.mp, 71},
            { M.Dynamic.p, 57},
            { M.Dynamic.pp, 43},
            { M.Dynamic.ppp, 29},
            { M.Dynamic.pppp, 15}
        };

        /// <summary>
        /// The key is one of the following strings: "ffff", "fff", "ff", "f", "mf", "mp", "p", "pp", "ppp", "pppp".
        /// The value is a string containing the equivalent CLicht character.
        /// </summary>
        public static Dictionary<M.Dynamic, string> CLichtDynamicsCharacters = new Dictionary<M.Dynamic, string>()
        {
            { M.Dynamic.ffff, "Î"}, // unused by Moritz (see M.MaxMidiVelocity)
            { M.Dynamic.fff, "Ï"},
            { M.Dynamic.ff, "ƒ"},
            { M.Dynamic.f, "f"},
            { M.Dynamic.mf, "F"},
            { M.Dynamic.mp, "P"},
            { M.Dynamic.p, "p"},
            { M.Dynamic.pp, "π"},
            { M.Dynamic.ppp, "∏"},
            { M.Dynamic.pppp, "Ø"}
        };

        /// <summary>
        /// Note that Moritz does not use M.Dynamic.ffff although it is defined in CLicht.
        /// (These colours were found using a separate, self-written program.)
        /// </summary>
        public static readonly Dictionary<M.Dynamic, string> NoteheadColors = new Dictionary<M.Dynamic, string>()
        {
            { M.Dynamic.fff, "#FF0000" },
            { M.Dynamic.ff, "#D50055" },
            { M.Dynamic.f, "#B5007E" },
            { M.Dynamic.mf, "#8800B5" }, 
            { M.Dynamic.mp, "#0000CA" }, 
            { M.Dynamic.p, "#0069A8" },
            { M.Dynamic.pp, "#008474" },
            { M.Dynamic.ppp, "#009F28" },
            { M.Dynamic.pppp,"#00CA00" }
        };

        /// <summary>
        /// The returned list contains 12 velocity values in range [1..127]
        /// The returned values are in order of absolute pitch, with C natural (absolute pitch 0) at position 0,
        /// C# (absolute pitch 1) at position 1, etc.
        /// </summary>
        /// <param name="absolutePitchHierarchy">retrieved using M.GetAbsolutePitchHierarchy(...)</param>
        /// <param name="velocityFactorsIndex">index in M.VelocityFactors: in range [0..7]</param>
        public static List<int> GetVelocityPerAbsolutePitch(List<int> absolutePitchHierarchy, int velocityFactorsIndex )
        {
            List<double> velocityFactorPerPitch = GetVelocityFactors(velocityFactorsIndex);

            List<int> velocityPerAbsPitch = new List<int>();
            for(int absPitch = 0; absPitch < 12; ++absPitch)
            {
                int absPitchIndex = absolutePitchHierarchy.IndexOf(absPitch);

                int velocity = M.MidiValue((int)(127 * velocityFactorPerPitch[absPitchIndex]));
                velocity = (velocity == 0) ? 1 : velocity;

                velocityPerAbsPitch.Add(velocity);
            }
            return velocityPerAbsPitch;
        }

        /// <summary>
        /// Returns a list of pitch numbers in range [0..127] in ascending order.
        /// The first pitch in the returned list is always rootPitch.
        /// The other returned pitches are transposed to be in ascending order by adding 12 as necessary.
        /// The maximum number of pitches returned is either nPitches or the number of pitches beginning with and
        /// following the rootPitch in the absolutePitchHierarchy, whichever is smaller.
        /// The number of returned pitches can also be smaller than nPitches because pitches that would be higher
        /// than 127 are simply not added to the returned list.
        /// </summary>
        /// <param name="nPitches">In range [1..12]</param>
        /// <param name="rootPitch">In range [0..127]</param>
        /// <param name="absolutePitchHierarchy"></param>
        /// <returns></returns>
        public static List<byte> GetAscendingPitches(int nPitches, int rootPitch, List<int> absolutePitchHierarchy)
        {
            Debug.Assert(nPitches > 0 && nPitches <= 12);
            Debug.Assert(rootPitch >= 0 && rootPitch <= 127);
            Debug.Assert(absolutePitchHierarchy.Count == 12);

            List<int> pitches = new List<int>();
            pitches.Add(rootPitch);

            if(nPitches > 1)
            {
                int absRootPitch = rootPitch % 12;
                int rootIndex = absolutePitchHierarchy.IndexOf(absRootPitch);
                int maxIndex = rootIndex + nPitches;
                maxIndex = (maxIndex < absolutePitchHierarchy.Count) ? maxIndex : absolutePitchHierarchy.Count;
                for(int i = rootIndex + 1; i < maxIndex; ++i)
                {
                    int pitch = absolutePitchHierarchy[i];
                    while(pitch <= pitches[pitches.Count - 1])
                    {
                        pitch += 12;
                    }

                    if(pitch <= 127)
                    {
                        pitches.Add(pitch);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            List<byte> bytePitches = new List<byte>();
            foreach(int pitch in pitches)
            {
                bytePitches.Add((byte)pitch);
            }
            return bytePitches;
        }

        #region static AbsolutePitchHierarchies

        /// <summary>
        /// Returns a list that basically contains the sums of absoluteValue(rootPitch) + RelativePitchHierarchies[index].
        /// If a value would be greater than 11, value = value - 12, so that all values are in range [0..11].
        /// </summary>
        /// <param name="relativePitchHierarchyIndex">In range [0..21]</param>
        /// <param name="rootPitch">In range [0..127]</param>
        public static List<int> GetAbsolutePitchHierarchy(int relativePitchHierarchyIndex, int rootPitch)
        {
            if(RelativePitchHierarchies.Count != 22)
            {
                throw new ArgumentException($"{nameof(RelativePitchHierarchies)} has changed!");
            }
            if(relativePitchHierarchyIndex < 0 || relativePitchHierarchyIndex >= RelativePitchHierarchies.Count)
            {
                throw new ArgumentException($"{nameof(relativePitchHierarchyIndex)} out of range.");
            }
            if(rootPitch < 0 || rootPitch > 127)
            {
                throw new ArgumentException($"{nameof(rootPitch)} out of range.");
            }

            List<int> absolutePitchHierarchy = new List<int>(RelativePitchHierarchies[relativePitchHierarchyIndex]); // checks index
            int absRootPitch = rootPitch % 12;

            for(int i = 0; i < absolutePitchHierarchy.Count; ++i)
            {
                absolutePitchHierarchy[i] += absRootPitch;
                absolutePitchHierarchy[i] = (absolutePitchHierarchy[i] > 11) ? absolutePitchHierarchy[i] - 12: absolutePitchHierarchy[i];
            }
            return absolutePitchHierarchy;
        }
        #endregion static AbsolutePitchHierarchies
        #region static RelativePitchHierarchies
        /// <summary>
        /// Returns a clone of the private list.
        /// </summary>
        /// <param name="index">In range [0..21]</param>
        public static List<int> GetRelativePitchHierarchy(int index)
        {
            if(RelativePitchHierarchies.Count != 22)
            {
                throw new ArgumentException($"{nameof(RelativePitchHierarchies)} has changed!");
            }
            if(index < 0 || index >= RelativePitchHierarchies.Count)
            {
                throw new ArgumentException($"{nameof(index)} out of range.");
            }

            List<int> relativePitchHierarchy = new List<int>(RelativePitchHierarchies[index]);

            #region check relativePitchHierarchy
            // Throws an exception if the relativePitchHierarchy is invalid for any of the following reasons:
            // 1. the number of values in the list is not 12.
            // 2. the first value in the list is not 0.
            // 3. any value is < 0 or > 11.
            // 4. all values must be different.
            if(relativePitchHierarchy.Count != 12)
            {
                throw new ArgumentException($"All lists in the static {nameof(M.RelativePitchHierarchies)} must have 12 values.");
            }
            if(relativePitchHierarchy[0] != 0)
            {
                throw new ArgumentException($"All lists in the static {nameof(M.RelativePitchHierarchies)} must begin with the value 0.");
            }

            string errorSource = $"static {nameof(M.RelativePitchHierarchies)}[{index}].";

            for(int i = 0; i < relativePitchHierarchy.Count; ++i)
            {
                int value = relativePitchHierarchy[i];
                if(value < 0 || value > 11)
                {
                    throw new ArgumentException($"Illegal value in {errorSource}");
                }
                for(int j = i + 1; j < relativePitchHierarchy.Count; ++j)
                {
                    int value2 = relativePitchHierarchy[j];
                    if(value == value2)
                    {
                        throw new ArgumentException($"Duplicate values in {errorSource}");
                    }
                }
            }
            #endregion check relativePitchHierarchy

            return relativePitchHierarchy;
        }
        /// <summary>
        /// This series of RelativePitchHierarchies is derived from the "most consonant" hierarchy at index 0:
        ///                    0, 7, 4, 10, 2, 5, 9, 11, 1, 3, 6, 8
        /// which has been deduced from the harmonic series as follows (decimals rounded to 3 figures):
        /// 
        ///              absolute   equal              harmonic:     absolute         closest
        ///              pitch:  temperament                         harmonic    equal temperament
        ///                        factor:                           factor:       absolute pitch:
        ///                0:       1.000       |          1   ->   1/1  = 1.000  ->     0:
        ///                1:       1.059       |          3   ->   3/2  = 1.500  ->     7:
        ///                2:       1.122       |          5   ->   5/4  = 1.250  ->     4:
        ///                3:       1.189       |          7   ->   7/4  = 1.750  ->     10:
        ///                4:       1.260       |          9   ->   9/8  = 1.125  ->     2:
        ///                5:       1.335       |         11   ->  11/8  = 1.375  ->     5:
        ///                6:       1.414       |         13   ->  13/8  = 1.625  ->     9:
        ///                7:       1.498       |         15   ->  15/8  = 1.875  ->     11:
        ///                8:       1.587       |         17   ->  17/16 = 1.063  ->     1:
        ///                9:       1.682       |         19   ->  19/16 = 1.187  ->     3:
        ///                10:      1.782       |         21   ->  21/16 = 1.313  ->     
        ///                11:      1.888       |         23   ->  23/16 = 1.438  ->     6:
        ///                                     |         25   ->  25/16 = 1.563  ->     8:
        /// </summary>
        private static List<List<int>> RelativePitchHierarchies = new List<List<int>>()
        {
            new List<int>(){ 0,  7,  4, 10,  2,  5,  9, 11,  1,  3,  6,  8 }, //  0
            new List<int>(){ 0,  4,  7,  2, 10,  9,  5,  1, 11,  6,  3,  8 }, //  1
            new List<int>(){ 0,  4,  2,  7,  9, 10,  1,  5,  6, 11,  8,  3 }, //  2
            new List<int>(){ 0,  2,  4,  9,  7,  1, 10,  6,  5,  8, 11,  3 }, //  3
            new List<int>(){ 0,  2,  9,  4,  1,  7,  6, 10,  8,  5,  3, 11 }, //  4
            new List<int>(){ 0,  9,  2,  1,  4,  6,  7,  8, 10,  3,  5, 11 }, //  5
            new List<int>(){ 0,  9,  1,  2,  6,  4,  8,  7,  3, 10, 11,  5 }, //  6
            new List<int>(){ 0,  1,  9,  6,  2,  8,  4,  3,  7, 11, 10,  5 }, //  7
            new List<int>(){ 0,  1,  6,  9,  8,  2,  3,  4, 11,  7,  5, 10 }, //  8
            new List<int>(){ 0,  6,  1,  8,  9,  3,  2, 11,  4,  5,  7, 10 }, //  9
            new List<int>(){ 0,  6,  8,  1,  3,  9, 11,  2,  5,  4, 10,  7 }, // 10
            new List<int>(){ 0,  8,  6,  3,  1, 11,  9,  5,  2, 10,  4,  7 }, // 11
            new List<int>(){ 0,  8,  3,  6, 11,  1,  5,  9, 10,  2,  7,  4 }, // 12
            new List<int>(){ 0,  3,  8, 11,  6,  5,  1, 10,  9,  7,  2,  4 }, // 13
            new List<int>(){ 0,  3, 11,  8,  5,  6, 10,  1,  7,  9,  4,  2 }, // 14
            new List<int>(){ 0, 11,  3,  5,  8, 10,  6,  7,  1,  4,  9,  2 }, // 15
            new List<int>(){ 0, 11,  5,  3, 10,  8,  7,  6,  4,  1,  2,  9 }, // 16
            new List<int>(){ 0,  5, 11, 10,  3,  7,  8,  4,  6,  2,  1,  9 }, // 17
            new List<int>(){ 0,  5, 10, 11,  7,  3,  4,  8,  2,  6,  9,  1 }, // 18
            new List<int>(){ 0, 10,  5,  7, 11,  4,  3,  2,  8,  9,  6,  1 }, // 19
            new List<int>(){ 0, 10,  7,  5,  4, 11,  2,  3,  9,  8,  1,  6 }, // 20
            new List<int>(){ 0,  7, 10,  4,  5,  2, 11,  9,  3,  1,  8,  6 }, // 21 
        };
        #endregion static RelativePitchHierarchies  

        #region VelocityFactors
        /// <summary>
        /// Returns a list of 12 doubles, in decreasing order of size (A copy of the static values.).
        /// Each value is in range [1..0], and represents the relative velocity of the corresponding pitch in a
        /// relativePitchHierarchy. The first value in the returned list is always 1.
        /// If the index argument to this function is 0, the double at the end of the returned list will be very small.
        /// If the index argument to this function is 7, all the doubles will be the same size (=1).
        /// </summary>
        /// <param name="index">In range [0..7]</param>
        public static List<double> GetVelocityFactors(int index)
        {
            if(index < 0 || index >= VelocityFactors.Count)
            {
                throw new ArgumentException($"{nameof(index)} out of range.");
            }

            List<double> velocityFactors = new List<double>(VelocityFactors[index]);

            Debug.Assert(velocityFactors.Count == 12);
            Debug.Assert(velocityFactors[0] == 1.0);
            for(int i = 1; i < 12; ++i)
            {
                double factor = velocityFactors[i];
                Debug.Assert(factor <= 1.0 && factor >= 0.0);
                Debug.Assert(velocityFactors[i - 1] >= factor);
            }

            return velocityFactors;
        }
        /// <summary>
        /// The xth (indexth) list contains 12 doubles, each of which represents the
        /// relative velocity of the corresponding pitch in a relativePitchHierarchy.
        /// <para>A list of lists of double containing the following values (rounded to 5 decimal places):
        ///      y=0 | y=1     | y=2     | y=3     | y=4     | y=5     | y=6     | y=7     | y=8     | y=9     | y=10    | y=11    | 
        /// x=0: 1   | 0,92985 | 0,8597  | 0,78955 | 0,7194  | 0,64925 | 0,5791  | 0,50895 | 0,4388  | 0,36865 | 0,2985  | 0,22835 | 
        /// x=1: 1   | 0,93987 | 0,87974 | 0,81961 | 0,75948 | 0,69936 | 0,63923 | 0,5791  | 0,51897 | 0,45884 | 0,39871 | 0,33858 | 
        /// x=2: 1   | 0,94989 | 0,89979 | 0,84968 | 0,79957 | 0,74946 | 0,69936 | 0,64925 | 0,59914 | 0,54903 | 0,49893 | 0,44882 | 
        /// x=3: 1   | 0,95991 | 0,91983 | 0,87974 | 0,83966 | 0,79957 | 0,75948 | 0,7194  | 0,67931 | 0,63923 | 0,59914 | 0,55906 | 
        /// x=4: 1   | 0,96994 | 0,93987 | 0,90981 | 0,87974 | 0,84968 | 0,81961 | 0,78955 | 0,75948 | 0,72942 | 0,69936 | 0,66929 | 
        /// x=5: 1   | 0,97996 | 0,95991 | 0,93987 | 0,91983 | 0,89979 | 0,87974 | 0,8597  | 0,83966 | 0,81961 | 0,79957 | 0,77953 | 
        /// x=5: 1   | 0,98998 | 0,97996 | 0,96994 | 0,95991 | 0,94989 | 0,93987 | 0,92985 | 0,91983 | 0,90981 | 0,89979 | 0,88976 | 
        /// x=7: 1   | 1       | 1       | 1       | 1       | 1       | 1       | 1       | 1       | 1       | 1       | 1       |
        /// </para>
        /// </summary>
        private static List<List<double>> VelocityFactors = VelocityFactorsPerPitch();
        private static List<List<double>> VelocityFactorsPerPitch()
        {
            List<List<double>> rval = new List<List<double>>();
            List<byte> maximumVelocityPerDynamicSymbol = new List<byte>();
            maximumVelocityPerDynamicSymbol.Add(M.MaxMidiVelocity[M.Dynamic.ppp]);
            maximumVelocityPerDynamicSymbol.Add(M.MaxMidiVelocity[M.Dynamic.pp]);
            maximumVelocityPerDynamicSymbol.Add(M.MaxMidiVelocity[M.Dynamic.p]);
            maximumVelocityPerDynamicSymbol.Add(M.MaxMidiVelocity[M.Dynamic.mp]);
            maximumVelocityPerDynamicSymbol.Add(M.MaxMidiVelocity[M.Dynamic.mf]);
            maximumVelocityPerDynamicSymbol.Add(M.MaxMidiVelocity[M.Dynamic.f]);
            maximumVelocityPerDynamicSymbol.Add(M.MaxMidiVelocity[M.Dynamic.ff]);
            maximumVelocityPerDynamicSymbol.Add(M.MaxMidiVelocity[M.Dynamic.fff]);

            for(int x = 0; x < maximumVelocityPerDynamicSymbol.Count; ++x)
            {
                double minVelocity = maximumVelocityPerDynamicSymbol[x];
                List<double> factors = new List<double>();
                double decrement = (127 - minVelocity) / 11;
                double currentVelocity = 127;
                for(int y = 0; y < 12; ++y)
                {
                    factors.Add(currentVelocity / 127);
                    currentVelocity -= decrement;
                }
                rval.Add(factors);
            }

            //for(int x = 0; x < maximumVelocityPerDynamicSymbol.Count; ++x)
            //{
            //    Console.Write("x:" + x.ToString() + " ");
            //    for(int v = 0; v < 12; ++v)
            //    {
            //        double val = Math.Round(rval[x][v], 5);
            //        Console.Write(val.ToString() + " | ");
            //    }
            //    Console.WriteLine();
            //}

            return rval;
        }
        #endregion VelocityFactors

        public readonly static Preferences Preferences;
        public readonly static IFormatProvider En_USNumberFormat;

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
        public static readonly byte DefaultBankAndPatchIndex = 0;
        public static readonly byte DefaultVolume = 100;
        public static readonly byte DefaultExpression = 127;
        public static readonly byte DefaultPitchWheelDeviation = 2;
        public static readonly byte DefaultPitchWheel = 64;
        public static readonly byte DefaultPan = 64;
        public static readonly byte DefaultModulationWheel = 0;
        /// <summary>
        /// Constants that are set when palette fields are empty
        /// </summary>
        public static readonly bool DefaultHasChordOff = true;
        public static readonly bool DefaultChordRepeats = false;
        public static readonly int DefaultOrnamentMinimumDuration = 1;
        #endregion MIDI
    }

}
