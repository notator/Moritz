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
        /// Constructor for loading a complete expansion krystal from a file.
        /// The Krystal base class reads the strands.
        /// </summary>
        /// <param name="filepath"></param>
        public ExpansionKrystalBase(string filepath)
            : base(filepath)
        {
            // heredity info is read in sub-classes
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
        public string ExpanderFilename { get; set; }
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
}

