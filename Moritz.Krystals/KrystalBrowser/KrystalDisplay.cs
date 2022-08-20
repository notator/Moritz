using Krystals4ObjectLibrary;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Moritz.Krystals
{
    public partial class KrystalDisplay : Form
    {
        public KrystalDisplay(Krystal krystal)
        {
            InitializeComponent();

            int kLevel = (int) krystal.Level;
            List<List<string>> allBlocks = new List<List<string>>();
            List<string> block = new List<string>(); // a block consists of a single list of consecutive strands (usually 7 or less)
            allBlocks.Add(block);
            foreach(var strand in krystal.Strands)
            {                
                if(strand.Level > 1 && strand.Level < kLevel)
                {
                    allBlocks.Add(block);
                    block = new List<string>(); 
                }
                block.Add(K.GetStringOfUnsignedInts(strand.Values));
            }
        }
    }
}
