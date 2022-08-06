using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Moritz.Globals;
using System.Linq;

namespace Krystals4ObjectLibrary
{
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
 }

