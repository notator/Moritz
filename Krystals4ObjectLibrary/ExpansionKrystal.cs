using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Moritz.Globals;
using System.Linq;

namespace Krystals4ObjectLibrary
{
    public abstract class ExpansionKrystalBase : Krystal
    {
        #region constructors
        /// <summary>
        /// constructor for loading a complete expansion krystal from a file
        /// </summary>
        /// <param name="filepath"></param>
        public ExpansionKrystalBase(string filepath)
            : base(filepath)
        {
        }

        /// <summary>
        /// Constructor used when beginning to edit a new krystal (which has no strands yet).
        /// </summary>
        /// <param name="densityInputFilepath">The file path to the density input</param>
        /// <param name="inputValuesFilepath">The file path to the input values</param>
        /// <param name="expanderFilepath">The file path to the expander (may be null or empty)</param>
        public ExpansionKrystalBase(string densityInputFilepath,
                                string pointsInputFilepath,
                                string expanderFilepath)
            : base()
        {
            if(String.IsNullOrEmpty(densityInputFilepath))
                _densityInputKrystal = null;
            else
            {
                _densityInputFilename = Path.GetFileName(densityInputFilepath);
                _densityInputKrystal = new DensityInputKrystal(densityInputFilepath);
            }

            if(String.IsNullOrEmpty(pointsInputFilepath))
                _pointsInputKrystal = null;
            else
            {
                _pointsInputFilename = Path.GetFileName(pointsInputFilepath);
                _pointsInputKrystal = new PointsInputKrystal(pointsInputFilepath);
            }

            if(String.IsNullOrEmpty(expanderFilepath))
                _expander = new Expander();
            else
                _expander = new Expander(expanderFilepath, _densityInputKrystal);

            if(_densityInputKrystal != null && _pointsInputKrystal != null)
            {
                this._level = _densityInputKrystal.Level > _pointsInputKrystal.Level ? _densityInputKrystal.Level : _pointsInputKrystal.Level;
                this._level++;
            }
        }
        /// <summary>
        /// Constructor used when the density and points input krystals, and the Expander are already available.
        /// Expand() is called in this constructor to create the strands.
        /// </summary>
        /// <param name="densityInputFilepath">The file path to the density input</param>
        /// <param name="inputValuesFilepath">The file path to the input values</param>
        /// <param name="expander">The expansion field consisting of input and output gametes</param>
        public ExpansionKrystalBase(DensityInputKrystal densityInputKrystal,
                                PointsInputKrystal pointsInputKrystal,
                                Expander expander)
            : base()
        {
            _densityInputKrystal = densityInputKrystal;
            _pointsInputKrystal = pointsInputKrystal;
            _expander = expander;

            if(_densityInputKrystal != null && _pointsInputKrystal != null)
            {
                this._level = (_densityInputKrystal.Level > _pointsInputKrystal.Level) ?
                    _densityInputKrystal.Level : _pointsInputKrystal.Level;
                this._level++;
            }
            Expand();
        }
        #endregion
        #region public
        /// <summary>
        /// Re-expands this krystal (using the existing input krystals and expander), then saves it,
        /// overwriting the existing file.
        /// All the krystals in the krystals folder are rebuilt, when one of them has been changed.
        /// </summary>
        public override void Rebuild()
        {
            Expand();
            this.Save(); // old was this.Save(true, false); Save(bool overwriteKrystal, bool overwriteExpander);
        }

