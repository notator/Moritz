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
    public sealed class ExpansionKrystal : Krystal
    {
        #region constructors
        /// <summary>
        /// Constructor for loading an expansion krystal from a file.
        /// By default, this constructor creates the KyIf onlyLoadHeredityInfo is set to true, 
        /// This constructor reads the heredity info, and constructs the corresponding objects.
        /// The Krystal base class reads the strands.
        /// </summary>
        /// <param name="filepath"></param>
        public ExpansionKrystal(string filepath, bool onlyLoadHeredityInfo = false)
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
                            DensityInputFilename = r.Value;
                            break;
                        case "inputPoints":
                            PointsInputFilename = r.Value;
                            break;
                        case "expander":
                            ExpanderFilename = r.Value;
                            break;
                    }
                }
            }

            if(onlyLoadHeredityInfo == false)
            {
                string densityInputFilepath = K.KrystalsFolder + @"\" + DensityInputFilename;
                string pointsInputFilepath = K.KrystalsFolder + @"\" + PointsInputFilename;
                string expanderFilepath = K.ExpansionOperatorsFolder + @"\" + ExpanderFilename;

                DensityInputKrystal = new DensityInputKrystal(densityInputFilepath);
                PointsInputKrystal = new PointsInputKrystal(pointsInputFilepath);
                Expander = new Expander(expanderFilepath, DensityInputKrystal);
            }
        }
        /// <summary>
        /// This constructor generates the strands and a unique name for the krystal.
        /// </summary>
        /// <param name="densityInputFilepath">The file path to the density input</param>
        /// <param name="inputValuesFilepath">The file path to the input values</param>
        /// <param name="expanderFilepath">The file path to the expander (may be null or empty)</param>
        public ExpansionKrystal(string densityInputFilepath,
                                string pointsInputFilepath,
                                string expanderFilepath)
            : base()
        {
            if(String.IsNullOrEmpty(densityInputFilepath))
                DensityInputKrystal = null;
            else
            {
                DensityInputFilename = Path.GetFileName(densityInputFilepath);
                DensityInputKrystal = new DensityInputKrystal(densityInputFilepath);
            }

            if(String.IsNullOrEmpty(pointsInputFilepath))
                PointsInputKrystal = null;
            else
            {
                PointsInputFilename = Path.GetFileName(pointsInputFilepath);
                PointsInputKrystal = new PointsInputKrystal(pointsInputFilepath);
            }

            if(String.IsNullOrEmpty(expanderFilepath))
                Expander = new Expander();
            else
                Expander = new Expander(expanderFilepath, DensityInputKrystal);

            if(DensityInputKrystal != null && PointsInputKrystal != null)
            {
                this.Level = DensityInputKrystal.Level > PointsInputKrystal.Level ? DensityInputKrystal.Level : PointsInputKrystal.Level;
                this.Level++;
            }

            if(DensityInputKrystal != null && PointsInputKrystal != null)
            {
                this.Level = (DensityInputKrystal.Level > PointsInputKrystal.Level) ?
                    DensityInputKrystal.Level : PointsInputKrystal.Level;
                this.Level++;
            }
            Expand();

            Name = GetUniqueName(K.KrystalType.exp); 
        }

        #endregion

        #region public virtual
        protected override string GetUniqueName(K.KrystalType kType)
        {
            string root = GetNameRoot(); // domain.shape.

            char[] dot = new char[] { '.' };
            var components = ExpanderFilename.Split(dot);
            string expanderIndex = components[2];

            root += expanderIndex;  // domain.shape.expanderIndex
            string suffix = string.Format($".{kType}{K.KrystalFilenameSuffix}");
            string uniqueName = GetUniqueName(root, suffix);

            return uniqueName;
        }

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
        /// <summary>
        /// If a krystal having identical content exists in the krystals directory,
        /// the user is given the option to
        ///    either overwrite the existing krystal (using that krystal's index),
        ///    or abort the save.
        /// This means that a given set of ancestors should always have the same index.
        /// </summary>
        public override void Save()
        {
            var pathname = K.KrystalsFolder + @"\" + Name;
            DialogResult answer = DialogResult.Yes;
            if(File.Exists(pathname))
            {
                string msg = $"Expansion krystal {Name} already exists.\nSave it again with a new date?";
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

                MessageBox.Show($"{Name} saved.", "Saved", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show($"{Name} not saved.", "Save Aborted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        public void Expand()
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
        public List<StrandNode> StrandNodeList()
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
        public DensityInputKrystal DensityInputKrystal { get; set; }
        public PointsInputKrystal PointsInputKrystal { get; set; }
        public Expander Expander { get; set; }

        public string DensityInputFilename { get; set; }
        public string PointsInputFilename { get; set; }
        public string ExpanderFilename { get; set; }
    }
 }

