using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using Moritz.Globals;
using System.Linq;

namespace Krystals4ObjectLibrary
{
    public sealed class ModulationKrystal : Krystal
    {
        #region constructors
        /// <summary>
        /// Constructor for loading a complete modulated krystal from a file.
        /// This constructor reads the heredity info, and constructs the corresponding objects.
        /// The Krystal base class reads the strands.
        /// </summary>
        /// <param name="filepath"></param>
        public ModulationKrystal(string filepath)
            : base(filepath)
        {
            string NewModulatorFilename(string oldName)
            {
                string on = oldName;
                string newName = oldName;
                if(on.StartsWith("m"))
                {
                    int mInd = on.IndexOf("m"); // = 0
                    int xInd = on.IndexOf("x");
                    int bInd = on.IndexOf("(");
                    int iInd = on.IndexOf("-");
                    int dotInd = on.IndexOf(".");

                    string xStr = on.Substring(mInd + 1, xInd - mInd - 1);
                    string yStr = on.Substring(xInd + 1, bInd - xInd - 1);
                    string dStr = on.Substring(bInd + 1, iInd - bInd - 2);
                    string iStr = on.Substring(iInd + 1, dotInd - iInd - 1);

                    int.TryParse(xStr, out int xDim);
                    int.TryParse(yStr, out int yDim);
                    int.TryParse(dStr, out int maxValue);
                    int.TryParse(iStr, out int index);
                    newName = String.Format($"{xDim}.{yDim}.{maxValue}.{index}.kmod");
                }

                return newName;
            }

            using(XmlReader r = XmlReader.Create(filepath))
            {
                K.ReadToXmlElementTag(r, "modulation"); // check that this is a modulation (the other checks have been done in base()
                for(int attr = 0; attr < 3; attr++)
                {
                    r.MoveToAttribute(attr);
                    switch(r.Name)
                    {
                        case "x":
                            this.XInputFilename = r.Value;
                            break;
                        case "y":
                            this.YInputFilename = r.Value;
                            break;
                        case "modulator":
                            this.ModulatorFilename = r.Value;
                            break;
                    }
                }
            }
            string xInputFilepath = K.KrystalsFolder + @"\" + XInputFilename;
            string yInputFilepath = K.KrystalsFolder + @"\" + YInputFilename;

            if(ModulatorFilename.StartsWith("m"))
            {
                ModulatorFilename = NewModulatorFilename(ModulatorFilename);
            }
            string modulatorFilepath = K.ModulationOperatorsFolder + @"\" + ModulatorFilename;

            _xInputKrystal = new ModulationInputKrystal(xInputFilepath);
            _yInputKrystal = new ModulationInputKrystal(yInputFilepath);
            _modulator = new Modulator(modulatorFilepath);

            _modulationNodeList = GetModulationNodeList();

            SetRedundantQualifierCoordinates();
        }
        /// <summary>
        /// Constructor used when beginning to edit a new modulated krystal (which has no modulator or strands yet).
        /// This constructor generates a unique name for the krystal.
        /// </summary>
        /// <param name="xInputFilepath">The file path to the x input</param>
        /// <param name="yInputFilepath">The file path to the y input</param>
        /// <param name="modulatorFilepath">The file path to the krystal containing the modulator (may be null or empty)</param>
        public ModulationKrystal(string xInputFilepath, string yInputFilepath, string modulatorFilepath)
            : base()
        {
            XInputFilename = Path.GetFileName(xInputFilepath);
            YInputFilename = Path.GetFileName(yInputFilepath);

            _xInputKrystal = new ModulationInputKrystal(xInputFilepath);
            _yInputKrystal = new ModulationInputKrystal(yInputFilepath);

            _modulationNodeList = GetModulationNodeList();

            this._level = _yInputKrystal.Level > _xInputKrystal.Level ? _yInputKrystal.Level : _xInputKrystal.Level;

            if(_yInputKrystal.Level == _xInputKrystal.Level && _yInputKrystal.NumValues != _xInputKrystal.NumValues)
                throw new ApplicationException("Error: the two input krystals are not of compatible size.");
            if(_yInputKrystal.Level == 0 && _xInputKrystal.Level == 0)
                throw new ApplicationException("Error: the two input krystals cannot both be constants.");

            if(string.IsNullOrEmpty(modulatorFilepath))
            {
                _modulator = new Modulator((int)_xInputKrystal.MaxValue, (int)_yInputKrystal.MaxValue);
            }
            else
            {
                _modulator = new Modulator(modulatorFilepath);

                if(_modulator.XDim < _xInputKrystal.MaxValue || _modulator.YDim < _yInputKrystal.MaxValue)
                    throw new ApplicationException("Error: One or more input values exceed the bounds of the modulator.");
            }
            SetRedundantQualifierCoordinates();
        }