        #region virtual
        public abstract void Expand();
        public abstract List<StrandNode> StrandNodeList();
        #endregion
        public DensityInputKrystal DensityInputKrystal
        {
            get { return _densityInputKrystal; }
            set { _densityInputKrystal = value; }
        }
        public PointsInputKrystal PointsInputKrystal
        {
            get { return _pointsInputKrystal; }
            set { _pointsInputKrystal = value; }
        }
        public string DensityInputFilename
        {
            get { return _densityInputFilename; }
            set { _densityInputFilename = value; }
        }
        public string PointsInputFilename
        {
            get { return _pointsInputFilename; }
            set { _pointsInputFilename = value; }
        }
        public Expander Expander
        {
            get { return _expander; }
            set { _expander = value; }
        }
        #endregion public
        #region private
        protected string _densityInputFilename = "";
        protected string _pointsInputFilename = "";
        protected DensityInputKrystal _densityInputKrystal = null;
        protected PointsInputKrystal _pointsInputKrystal = null;
        protected Expander _expander = new Expander();
        #endregion private
    }

    /// <summary>
    /// A simple, non-contoured expansion
    /// </summary>
    public sealed class ExpansionKrystal : ExpansionKrystalBase
    {
        #region constructors
        /// <summary>
        /// constructor for loading a complete expansion krystal from a file
        /// </summary>
        /// <param name="filepath"></param>
        public ExpansionKrystal(string filepath)
            : base(filepath)
        {
            string expanderFilename = "";
            using(XmlReader r = XmlReader.Create(filepath))
            {
                K.ReadToXmlElementTag(r, "expansion"); // check that this is an expansion (the other checks have been done in base()
                for(int attr = 0 ; attr < r.AttributeCount ; attr++)
                {
                    r.MoveToAttribute(attr);
                    switch(r.Name)
                    {
                        case "density":
                            this._densityInputFilename = r.Value;
                            break;
                        case "inputPoints":
                            this._pointsInputFilename = r.Value;
                            break;
                        case "expander":
                            expanderFilename = r.Value;
                            break;
                    }
                }
            }
            string densityInputFilepath = K.KrystalsFolder + @"\" + _densityInputFilename;
            string pointsInputFilepath = K.KrystalsFolder + @"\" + _pointsInputFilename;
            string expanderFilepath = K.ExpansionOperatorsFolder + @"\" + expanderFilename;

            _densityInputKrystal = new DensityInputKrystal(densityInputFilepath);
            _pointsInputKrystal = new PointsInputKrystal(pointsInputFilepath);
            Expander = new Expander(expanderFilepath, _densityInputKrystal);
        }
        /// <summary>
        /// Constructor used when beginning to edit a new krystal (which has no strands yet).
        /// </summary>
        /// <param name="densityInputFilepath">The file path to the density input</param>
        /// <param name="inputValuesFilepath">The file path to the input values</param>
        /// <param name="expanderFilepath">The file path to the expander (may be null or empty)</param>
        public ExpansionKrystal(string densityInputFilepath,
                                string pointsInputFilepath,
                                string expanderFilepath)
            : base(densityInputFilepath, pointsInputFilepath, expanderFilepath)
        {
        }
        /// <summary>
        /// Constructor used when the density and points input krystals, and the Expander are already available.
        /// Expand() is called in this constructor to create the strands.
        /// </summary>
        /// <param name="densityInputFilepath">The file path to the density input</param>
        /// <param name="inputValuesFilepath">The file path to the input values</param>
        /// <param name="expander">The expansion field consisting of input and output gametes</param>
        public ExpansionKrystal(DensityInputKrystal densityInputKrystal,
                                PointsInputKrystal pointsInputKrystal,
                                Expander expander)
            : base(densityInputKrystal, pointsInputKrystal, expander)
        {
        }
        #endregion

