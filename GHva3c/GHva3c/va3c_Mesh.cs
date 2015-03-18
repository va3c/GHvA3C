using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Timers;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using GHva3c.Properties;


using Newtonsoft.Json;

namespace GHva3c
{
    public class va3c_Mesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the va3c_geometry class.
        /// </summary>
        public va3c_Mesh()
            : base("vA3C_Mesh", "vA3C_Mesh",
                "Creates a vA3C mesh from a grasshopper mesh.",
                "vA3C", "geometry")
        { }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.secondary;
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "A Grasshopper Mesh", GH_ParamAccess.item);
            pManager.AddGenericParameter("Mesh Material", "Mm", "Mesh Material", GH_ParamAccess.item);
            pManager.AddTextParameter("Attribute Names", "[aN]", "Attribute Names", GH_ParamAccess.list);
            pManager[2].Optional = true;
            pManager.AddTextParameter("Attribute Values", "[aV]", "Attribute Values", GH_ParamAccess.list);
            pManager[3].Optional = true;
            pManager.AddTextParameter("Layer", "[L]", "Layer", GH_ParamAccess.item);
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddTextParameter("Mesh JSON", "Mj", "Mesh JSON output to feed into scene compiler component", GH_ParamAccess.item);

            pManager.AddGenericParameter("Mesh Element", "Me", "Mesh element output to feed into scene compiler component", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //local varaibles
            GH_Mesh mesh = null;
            List<GH_String> attributeNames = new List<GH_String>();
            List<GH_String> attributeValues = new List<GH_String>();
            Dictionary<string, object> attributesDict = new Dictionary<string, object>();
            Material material = null;
            string layerName = "Default";


            //catch inputs and populate local variables
            if (!DA.GetData(0, ref mesh))
            {
                return;
            }
            if (mesh == null)
            {
                return;
            }

            //Make sure the material is of mType Mesh
            if (!DA.GetData(1, ref material))
            {
                return;
            }
            if (material == null)
            {
                return;
            }

            if (material.Type != va3cMaterialType.Mesh)
            {
                throw new Exception("Please use a MESH Material");
            }

            DA.GetDataList(2, attributeNames);
            DA.GetDataList(3, attributeValues);
            if (attributeValues.Count != attributeNames.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Please provide equal numbers of attribute names and values.");
                return;
            }

            Layer layer = null;
            DA.GetData(4, ref layerName);

            layer = new Layer(layerName);


            //populate dictionary
            int i = 0;
            foreach (var a in attributeNames)
            {
                attributesDict.Add(a.Value, attributeValues[i].Value);
                i++;
            }
            
            //add the layer name to the attributes dictionary
            attributesDict.Add("layer", layerName);

            //create json from mesh
            string outJSON = _Utilities.geoJSON(mesh.Value, attributesDict);

            Element e = new Element(outJSON, va3cElementType.Mesh, material, layer);

            DA.SetData(0, e);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.MESH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{f56327e7-379b-4ebd-8266-4c51e2ae2e74}"); }
        }
    }
}