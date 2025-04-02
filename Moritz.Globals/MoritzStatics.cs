// Ignore Spelling: Schemas

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        /// Converts the value to a string, using as few decimal places as possible (maximum 4) and a '.' decimal point where necessary.
        /// Use this whenever writing an attribute to SVG.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FloatToShortString(float value)
        {
            return value.ToString("0.####", En_USNumberFormat);
        }

        #region folders

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

        public static string MoritzAppDataFolder = @"C:\Users\James\OneDrive\Moritz_2025";

        public static string MoritzPreferencesPath = MoritzAppDataFolder + @"\Preferences.mzpf";
        public static string MoritzAudioFolder = MoritzAppDataFolder + @"\audio";

        public static string MoritzKrystalsFolder = MoritzAppDataFolder + @"\krystals\krystals";
        public static string MoritzExpansionFieldsFolder = MoritzAppDataFolder + @"\krystals\expansion operators";
        public static string MoritzModulationOperatorsFolder = MoritzAppDataFolder + @"\krystals\modulation operators";
        public static string MoritzKrystalsSVGFolder = MoritzAppDataFolder + @"\krystals\svg";

        // Moritz´output scores are saved here.
        // They should be copied to the online folder
        // james-ingram-act-two\open-source\assistantPerformerTestSite\scores";
        public static string MoritzScoresFolder = MoritzAppDataFolder + @"\scores";

        public static string OnlineXMLSchemasFolder { get { return "https://james-ingram-act-two.de/open-source/XMLSchemas"; } }

        #endregion folders

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
        /// This function is a replacement for XmlDocument.GetElementById(idStr), which
        /// only works if there is a DTD for the XML file. So it won't work with inkscape's SVG.
        /// See: https://docs.microsoft.com/en-us/dotnet/api/system.xml.xmldocument.getelementbyid?view=net-6.0
        /// </summary>
        /// <param name="svgDoc"></param>
        /// <param name="idStr"></param>
        /// <returns></returns>
        public static XmlElement GetElementById(XmlDocument svgDoc, string tagName, string idStr)
        {
            var pathElems = svgDoc.GetElementsByTagName(tagName);

            XmlElement rvalElem = null;

            foreach(XmlElement pathElem in pathElems)
            {
                var id = pathElem.GetAttribute("id");

                if(!string.IsNullOrEmpty(id) && id == idStr)
                {
                    rvalElem = pathElem;
                    break;
                }
            }

            return rvalElem;
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

        public static List<uint> StringToUIntList(string s, char separator)
        {
            List<uint> rval = new List<uint>();
            char[] delimiter = { separator };
            string[] integers = s.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                foreach(string integer in integers)
                {
                    string i = integer.Trim();
                    if(!string.IsNullOrEmpty(i))
                    {
                        rval.Add(uint.Parse(i));
                    }
                }
            }
            catch
            {
                throw new ApplicationException("Error in Moritz.Globals.StringToUIntList()");
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
        /// Used to populate the Inversions lists
        /// </summary>
        private class IntervalPositionDistance
        {
            public IntervalPositionDistance(int value, int position)
            {
                Value = value;
                Position = (float)position;
            }
            public readonly int Value;
            public readonly float Position;
            public float Distance = 0;
        }

        private static int Compare(IntervalPositionDistance ipd1, IntervalPositionDistance ipd2)
        {
            int rval = 0;
            if(ipd1.Distance > ipd2.Distance)
                rval = 1;
            else if(ipd1.Distance == ipd2.Distance)
                rval = 0;
            else if(ipd1.Distance < ipd2.Distance)
                rval = -1;
            return rval;
        }

        /// <summary>
        /// Returns a list of byteLists whose first byteList is inversion0.
        /// If inversion0 is null or inversion0.Count == 0, the returned list of intlists is empty, otherwise
        /// If inversion0.Count == 1, the contained byteList is simply inversion0, otherwise
        /// The returned list of byteLists has a Count of (n-1)*2, where n is the Count of inversion0.
        /// </summary>
        /// <param name="inversion0"></param>
        /// <returns></returns>
        public static List<List<int>> GetLinearMatrix(List<int> inversion0)
        {
            List<List<int>> inversions = new List<List<int>>();
            if(inversion0 != null && inversion0.Count != 0)
            {
                if(inversion0.Count == 1)
                    inversions.Add(inversion0);
                else
                {

                    List<IntervalPositionDistance> ipdList = new List<IntervalPositionDistance>();
                    for(byte i = 0; i < inversion0.Count; i++)
                    {
                        IntervalPositionDistance ipd = new IntervalPositionDistance(inversion0[i], i);
                        ipdList.Add(ipd);
                    }
                    // ipdList is a now representaion of the field, now calculate the interval hierarchy per inversion
                    for(float pos = 0.25F; pos < (float)inversion0.Count - 1; pos += 0.5F)
                    {
                        List<IntervalPositionDistance> newIpdList = new List<IntervalPositionDistance>(ipdList);
                        foreach(IntervalPositionDistance ipd in newIpdList)
                        {
                            ipd.Distance = ipd.Position - pos;
                            ipd.Distance = ipd.Distance > 0 ? ipd.Distance : ipd.Distance * -1;
                        }
                        newIpdList.Sort(Compare);
                        List<int> intervalList = new List<int>();
                        foreach(IntervalPositionDistance ipd in newIpdList)
                            intervalList.Add(ipd.Value);
                        inversions.Add(intervalList);
                    }
                    // the intervalList for a particular inversionIndex is now inversions[inversionIndex]
                }
            }
            return inversions;
        }

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
        public static string ByteListToString(IEnumerable<byte> byteList)
        {
            return ListToString<byte>(byteList, " ");
        }
        /// <summary>
        /// Converts the argument to a string of numbers separated by the separator string
        /// </summary>
        public static string IntListToString(IEnumerable<int> intList, string separator)
        {
            return ListToString<int>(intList, separator);
        }

        private static string ListToString<T>(IEnumerable<T> listOfT, string separator)
        {
            StringBuilder sb = new StringBuilder();
            foreach(T t in listOfT)
            {
                sb.Append(separator);
                sb.Append(t.ToString());
            }
            sb.Remove(0, separator.Length);
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
            {
                //Debug.Assert(i >= 0);
                if(i < 0)
                {
                    throw new ApplicationException();
                }
                intSum += i;
            }
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
        /// Returns the value argument as an int, coerced to the range [0..127] 
        /// </summary>
        public static int MidiValue(int value)
        {
            int rval;

            if(value > 127)
            {
                rval = 127;
                Debug.WriteLine("Warning: value above limit.");
            }
            else if(value < 0)
            {
                rval = 0;
                Debug.WriteLine("Warning: value below limit.");
            }
            else
                rval = value;

            return rval;
        }
        public static int MidiValue(double value)
        {
            return MidiValue((int)value);
        }

        /// <summary>
        /// Returns the value argument as an int, coerced to the range [-64..63] 
        /// </summary>
        public static int MidiOffsetValue(int value)
        {
            int rval;

            if(value > 63)
            {
                rval = 63;
                Debug.WriteLine("Warning: value above limit.");
            }
            else if(value < -64)
            {
                rval = -64;
                Debug.WriteLine("Warning: value below limit.");
            }
            else
                rval = value;

            return rval;
        }

        public static List<byte> MidiList(List<int> values)
        {
            List<byte> rval = new List<byte>();
            foreach(int val in values)
            {
                Debug.Assert(val >= 0 && val <= 127);
                rval.Add((byte)val);
            }
            return rval;
        }

        /// <summary>
        /// Returns the argument as a byte coerced to the range 1..127.
        /// </summary>
        public static byte VelocityValue(int velocity)
        {
            velocity = (velocity >= 1) ? velocity : 1;
            velocity = (velocity <= 127) ? velocity : 127;
            return (byte)velocity;
        }

        /// <summary>
        /// A Debug.Assert that fails if the argument is outside the range 1..127.
        /// </summary>
        public static void AssertIsVelocityValue(int velocity)
        {
            Debug.Assert(velocity >= 1 && velocity <= 127);
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

        /// <summary>
        /// Note that Moritz does not use M.Dynamic.ffff even though it is defined in CLicht.
        /// </summary>
        public enum Dynamic
        {
            none, pppp, ppp, pp, p, mp, mf, f, ff, fff
        }

        /// <summary>
        /// The key is one of the following strings: "fff", "ff", "f", "mf", "mp", "p", "pp", "ppp", "pppp".
        /// The value is used to determine Moritz' transcription of velocity -> dynamic symbol.
        /// Note that Moritz does not use M.Dynamic.ffff even though it is defined in CLicht.
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
        /// The key is one of the following strings: "fff", "ff", "f", "mf", "mp", "p", "pp", "ppp", "pppp".
        /// The value is a string containing the equivalent CLicht character.
        /// Note that Moritz does not use M.Dynamic.ffff even though it is defined in CLicht.
        /// </summary>
        public static Dictionary<M.Dynamic, string> CLichtDynamicsCharacters = new Dictionary<M.Dynamic, string>()
        {
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

        public readonly static Preferences Preferences;
        public readonly static IFormatProvider En_USNumberFormat;

        public readonly static string MoritzPerformanceOptionsExtension;
        public readonly static string MoritzKrystalScoreSettingsExtension;

        #region MIDI
        /// <summary>
        /// Moritz only uses commands and controls supported by the ResidentSynth.
        /// See https://james-ingram-act-two.de/open-source/aboutResidentSynthHost.html
        /// </summary>
        public enum CMD
        {
            // AFTERTOUCH and CHANNEL_PRESSURE are not defined, because
            // they are not used in existing scores, and the ResidentSynth
            // does not support them.
            NOTE_OFF_120 = 0x80,
            NOTE_ON_144 = 0x90,
            CONTROL_CHANGE_176 = 0xB0,
            PRESET_192 = 0xC0,
            PITCH_WHEEL_224 = 0xE0
        }

        /// <summary>
        /// Standard and non-standard MIDI control numbers implemented by the ResidentSynth.
        /// Some, but not all, of the standard controls are used in legacy code.
        /// See https://james-ingram-act-two.de/open-source/aboutResidentSynthHost.html
        /// </summary>
        public enum CTL
        {
            #region standard controls
            BANK_0 = 0, 
            MOD_WHEEL_1 = 1,
            VOLUME_7 = 7,
            PAN_10 = 10,
            EXPRESSION_11 = 11,
            #endregion standard controls
            #region defined but not used in legacy code
            ALL_SOUND_OFF_120 = 120,
            ALL_CONTROLLERS_OFF_121 = 121,
            #endregion
            #region non-standard controls
            // These can only be used by the ResidentSynth, not by the Microsoft GS Wavetable Synth.
            PITCH_WHEEL_SENSITIVITY_16 = 16,
            MIXTURE_17 = 17,
            TUNING_GROUP_18,
            TUNING_19 = 19,
            ORNAMENT_GROUP_75 = 75,
            ORNAMENT_76 = 76,
            SEMITONE_OFFSET_80 = 80,
            CENT_OFFSET_81 = 81,
            VELOCITY_PITCH_SENSITIVITY_83 = 83,
            REVERBERATION_91 = 91
            #endregion
        }

        #region MIDI helper constants
        public static readonly int SELECT_PITCHBEND_RANGE_0 = 0; // deprecated (Use CTL2.PITCHWHEEL_SENSITIVITY_16 instead)
        public static readonly byte DEFAULT_PITCHWHEELDEVIATION_2 = 2;
        public static readonly int DEFAULT_NOTEOFF_VELOCITY_64 = 64;
        #endregion

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

        #endregion MIDI
    }

}