        #endregion
        #region public functions
        private List<ModulationNode> GetModulationNodeList()
        {
            //InputKrystal xInputKrystal, InputKrystal yInputKrystal
            List<ModulationNode> modulationNodeList = new List<ModulationNode>();

            InputKrystal master, slave;
            if(_xInputKrystal.Level > _yInputKrystal.Level)
            {
                master = _xInputKrystal;
                slave = _yInputKrystal;
            }
            else
            {
                slave = _xInputKrystal;
                master = _yInputKrystal;
            }

            int[] alignedSlaveValues = slave.AlignedValues(master);
            // first construct a flat list of modulation nodes (the leaf nodes of the final tree)
            int momentIndex = 0;
            foreach(LeveledValue leveledValue in master.LeveledValues)
            {
                int level = leveledValue.level;
                int mVal = leveledValue.value;
                if(mVal == 0 || alignedSlaveValues[momentIndex] == 0)
                {
                    string msg = "Error: An input krystal contained a value of zero.";
                    throw new ApplicationException(msg);
                }
                ModulationNode mn;
                if(master == _xInputKrystal)
                    mn = new ModulationNode(momentIndex + 1, level, mVal, alignedSlaveValues[momentIndex]);
                else
                    mn = new ModulationNode(momentIndex + 1, level, alignedSlaveValues[momentIndex], mVal);
                modulationNodeList.Add(mn);
                momentIndex++;
            }
            return modulationNodeList;
        }

