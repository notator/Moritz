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
 }