        #region public virtual
        /// <summary>
        /// Sets the krystal's Name, and saves it (but not any of its ancestor files).
        /// If a krystal having identical content exists in the krystals directory,
        /// the user is given the option to
        ///    either overwrite the existing krystal (using that krystal's index),
        ///    or abort the save.
        /// This means that a given set of ancestors should always have the same index.
        /// </summary>
        public override void Save()
        {
            bool ExpansionIsUnique(out string name)
            {
                var isUnique = true;
                var nameRoot = GetNameRoot();

                IEnumerable<string> similarKrystalPaths = GetSimilarKrystalPaths(nameRoot, K.KrystalType.exp);

                name = nameRoot + (similarKrystalPaths.Count() + 1).ToString() + K.ModulatorFilenameSuffix;

                foreach(var existingPath in similarKrystalPaths)
                {
                    var existingKrystal = new ExpansionKrystal(existingPath);
                    if(existingKrystal.PointsInputFilename == PointsInputFilename
                    && existingKrystal.DensityInputFilename == DensityInputFilename
                    && existingKrystal.Expander.Name == Expander.Name)
                    {
                        isUnique = false;
                        name = Path.GetFileName(existingPath);
                        break;
                    }
                }
                return isUnique;
            }

            DialogResult answer = DialogResult.Yes;
            if(ExpansionIsUnique(out _name) == false)
            {
                string msg = $"Expansion krystal {_name} already existed. Save it again with a new date?";
                answer = MessageBox.Show(msg, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            }

            if(answer == DialogResult.Yes)
            {
                XmlWriter w = base.BeginSaveKrystal(); // disposed of in EndSaveKrystal
                #region save heredity info
                w.WriteStartElement("expansion");
                w.WriteAttributeString("density", this._densityInputFilename);
                w.WriteAttributeString("inputPoints", this._pointsInputFilename);
                w.WriteAttributeString("expander", this._expander.Name);
                w.WriteEndElement(); // expansion
                #endregion
                base.EndSaveKrystal(w); // saves the strands, closes the document, disposes of w
            }
        }
        public override void Expand()
        {
            try
            {
                Expansion expansion = new Expansion(this);
                this.Update(expansion.Strands);

            }
            catch(ApplicationException ex)
            {
                throw ex;
            }
        }
        public override List<StrandNode> StrandNodeList()
        {
            DensityInputKrystal dKrystal = this.DensityInputKrystal;
            PointsInputKrystal pKrystal = this.PointsInputKrystal;

            if(dKrystal.Level < pKrystal.Level)
            {
                string msg = "Error: The level of the density input krystal must be\n"
                        + "greater than or equal to the level of any other input krystals.";
                throw new ApplicationException(msg);
            }
            int[] alignedInputPointValues = pKrystal.AlignedValues(dKrystal);

            if(dKrystal.NumValues != alignedInputPointValues.Length)
            {
                string msg = "Error: All the input krystals must belong to the same density family.\n";
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
        #endregion public virtual
    }

    public sealed class ShapedExpansionKrystal : ExpansionKrystalBase
    {
        #region constructors
        /// <summary>
        /// constructor for loading a complete, shaped expansion krystal from a file
        /// </summary>
        /// <param name="filepath"></param>
        public ShapedExpansionKrystal(string filepath)
            : base(filepath)
        {
            string expanderFilename = "";
            using(XmlReader r = XmlReader.Create(filepath))
            {
                K.ReadToXmlElementTag(r, "expansion"); // check that this is an expansion (the other checks have been done in base()
                for(int attr = 0 ; attr < 5 ; attr++)
                {
                    r.MoveToAttribute(attr);
                    switch(r.Name)
                    {
                        case "density":
                            this.DensityInputFilename = r.Value;
                            break;
                        case "inputPoints":
                            this.PointsInputFilename = r.Value;
                            break;
                        case "axis":
                            this._axisInputFilename = r.Value;
                            break;
                        case "contour":
                            this._contourInputFilename = r.Value;
                            break;
                        case "expander":
                            expanderFilename = r.Value;
                            break;
                    }
                }
            }
            string densityInputFilepath = K.KrystalsFolder + @"\" + DensityInputFilename;
            string pointsInputFilepath = K.KrystalsFolder + @"\" + PointsInputFilename;
            string axisInputFilepath = K.KrystalsFolder + @"\" + _axisInputFilename;
            string contourInputFilepath = K.KrystalsFolder + @"\" + _contourInputFilename;
            string expanderFilepath = K.ExpansionOperatorsFolder + @"\" + expanderFilename;

            DensityInputKrystal = new DensityInputKrystal(densityInputFilepath);
            PointsInputKrystal = new PointsInputKrystal(pointsInputFilepath);
            AxisInputKrystal = new AxisInputKrystal(axisInputFilepath);
            ContourInputKrystal = new ContourInputKrystal(contourInputFilepath);
            Expander = new Expander(expanderFilepath, DensityInputKrystal);
        }

        /// <summary>
        /// Constructor used by Moritz to construct a shaped expansion krystal (which has no strands yet).
        /// </summary>
        /// <param name="densityInputFilepath">The file path to the density input</param>
        /// <param name="pointsInputFilepath">The file path to the points input values</param>
        /// <param name="axisInputFilepath">The file path to the axis values of the shape</param>
        /// <param name="contourInputFilepath">The file path to the contour numbers of the shape</param>
        /// <param name="expanderFilepath">The file path to the expander (may be null or empty)</param>
        public ShapedExpansionKrystal(string densityInputFilepath,
                                string pointsInputFilepath,
                                string axisInputFilepath,
                                string contourInputFilepath,
                                string expanderFilepath)
            : base(densityInputFilepath, pointsInputFilepath, expanderFilepath)
        {
            if(String.IsNullOrEmpty(axisInputFilepath))
                _axisInputKrystal = null;
            else
            {
                _axisInputFilename = Path.GetFileName(axisInputFilepath);
                _axisInputKrystal = new AxisInputKrystal(axisInputFilepath);
            }

            if(String.IsNullOrEmpty(contourInputFilepath))
                _contourInputKrystal = null;
            else
            {
                _contourInputFilename = Path.GetFileName(contourInputFilepath);
                _contourInputKrystal = new ContourInputKrystal(contourInputFilepath);
            }
        }
        #endregion

        #region public virtual

        /// <summary>
        /// Sets the krystal's Name, and saves it (but not any of its ancestor files).
        /// If a krystal having identical content exists in the krystals directory,
        /// the user is given the option to
        ///    either overwrite the existing krystal (using that krystal's index),
        ///    or abort the save.
        /// This means that a given set of ancestors should always have the same index.
        /// </summary>
        public override void Save()
        {
            bool ShapedExpansionIsUnique(out string name)
            {
                var isUnique = true;
                var nameRoot = GetNameRoot();

                IEnumerable<string> similarKrystalPaths = GetSimilarKrystalPaths(nameRoot, K.KrystalType.shaped);

                name = nameRoot + (similarKrystalPaths.Count() + 1).ToString() + K.ModulatorFilenameSuffix;

                foreach(var existingPath in similarKrystalPaths)
                {
                    var existingKrystal = new ShapedExpansionKrystal(existingPath);
                    if(existingKrystal.DensityInputFilename == DensityInputFilename
                    && existingKrystal.PointsInputFilename == PointsInputFilename
                    && existingKrystal.AxisInputFilename == AxisInputFilename
                    && existingKrystal.ContourInputFilename == ContourInputFilename
                    && existingKrystal.Expander.Name == Expander.Name)
                    {
                        isUnique = false;
                        name = Path.GetFileName(existingPath);
                        break;
                    }
                }
                return isUnique;
            }

            DialogResult answer = DialogResult.Yes;
            if(ShapedExpansionIsUnique(out _name) == false)
            {
                string msg = $"ShapedExpansionKrystal {_name} already existed. Save it again with a new date?";
                answer = MessageBox.Show(msg, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            }

            if(answer == DialogResult.Yes)
            {
                XmlWriter w = base.BeginSaveKrystal(); // disposed of in EndSaveKrystal
                #region save heredity info
                w.WriteStartElement("expansion");
                w.WriteAttributeString("density", this._densityInputFilename);
                w.WriteAttributeString("inputPoints", this._pointsInputFilename);
                w.WriteAttributeString("axis", this._axisInputFilename);
                w.WriteAttributeString("contour", this._contourInputFilename);
                w.WriteAttributeString("expander", this._expander.Name);
                w.WriteEndElement(); // expansion
                #endregion
                base.EndSaveKrystal(w); // saves the strands, closes the document, disposes of w
            }
        }

        public override void Expand()
        {
            try
            {
                Expansion expansion = new Expansion(this);
                this.Update(expansion.Strands);
            }
            catch(ApplicationException ex)
            {
                throw ex;
            }
        }
        public override List<StrandNode> StrandNodeList()
        {
            DensityInputKrystal dKrystal = this.DensityInputKrystal;
            PointsInputKrystal pKrystal = this.PointsInputKrystal;
            AxisInputKrystal aKrystal = this.AxisInputKrystal;
            ContourInputKrystal cKrystal = this.ContourInputKrystal;

            if(aKrystal == null || cKrystal == null)
            {
                string msg = "Error: Both the axis and contour inputs must be set.";
                throw new ApplicationException(msg);
            }

            if(dKrystal.Level < pKrystal.Level
                    || (aKrystal != null && dKrystal.Level < aKrystal.Level)
                    || (cKrystal != null && dKrystal.Level < cKrystal.Level))
            {
                string msg = "Error: The level of the density input krystal must be\n"
                        + "greater than or equal to the level of all the other input krystals.";
                throw new ApplicationException(msg);
            }
            int[] alignedInputPointValues = pKrystal.AlignedValues(dKrystal);
            int[] alignedInputAxisValues = { };
            if(aKrystal != null)
                alignedInputAxisValues = aKrystal.AlignedValues(dKrystal);
            int[] alignedInputContourValues = { };
            if(cKrystal != null)
                alignedInputContourValues = cKrystal.AlignedValues(dKrystal);

            if(dKrystal.NumValues != alignedInputPointValues.Length
                || (aKrystal != null && dKrystal.NumValues != alignedInputAxisValues.Length)
                || (cKrystal != null && dKrystal.NumValues != alignedInputContourValues.Length))
            {
                string msg = "Error: All the input krystals must belong to the same density family.\n";
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
                if(mVal == 0
                    || alignedInputPointValues[momentIndex] == 0
                    || alignedInputAxisValues[momentIndex] == 0
                    || alignedInputContourValues[momentIndex] == 0)
                {
                    string msg = "Error: An input krystal contained a value of zero.";
                    throw new ApplicationException(msg);
                }

                ContouredStrandNode csn = new ContouredStrandNode(momentIndex+1, level, mVal,
                        alignedInputPointValues[momentIndex],
                        alignedInputAxisValues[momentIndex],
                        alignedInputContourValues[momentIndex]);

                strandNodeList.Add(csn);
                momentIndex++;
            }
            return (strandNodeList);
        }
        #endregion public virtual

        public AxisInputKrystal AxisInputKrystal
        {
            get { return _axisInputKrystal; }
            set { _axisInputKrystal = value; }
        }
        public ContourInputKrystal ContourInputKrystal
        {
            get { return _contourInputKrystal; }
            set { _contourInputKrystal = value; }
        }
        public string AxisInputFilename
        {
            get { return _axisInputFilename; }
            set { _axisInputFilename = value; }
        }
        public string ContourInputFilename
        {
            get { return _contourInputFilename; }
            set { _contourInputFilename = value; }
        }
        private string _axisInputFilename = "";
        private string _contourInputFilename = "";
        private AxisInputKrystal _axisInputKrystal = null;
        private ContourInputKrystal _contourInputKrystal = null;
    }

    /// <summary>
    /// The density input krystal for an expansion has this class
    /// </summary>
    public sealed class DensityInputKrystal : InputKrystal
    {
        public DensityInputKrystal(string filepath)
            : base(filepath)
        {
            CalculateRelativePlanetPointPositions();
        }
        #region properties
        public float[] RelativePlanetPointPositions { get { return _relativePlanetPointPositions; } }
        #endregion properties
        #region private functions
        /// <summary>
        /// This function sets the private _relativePlanetPointPositions field for this krystal.
        /// The relative planet point positions are the positions of the points along a straight
        /// line having a length of one abstract unit. The actual positions of the points are
        /// calculated by distributing these relative positions along the actual path of the planet.
        /// </summary>
        private void CalculateRelativePlanetPointPositions()
        {
            int nValues = 0;
            foreach(Strand strand in this._strands)
                nValues += strand.Values.Count;
            _relativePlanetPointPositions = new float[nValues];
            float[] pos = _relativePlanetPointPositions; // just an alias
            const float width = 1.0f;
            float levelWidth;

            if(nValues == 1) // this is a constant krystal or a line krystal with one value
                return;      // _relativePlanetPointPositions[0] == 0
            else if(this.Level == 1) // a single strand with more than one value
            {
                levelWidth = width / (nValues - 1);
                for(int i = 0 ; i < nValues ; i++)
                    pos[i] = levelWidth * i;
                return;
            }
            else // this.Level > 1
            {
                pos[0] = 0.0f; // does not change
                pos[nValues - 1] = width; // does not change
                uint[] levels = new uint[nValues];
                #region set levels array
                uint maxValueLevel = this._level + 1;
                int index = 0;
                foreach(Strand strand in this._strands)
                {
                    levels[index] = strand.Level;
                    index++;
                    int nMaxLevelValues = strand.Values.Count - 1;
                    while(nMaxLevelValues > 0)
                    {
                        levels[index] = maxValueLevel;
                        index++;
                        nMaxLevelValues--;
                    }
                }
                #endregion set levels array
                for(uint currentLevel = 2 ; currentLevel <= maxValueLevel ; currentLevel++)
                {
                    int startIndex = 0;
                    while(startIndex < nValues)
                    {
                        int nSections = 0;
                        int valueIndex;
                        for(valueIndex = startIndex ; valueIndex < nValues ; valueIndex++)
                        {
                            if(startIndex == valueIndex || levels[valueIndex] == currentLevel)
                                nSections++;
                            if(startIndex != valueIndex && levels[valueIndex] < currentLevel)
                                break;
                        }
                        float currentLevelWidth;
                        if(valueIndex == nValues)
                            currentLevelWidth = (width - pos[startIndex]) / nSections;
                        else
                            currentLevelWidth = (pos[valueIndex] - pos[startIndex]) / nSections;
                        float position = pos[startIndex] + currentLevelWidth; ;
                        for(int setValueIndex = startIndex ; setValueIndex < valueIndex ; setValueIndex++)
                        {
                            if(levels[setValueIndex] == currentLevel)
                            {
                                pos[setValueIndex] = position;
                                position += currentLevelWidth;
                            }
                        }
                        startIndex = valueIndex;
                    } // while
                }
            }
        }
        #endregion private functions
        #region private variables
        private float[] _relativePlanetPointPositions;
        #endregion private variables
    }
    /// <summary>
    /// The points input krystal for an expansion has this class
    /// </summary>
    public sealed class PointsInputKrystal : InputKrystal
    {
        public PointsInputKrystal(string filepath)
            : base(filepath)
        {
        }
    }

    /// <summary>
    /// When contouring a krystal, the axis input has this class
    /// </summary>
    public sealed class AxisInputKrystal : InputKrystal
    {
        public AxisInputKrystal(string filepath)
            : base(filepath)
        {
        }
    }

    /// <summary>
    /// When contouring a krystal, the contourNumber input has this class
    /// </summary>
    public sealed class ContourInputKrystal : InputKrystal
    {
        public ContourInputKrystal(string filepath)
            : base(filepath)
        {
        }
    }
 }