        /// <summary>
        /// If a krystal having identical content exists in the krystals directory,
        /// the user is given the option to
        ///    either overwrite the existing krystal (using that krystal's index),
        ///    or abort the save.
        /// This means that a given set of ancestors should always have the same index.
        /// </summary>
        public override void Save()
        {
            var pathname = K.KrystalsFolder + @"\" + _name;
            DialogResult answer = DialogResult.Yes;
            if(File.Exists(pathname))
            {
                string msg = $"ModulationKrystal {_name} already exists. Save it again with a new date?";
                answer = MessageBox.Show(msg, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            }

            if(answer == DialogResult.Yes)
            {
                XmlWriter w = base.BeginSaveKrystal(); // disposed of in EndSaveKrystal
                #region save heredity info
                w.WriteStartElement("modulation");
                w.WriteAttributeString("x", this.XInputFilename);
                w.WriteAttributeString("y", this.YInputFilename);
                w.WriteAttributeString("modulator", this._modulator.Name);
                w.WriteEndElement(); // expansion
                #endregion
                base.EndSaveKrystal(w); // saves the strands, closes the document, disposes of w
            }
        }

        private bool MaxValueHasChanged()
        {
            string[] segs = _name.Split('.');
            uint max = uint.Parse(segs[0]);
            if(max == MaxValue)
                return false;
            else
                return true;
        }

        public void Modulate()
        {
            foreach(ModulationNode m in _modulationNodeList)
                m.ModResult = _modulator.Array[m.X - 1, m.Y - 1];
            #region convert to a list of strands
            List<Strand> strandList = new List<Strand>();
            List<uint> valueList = new List<uint>();
            uint strandValueLevel = this.Level + 1;
            Strand s = new Strand(1, valueList);
            ModulationNode mNode;
            for(int index = 0; index < _modulationNodeList.Count; index++)
            {
                mNode = _modulationNodeList[index];
                if(mNode.ModLevel < strandValueLevel && index > 0)
                {
                    strandList.Add(s);
                    valueList = new List<uint>();
                    s = new Strand((uint)mNode.ModLevel, valueList);
                }
                s.Values.Add((uint)mNode.ModResult);
            }
            strandList.Add(s);
            #endregion convert to a list of strands

            this.Update(strandList);
        }

        /// <summary>
        /// Re-modulates this krystal (using the existing input krystals and modulator), then saves it,
        /// overwriting the existing file.
        /// All the krystals in the krystals folder are rebuilt, when one of them has been changed.
        /// </summary>
        public override void Rebuild()
        {
            this.Modulate();
            Save();
        }
        #endregion public functions
        #region Properties
        /// <summary>
        /// A string of uints separated by spaces, containing the x-values which occur in the
        /// modulator, but which do not have corresponding values in the X-input krystal.
        /// </summary>
        public List<uint> RedundantQualifierXInputValues { get { return _redundantModifierXInputValues; } }
        /// <summary>
        /// A string of uints separated by spaces, containing the y-values which occur in the
        /// modulator, but which do not have corresponding values in the Y-input krystal.
        /// </summary>
        public List<uint> RedundantQualifierYInputValues { get { return _redundantModifierYInputValues; } }
        public string XInputFilename { get; set; }
        public string YInputFilename { get; set; }
        public string ModulatorFilename { get; set; }

        public ModulationInputKrystal XInputKrystal
        {
            get { return _xInputKrystal; }
            set { _xInputKrystal = value; }
        }
        public ModulationInputKrystal YInputKrystal
        {
            get { return _yInputKrystal; }
            set { _yInputKrystal = value; }
        }
        public Modulator Modulator
        {
            get { return _modulator; }
            set { _modulator = value; }
        }
        public List<ModulationNode> ModulationNodeList { get { return _modulationNodeList; } }
        private List<ModulationNode> _modulationNodeList;
        public List<StrandNode> StrandNodeList
        {
            get
            {
                ModulationInputKrystal dKrystal;
                ModulationInputKrystal pKrystal;
                if(_xInputKrystal.Level >= _yInputKrystal.Level)
                {
                    dKrystal = this._xInputKrystal;
                    pKrystal = this._yInputKrystal;
                }
                else
                {
                    dKrystal = this._yInputKrystal;
                    pKrystal = this._xInputKrystal;
                }

                int[] alignedInputPointValues = pKrystal.AlignedValues(dKrystal);
                if(dKrystal.NumValues != alignedInputPointValues.Length)
                {
                    string msg = "Error: The input krystals must belong to the same density family.\n";
                    throw new ApplicationException(msg);
                }
                List<LeveledValue> leveledValues = new List<LeveledValue>();
                foreach(LeveledValue leveledValue in dKrystal.LeveledValues)
                    leveledValues.Add(leveledValue);

                // construct the list of StrandNodes
                List<StrandNode> strandNodeList = new List<StrandNode>();
                int momentIndex = 0;
                foreach(LeveledValue leveledValue in leveledValues)
                {
                    int level = leveledValue.level;
                    int mVal = leveledValue.value;
                    if(mVal == 0 || alignedInputPointValues[momentIndex] == 0)
                    {
                        string msg = "Error: An input krystal contained a value of zero.";
                        throw new ApplicationException(msg);
                    }
                    StrandNode sn = new StrandNode(momentIndex + 1, level, mVal, alignedInputPointValues[momentIndex]);
                    strandNodeList.Add(sn);
                    momentIndex++;
                }
                return (strandNodeList);
            }
        }
        #endregion Properties
        #region private functions
        /// <summary>
        /// Called from ModulationKrystal constructors, this function sets the private variables
        ///     _redundantQualifierXInputValues and
        ///     _redundantQualifierYInputValues
        /// These are lists of integers containing the legal modulator coordinates which are not actually used
        /// by the x-input or y-input krystals
        /// </summary>
        private void SetRedundantQualifierCoordinates()
        {
            List<int> kValuesList = _xInputKrystal.AbsoluteValues; // set by the InputKrystal constructor
            _redundantModifierXInputValues.Clear();
            for(uint x = 1; x <= _modulator.XDim; x++)
                if(!kValuesList.Contains((int)x))
                    _redundantModifierXInputValues.Add(x);

            kValuesList = _yInputKrystal.AbsoluteValues; // set by the InputKrystal constructor
            _redundantModifierYInputValues.Clear();
            for(uint y = 1; y <= _modulator.YDim; y++)
                if(!kValuesList.Contains((int)y))
                    _redundantModifierYInputValues.Add(y);
        }
        #endregion private functions
        #region private variables
        private List<uint> _redundantModifierXInputValues = new List<uint>();
        private List<uint> _redundantModifierYInputValues = new List<uint>();
        //private string _modulatorFilename;
        private ModulationInputKrystal _xInputKrystal;
        private ModulationInputKrystal _yInputKrystal;
        private Modulator _modulator;
        #endregion private variables
    }

    /// <summary>
    /// The X- and Y-Input krystals for a modulation both have this class
    /// </summary>
    public sealed class ModulationInputKrystal : InputKrystal
    {
        public ModulationInputKrystal(string filepath)
            : base(filepath)
        {
        }
    }

    /// <summary>
    /// This class contains modulation parameters, and is used to build the _modulationNodeList for a modulation.
    /// The _modulationNodeList is used as a parameter in the following constructor:
    ///     ModulationEditor.ModulationTreeView
    /// and in the function:
    ///     ModulationEditor.Modulate()
    /// </summary>
    public class ModulationNode : TreeNode
    {
        public ModulationNode(int moment, int level, int x, int y)
        {
            ModMoment = moment;
            ModLevel = level;
            X = x;
            Y = y;
        }
        public int ModMoment;
        public int ModLevel;
        public int X;
        public int Y;
        public int ModResult;
    }

}

