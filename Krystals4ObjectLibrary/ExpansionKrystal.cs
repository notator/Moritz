using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Krystals4ObjectLibrary
{
    /// <summary>
    /// A simple, non-contoured expansion
    /// </summary>
    public sealed class ExpansionKrystal : ExpansionKrystalBase
    {
        #region constructors
        /// <summary>
        /// Constructor for loading a complete expansion krystal from a file.
        /// This constructor reads the heredity info, and constructs the corresponding objects.
        /// The Krystal base class reads the strands.
        /// </summary>
        /// <param name="filepath"></param>
        public ExpansionKrystal(string filepath)
            : base(filepath)
        {
            using(XmlReader r = XmlReader.Create(filepath))
            {
                K.ReadToXmlElementTag(r, "expansion"); // check that this is an expansion (the other checks have been done in base()
                for(int attr = 0 ; attr < r.AttributeCount ; attr++)
                {
                    r.MoveToAttribute(attr);
                    switch(r.Name)
                    {
                        case "density":
                            _densityInputFilename = r.Value;
                            break;
                        case "inputPoints":
                            _pointsInputFilename = r.Value;
                            break;
                        case "expander":
                            ExpanderFilename = r.Value;
                            break;
                    }
                }
            }
            string densityInputFilepath = K.KrystalsFolder + @"\" + _densityInputFilename;
            string pointsInputFilepath = K.KrystalsFolder + @"\" + _pointsInputFilename;
            string expanderFilepath = K.ExpansionOperatorsFolder + @"\" + ExpanderFilename;

            _densityInputKrystal = new DensityInputKrystal(densityInputFilepath);
            _pointsInputKrystal = new PointsInputKrystal(pointsInputFilepath);
            Expander = new Expander(expanderFilepath, _densityInputKrystal);
        }
        /// <summary>
        /// Constructor used when beginning to edit a new krystal (which has no strands yet).
        /// This constructor generates the strands and a unique name for the krystal.
        /// </summary>
        /// <param name="densityInputFilepath">The file path to the density input</param>
        /// <param name="inputValuesFilepath">The file path to the input values</param>
        /// <param name="expanderFilepath">The file path to the expander (may be null or empty)</param>
        public ExpansionKrystal(string densityInputFilepath,
                                string pointsInputFilepath,
                                string expanderFilepath)
            : base(densityInputFilepath, pointsInputFilepath, expanderFilepath)
        {
            var ek = new ExpansionKrystal(_densityInputKrystal, _pointsInputKrystal, _expander);
            _strands = ek.Strands;
            _name = ek.Name; 
        }
        /// <summary>
        /// Constructor used when the density and points input krystals, and the Expander are already available.
        /// Expand() is called in this constructor to create the strands and strand related properties.
        /// </summary>
        /// <param name="densityInputFilepath">The file path to the density input</param>
        /// <param name="inputValuesFilepath">The file path to the input values</param>
        /// <param name="expander">The expansion field consisting of input and output gametes</param>
        public ExpansionKrystal(DensityInputKrystal densityInputKrystal,
                                PointsInputKrystal pointsInputKrystal,
                                Expander expander)
            : base(densityInputKrystal, pointsInputKrystal, expander)
        {
            _name = GetUniqueName(K.KrystalType.exp);
        }

        #endregion

        #region public virtual
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
                string msg = $"Expansion krystal {_name} already exists. Save it again with a new date?";
                answer = MessageBox.Show(msg, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            }

            if(answer == DialogResult.Yes)
            {
                XmlWriter w = base.BeginSaveKrystal(); // disposed of in EndSaveKrystal
                #region save heredity info
                w.WriteStartElement("expansion");
                w.WriteAttributeString("density", this.DensityInputFilename);
                w.WriteAttributeString("inputPoints", this.PointsInputFilename);
                w.WriteAttributeString("expander", this.ExpanderFilename);
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

