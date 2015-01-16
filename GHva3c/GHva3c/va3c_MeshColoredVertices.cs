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
    public class va3c_MeshColoredVertices : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the va3c_MeshColoredVertices class.
        /// </summary>
        public va3c_MeshColoredVertices()
            : base("vA3C_MeshColoredVertices", "vA3C_MeshColoredVertices",
                "Creates a vA3C mesh and a material from a grasshopper mesh with color data.",
                "vA3C", "geometry")
        {
        }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.tertiary;
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "A Grasshopper Mesh", GH_ParamAccess.item);
            pManager.AddTextParameter("Attribute Names", "[aN]", "Attribute Names", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager.AddTextParameter("Attribute Values", "[aV]", "Attribute Values", GH_ParamAccess.list);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Mesh JSON", "Mj", "Mesh JSON output to feed into scene compiler component", GH_ParamAccess.item);
            pManager.AddTextParameter("Mesh Material", "Mm", "Mesh Material JSON output to feed into scene compiler component.  Make sure to amtch this material with the corresponding mesh from Mj above.", GH_ParamAccess.list);
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

            //catch inputs and populate local variables
            if (!DA.GetData(0, ref mesh))
            {
                return;
            }
            if (mesh == null)
            {
                return;
            }
            DA.GetDataList(1, attributeNames);
            DA.GetDataList(2, attributeValues);
            if (attributeValues.Count != attributeNames.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Please provide equal numbers of attribute names and values.");
                return;
            }

            //populate dictionary
            int i = 0;
            foreach (var a in attributeNames)
            {
                attributesDict.Add(a.Value, attributeValues[i].Value);
                i++;
            }


            //create json from mesh
            string outJSON = _Utilities.geoJSON(mesh.Value, attributesDict);

            DA.SetData(0, outJSON);
            DA.SetData(1, MaterialWithVertexColors());
        }

        public string MaterialWithVertexColors()
        {
            dynamic JsonMat = new ExpandoObject();

            JsonMat.uuid = Guid.NewGuid();
            JsonMat.type = "MeshLambertMaterial";
            JsonMat.color = _Utilities.hexColor(new GH_Colour(System.Drawing.Color.White));
            JsonMat.side = 2;
            JsonMat.vertexColors = 1;
            return JsonConvert.SerializeObject(JsonMat);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Resources.MESH_VERTICES;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{0a8650b4-31ef-4d56-92f8-dcdd2f9bec8f}"); }
        }
    }
}