/*  HEADER
 * 
 *  Custom GH Parameter : Element
 *  This register the custom object as a GH parameter
 *  //http://www.grasshopper3d.com/forum/topics/custom-data-and-parameter-no-implicit-reference-conversion
 * 
 *  03/03/15 
 *  Ana Garcia Puyol
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace GHva3c
{
    class LayerParameter:GH_Goo<Layer>
    {
        public override IGH_Goo Duplicate()
        {
            LayerParameter layer = new LayerParameter();
            layer.Value = Value;
            return layer;
        }

        public override string TypeDescription
        {
            get { return "Layer: A high level vA3C class to inherit from"; }
        }

        public override string TypeName
        {
            get { return "Layer"; }
        }

        public override bool IsValid
        {
            get { return Value.ID != null; }
        }

        public override string ToString()
        {
            return Value.ID;
        }
    }
}
