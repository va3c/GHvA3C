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
    public class va3c_MeshColoredFaces : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the va3c_coloredMesh class.
        /// </summary>
        public va3c_MeshColoredFaces()
            : base("va3c_MeshColoredFaces", "va3c_MeshColoredFaces",
                "Creates a va3c mesh and a set of materials from a grasshopper mesh and a list of colors - one color per face.  If the colors list isn't the same length as the list of faces, we'll do standard grasshopper longest list iteration, using the mesh faces as the driving list.",
                "va3c", "geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "A Grasshopper Mesh", GH_ParamAccess.item);
            pManager.AddColourParameter("colors", "C", "A list of colors - one per face.", GH_ParamAccess.list);
            pManager.AddTextParameter("Attribute Names", "[aN]", "Attribute Names", GH_ParamAccess.list);
            pManager[2].Optional = true;
            pManager.AddTextParameter("Attribute Values", "[aV]", "Attribute Values", GH_ParamAccess.list);
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Mesh JSON", "Mj", "Mesh JSON output to feed into scene compiler component", GH_ParamAccess.item);
            pManager.AddTextParameter("Mesh Material JSON", "Mm", "Mesh Material JSON output to feed into scene compiler component.  Make sure to amtch this material with the corresponding mesh from Mj above.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //local varaibles
            GH_Mesh mesh = null;
            List<GH_Colour> colors = new List<GH_Colour>();
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
            if (!DA.GetDataList(1, colors))
            {
                return;
            }
            DA.GetDataList(2, attributeNames);
            DA.GetDataList(3, attributeValues);
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
            string meshJSON = _Utilities.geoJSON(mesh.Value, attributesDict);
            //create MeshFaceMaterial
            string meshMaterailJSON = makeMeshFaceMaterialJSON(mesh.Value, colors);

            DA.SetData(0, meshJSON);
            DA.SetData(1, meshMaterailJSON);
        }

        private string makeMeshFaceMaterialJSON(Mesh mesh, List<GH_Colour> colors)
        {
            //JSON object to populate
            dynamic jason = new ExpandoObject();
            jason.uuid = Guid.NewGuid();
            jason.type = "MeshFaceMaterial"; 
            //jason.side = 2;
            //jason.transparent = false;
            //jason.wireframe = false;
            //jason.color = _Utilities.hexColor(new GH_Colour(System.Drawing.Color.White));

            //now we need an array of materials, one for each face of the mesh.
            var quads = from q in mesh.Faces
                        where q.IsQuad
                        select q;
            jason.materials = new object[mesh.Faces.Count + quads.Count()];

            //we'll loop over the mesh to make sure that each quad is assigned two materials
            //since it is really two triangles as a three.js mesh .  If there are fewer colors than mesh faces, we'll take the last one
            int faceCounter = 0;
            int matCounter = 0;
            foreach (var f in mesh.Faces)
            {
                //make sure there is an item at this index.  if not, grab the last one
                if (matCounter == mesh.Faces.Count)
                {
                    matCounter = mesh.Faces.Count = 1;
                }

                //add the color[s] to the array.  one for a tri, two for a quad
                if (f.IsTriangle)
                {
                    //set up our basic material
                    dynamic matthew = new ExpandoObject();
                    matthew.uuid = Guid.NewGuid();
                    matthew.type = "MeshBasicMaterial";
                    matthew.color = _Utilities.hexColor(colors[matCounter]);
                    matthew.side = 2;

                    jason.materials[faceCounter] = matthew;
                    faceCounter++;
                }
                if (f.IsQuad)
                {
                    //set up our basic material
                    dynamic matthew = new ExpandoObject();
                    matthew.uuid = Guid.NewGuid();
                    matthew.type = "MeshBasicMaterial";
                    matthew.color = _Utilities.hexColor(colors[matCounter]);
                    matthew.side = 2;
                    jason.materials[faceCounter] = matthew;
                    faceCounter++;

                    dynamic ana = new ExpandoObject();
                    ana.uuid = Guid.NewGuid();
                    ana.type = "MeshBasicMaterial";
                    ana.color = _Utilities.hexColor(colors[matCounter]);
                    ana.side = 2;
                    ana.uuid = Guid.NewGuid();
                    jason.materials[faceCounter] = ana;
                    faceCounter++;
                } 

                matCounter++;
            }

            return JsonConvert.SerializeObject(jason);
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{815a9851-bf5d-44d8-87a7-0915bc58a628}"); }
        }
    }
}