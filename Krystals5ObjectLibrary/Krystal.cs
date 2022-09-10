
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Krystals5ObjectLibrary
{
    #region helper classes
    /// <summary>
    /// The struct returned by the Enumerable property Krystal.LeveledValues.
    /// Each value in the krystal is returned with its level.
    /// The first value in each strand has the strand's level.
    /// All other values have a level equal to the krystal's level + 1.
    /// </summary>
    public struct LeveledValue
    {
        public int level;
        public int value;
    }
    /// <summary>
    /// this class contains strand parameters, and is used to build the _strandNodeList for an expansion.
    /// The _strandNodeList is used as a parameter in the following constructors:
    ///     FieldEditor.Painter
    ///     FieldEditor.ExpansionTreeView
    ///     FieldEditor.Expansion
    /// </summary>
    public class StrandNode : TreeNode
    {
        public StrandNode(int moment, int level, int density, int point)
        {
            StrandMoment = moment;
            StrandLevel = level;
            StrandDensity = density;
            StrandPoint = point;
        }
        public int StrandMoment;
        public int StrandLevel;
        public int StrandDensity;
        public int StrandPoint;
    }
    public class ContouredStrandNode : StrandNode
    {
        public ContouredStrandNode(int moment, int level, int density, int point, int axis, int contour)
            : base(moment, level, density, point)
        {
            strandAxis = axis;
            strandContour = contour;
        }
        public int strandAxis;
        public int strandContour;
    }
    #endregion helper classes

    public interface INamedComparable : IComparable
    {
        string Name { get; }
    }

    public abstract class Krystal : INamedComparable
    {
        public Krystal() { }

        /// <summary>
        /// Checks that the argument file is a krystal definition, and loads the strands.
        /// Does not load heredity info.
        /// </summary>
        /// <param name="filepath"></param>
        public Krystal(string filepath)
        {
            string filename = Path.GetFileName(filepath);
            Strands = new List<Strand>();
            try
            {
                using(XmlReader r = XmlReader.Create(filepath))
                {
                    Name = Path.GetFileName(filepath);
                    MinValue = uint.MaxValue;
                    MaxValue = uint.MinValue;
                    K.ReadToXmlElementTag(r, "krystal"); // check that this is a krystal
                    // ignore the heredity info in the next element (<expansion> etc.)
                    K.ReadToXmlElementTag(r, "strands"); // check that there is a "strands" element
                    // get the strands and their related variables
                    K.ReadToXmlElementTag(r, "s");
                    while(r.Name == "s")
                    {
                        Strand strand = new Strand(r);
                        Level = (Level < strand.Level) ? strand.Level : Level;
                        foreach(uint value in strand.Values)
                        {
                            MinValue = (MinValue < value) ? MinValue : value;
                            MaxValue = (MaxValue < value) ? value : MaxValue;
                        }
                        NumValues += (uint)strand.Values.Count;
                        Strands.Add(strand);
                    }
                    // r.Name is the end tag "strands" here
                }
            }
            catch(Exception ex)
            {
                throw (ex);
            }
        }
        #region public functions
        /// <summary>
        /// Updates the strand-related Properties Strands, Level, MinValue, MaxValue and NumValues
        /// using the given list of strands.
        /// </summary>
        /// <param name="strands"></param>
        public void Update(List<Strand> strands)
        {
            Strands = strands;
            Level = 0;
            MinValue = uint.MaxValue;
            MaxValue = uint.MinValue;
            NumValues = 0;
            foreach(Strand strand in strands)
            {
                NumValues += (uint)strand.Values.Count;
                Level = (Level < strand.Level) ? strand.Level : Level;
                foreach(uint value in strand.Values)
                {
                    MinValue = (MinValue < value) ? MinValue : value;
                    MaxValue = (MaxValue < value) ? value : MaxValue;
                }
            }
        }

        /// <summary>
        /// Uses the strand related properties and the names of krystals already 
        /// in the krystals folder to create a new unique name.
        /// ExpansionKrystal overrides this function to include the ExpanderID.
        /// </summary>
        protected virtual string GetUniqueName(K.KrystalType type)
        {
            string root = GetNameRoot(); // domain.shape.
            string suffix = string.Format($".{type}{K.KrystalFilenameSuffix}");
            string uniqueName = GetUniqueName(root, suffix);

            return uniqueName;
        }

        /// <summary>
        /// Returns a krystal name beginning with prefix, and ending with suffix.
        /// If there is such a krystal in the krystals folder containing identical strands,
        /// then that krystal's name is returned. Otherwise a new name (with an unused index) is returned.
        /// ExpansionKrystal overrides this function, including  to include the ExpanderID.
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        protected string GetUniqueName(string prefix, string suffix)
        {
            int GetIndex(string name)
            {
                char[] dot = new char[] { '.' };
                var components = name.Split(dot);
                string iString = components[components.Length - 3];
                int.TryParse(iString, out int iVal);
                return iVal;
            }

            string searchString = String.Format($"{prefix}*{suffix}");
            string[] similarPathnames = Directory.GetFiles(K.KrystalsFolder, searchString);
            string rval = String.Format($"{prefix}{similarPathnames.Length + 1}{suffix}"); // default value

            var pathList = similarPathnames.ToList();
            pathList.Sort();
            int runningIndex = 1;
            foreach(var path in pathList)
            {
                var name = Path.GetFileName(path);
                int index = GetIndex(name);
                if(index > runningIndex)
                {
                    // The krystal whose name has runningIndex has been deleted
                    // from the folder, so return the name having runningIndex.
                    rval = String.Format($"{prefix}{runningIndex}{suffix}");
                    break;
                }

                var existingKrystal = new DensityInputKrystal(path);
                if(this.Equals(existingKrystal))
                {
                    rval = existingKrystal.Name;
                    break;
                }
                runningIndex++;
            }

            return rval;
        }

        /// <summary>
        /// A Krystal's name root consists of its domain (=MaxValue) followed by a '.' character,
        ///   followed by a shapeNameString followed by a '.' character.
        /// The shapeNameString contains one or more integers separated by '_' characters.
        /// The first int in the shapeNameString is the number of level 1 and level 2 strands, so:
        ///  "0" is a constant krystal -- containing one strand having level 0 and one value (=domain) (no level 1 or level 2 strands).
        ///  "1_[nValues]" is a line krystal -- containing one strand having level 1 and [nValues] values (no level 2 strands).
        ///  "7_[nValues)" is a level 2 krystal -- containing 7 strands (1 level 1 and 6 level 2 strands) and [nValues] values.
        ///  "7_28_[nValues]" is a level 3 krystal - containing 28 strands having level 1, 2, or 3, and [nValues] values.
        ///  "7_28_206_[nValues]" is a level 4 krystal - containing 206 strands having level 1, 2, 3 or 4, and [nValues] values. 
        /// </summary>
        /// <returns></returns>
        protected string GetNameRoot()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(MaxValue.ToString() + ".");

            if(Level == 0)
            {
                sb.Append("0");
            }
            else if(Level == 1)
            {
                sb.Append("1");
                sb.Append("_");
                sb.Append(Strands[0].Values.Count().ToString());
            }
            else
            {
                for(int i = 1; i < ShapeArray.Length; i++)
                {
                    sb.Append(ShapeArray[i].ToString() + '_');
                }
                sb.Remove(sb.Length - 1, 1);
            }

            sb.Append('.');

            return sb.ToString();
        }

        /// <summary>
        /// Sets the krystal's Name, and saves it (but not any of its ancestor files).
        /// If a krystal having identical content exists in the krystals directory,
        /// the user is given the option to
        ///    either overwrite the existing krystal (using that krystal's index),
        ///    or abort the save.
        /// This means that a given set of ancestors should always have the same index.
        /// Returns true if the file has been saved, false if the save has been aborted.
        /// </summary>
        public abstract bool Save();

        /// <summary>
        /// Returns true if the krystals contain identical strands, otherwise false.
        /// </summary>
        /// <param name="obj"></param>
        public new bool Equals(Object obj)
        {
            var otherK = obj as Krystal;
            if(otherK == null || this.Strands.Count != otherK.Strands.Count)
            {
                return false;
            }

            for(int i = 0; i < Strands.Count; i++)
            {
                var strand = Strands[i];
                var otherStrand = otherK.Strands[i];
                if(strand.Level != otherStrand.Level || strand.Values.SequenceEqual(otherStrand.Values) == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns 0 if the content of the two krystals is identical,
        /// otherwise it returns the result of comparing the two names.
        /// </summary>
        /// <param name="otherKrystal"></param>
        /// <returns></returns>
        public int CompareTo(object other)
        {
			if(!(other is Krystal otherKrystal))
				throw new ArgumentException();

			bool contentIsIdentical = false;
            if(this.Shape == otherKrystal.Shape
            && this.Strands.Count == otherKrystal.Strands.Count
            && this.Level == otherKrystal.Level
            && this.MaxValue == otherKrystal.MaxValue
            && this.MinValue == otherKrystal.MinValue
            && this.NumValues == otherKrystal.NumValues
            && this.MissingValues == otherKrystal.MissingValues
            && this.Equals(otherKrystal))
            {
                contentIsIdentical = true;
            }

            if(contentIsIdentical == true)
                return 0;
            else
                return this.Name.CompareTo(otherKrystal.Name);
        }

        /// <summary>
        /// Recreates this krystal using its existing inputs, without opening an editor.
        /// Used to preserve the integrity of a group of krystals, when one of them has changed.
        /// </summary>
        public abstract void Rebuild();
        public override string ToString()
        {
            return this.Name;
        }
        /// <summary>
        /// This krystal can be permuted at the given level if it has less then 7 elements at that level.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool IsPermutableAtLevel(uint level)
        {
            bool isPermutableAtLevel = true;
            int nElements = 0;
            if(level > this.Level || level < 1)
            {
                isPermutableAtLevel = false;
            }
            else if(level < this.Level)
            {
                foreach(Strand strand in this.Strands)
                {
                    if(strand.Level <= level)
                    {
                        nElements = 1;
                    }
                    else if(strand.Level == level + 1)
                    {
                        nElements++;
                        if(nElements > 7)
                        {
                            isPermutableAtLevel = false;
                            break;
                        }
                    }
                }
            }
            else // level == this.Level
            {
                foreach(Strand strand in this.Strands)
                {
                    if(strand.Values.Count > 7)
                    {
                        isPermutableAtLevel = false;
                        break;
                    }
                }
            }
            return isPermutableAtLevel;
        }

		/// <summary>
		/// Returns the flat krystal values, spread over the range [minEnvValue..maxEnvValue],
		/// as an Envelope.
		/// </summary>
		/// <param name="minEnvValue"></param>
		/// <param name="maxEnvValue"></param>
		/// <returns></returns>
		public Envelope ToEnvelope(int minEnvValue, int maxEnvValue)
		{
			List<int> substituteValues = new List<int>();
			double incr = ((double)(maxEnvValue - minEnvValue)) / (MaxValue - MinValue);
			for(int i = (int)MinValue; i < (MaxValue + 1); i++)
			{
				substituteValues.Add((int) Math.Round(minEnvValue + (incr * (i - 1))));
			}

			List<int> kValues = GetValues(1)[0];

			for(int i = 0; i < kValues.Count; i++)
			{
				kValues[i] = substituteValues[kValues[i] - 1];
			}

			Envelope envelope = new Envelope(kValues, maxEnvValue, maxEnvValue, kValues.Count);

			return envelope;
		}
		#endregion public functions
		#region protected functions
        protected XmlWriter BeginSaveKrystal()
        {
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				IndentChars = ("\t"),
				CloseOutput = true
			};
			string namePath = K.KrystalsFolder + @"\" + Name;
            XmlWriter w = XmlWriter.Create(namePath, settings);
            w.WriteStartDocument();
            w.WriteComment("created: " + K.Now);

            w.WriteStartElement("krystal"); // ended in EndSaveKrystal()
            w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, K.MoritzXmlSchemasFolder + @"/krystals.xsd");

            return w;
        }
        protected void EndSaveKrystal(XmlWriter w)
        {
            w.WriteStartElement("strands");
            foreach(Strand strand in Strands)
            {
                w.WriteStartElement("s");
                w.WriteAttributeString("l", strand.Level.ToString());
                w.WriteString(K.GetStringOfUnsignedInts(strand.Values));
                w.WriteEndElement();
            }
            w.WriteEndElement(); // ends the strands
            w.WriteEndElement(); // ends the krystal element
            w.Close(); // closes the stream and (I think) disposes of w            
        }
        #endregion protected functions
        #region public properties
        public string Name { get; set; }
        public uint Level { get; set; }
        public uint MinValue { get; set; }
        public uint MaxValue { get; set; }
        public uint NumValues { get; set; }
        public List<Strand> Strands { get; set; }
        /// <summary>
        /// A string of comma-separated values not contained in the krystal
        /// </summary>
        public string MissingValues
        {
            get
            {
                int[] nValues = new int[this.MaxValue + 1];
                foreach(Strand strand in this.Strands)
                    foreach(uint value in strand.Values)
                        nValues[value]++;

                StringBuilder sb = new StringBuilder();
                for(uint i = MinValue; i <= this.MaxValue; i++)
                    if(nValues[i] == 0)
                    {
                        sb.Append(", ");
                        sb.Append(i.ToString());
                    }
                if(sb.Length > 0)
                    sb.Remove(0, 2);
                else
                    sb.Append("(none)");
                return sb.ToString();
            }
        }

        /// <summary>
        /// At level 1, the single int list contains all the strand values as a single list.
        /// At level krystal.Level, each internal int list contains the values from a single strand.
        /// At level krystal.Level + 1, each internal int list contains a single strand value.
        /// An ArgumentOutOfRangeException is thrown if the argument is outside the range 1..krystal.Level + 1.
        /// </summary>
        /// <param name="level">Must be in range 1..krystal.Level+1</param>
        /// <returns>The values in the krystal as a list of int lists.</returns>
        public List<List<int>> GetValues(uint level)
        {
            if(level < 1 || level > Level + 1)
                throw new ArgumentOutOfRangeException("Error in Krystal.GetValues().");

            List<List<int>> returnList = new List<List<int>>();
            int[] shapeArray = ShapeArray;
            int nInternalLists = shapeArray[level-1];
            for(int i = 0; i < nInternalLists; i++)
            {
                List<int> internalList = new List<int>();
                returnList.Add(internalList);
            }

            int returnListIndex = -1;
            foreach(Strand strand in this.Strands)
            {
                if(level == Level + 1)
                {
                    foreach(uint value in strand.Values)
                    {
                        returnListIndex++;
                        returnList[returnListIndex].Add((int)value);
                    }
                }
                else
                {
                    if(strand.Level <= level)
                        returnListIndex++;

                    foreach(uint value in strand.Values)
                        returnList[returnListIndex].Add((int)value);
                }
            }

            return returnList;
        }

        public int[] ShapeArray
        {
            get
            {
                int[] shapeArray = new int[this.Level + 1];
                foreach(Strand strand in this.Strands)
                {
                    for(uint i = strand.Level; i <= this.Level; i++)
                        shapeArray[i - 1] += 1;
                    shapeArray[this.Level] += strand.Values.Count;
                }
                return shapeArray;
            }
        }
        /// <summary>
        /// A string of " : " separated values showing the final clock sizes
        /// </summary>
        public string Shape
        {
            get
            {
                string shapeString = "";
                if(this.Level > 0)
                {
                    int[] shapeArray = ShapeArray;
                    StringBuilder shapeStrB = new StringBuilder();
                    for(int i = 0; i <= this.Level; i++)
                    {
                        shapeStrB.Append(" : ");
                        shapeStrB.Append(shapeArray[i].ToString());
                    }
                    shapeStrB.Remove(0, 3);
                    shapeString = shapeStrB.ToString();
                }
                return shapeString;
            }
        }
        #endregion public properties

        //#region protected variables
        //protected string Name = ""; // Used by status line: set ONLY by Save() -- i.e. when writing the newly created krystal's XML
        //protected uint Level; // the maximum level of any strand in the krystal
        //protected uint MinValue; // the minimum value in the krystal
        //protected uint MaxValue; // the maximum value in the krystal
        //protected uint NumValues; // the number of values in the krystal
        //protected List<Strand> Strands = new List<Strand>();
        //#endregion protected variables
    }
}

