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
            : base("va3c_Mesh", "va3c_Mesh",
                "Creates a va3c mesh from a grasshopper mesh.",
                "va3c", "geometry")
        {
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
            pManager.AddTextParameter("Mesh JSON", "Me", "Mesh JSON output to feed into scene compiler component", GH_ParamAccess.item);
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
            int i=0;
            foreach (var a in attributeNames)
            {
                attributesDict.Add(a.Value, attributeValues[i].Value);
                i++;
            }

            //create json from mesh
            string outJSON = geoJSON(mesh.Value, attributesDict);

            DA.SetData(0, outJSON);
            
        }


        private string geoJSON(Mesh mesh, Dictionary<string, object> attDict)
        {
            //create a dynamic object to populate
            dynamic jason = new ExpandoObject();


            jason.uuid = Guid.NewGuid();
            jason.type = "Geometry";
            jason.data = new ExpandoObject();
            jason.userData = new ExpandoObject();

            //populate data object properties
            jason.data.vertices = new object[mesh.Vertices.Count * 3];
            jason.data.normals = new object[0];
            jason.data.uvs = new object[0];
            jason.data.faces = new object[mesh.Faces.Count * 4];
            jason.data.scale = 1;
            jason.data.visible = true;
            jason.data.castShadow = true;
            jason.data.receiveShadow = false;
            jason.data.doubleSided = true;

            //populate vertices
            int counter = 0;
            int i = 0;
            foreach (var v in mesh.Vertices)
            {
                jason.data.vertices[counter++] = mesh.Vertices[i].X * -1.0;
                jason.data.vertices[counter++] = mesh.Vertices[i].Z;
                jason.data.vertices[counter++] = mesh.Vertices[i].Y;
                i++;
            }

            //populate faces
            counter = 0;
            i = 0;
            foreach (var f in mesh.Faces)
            {
                jason.data.faces[counter++] = 0;
                jason.data.faces[counter++] = mesh.Faces[i].A;
                jason.data.faces[counter++] = mesh.Faces[i].B;
                jason.data.faces[counter++] = mesh.Faces[i].C;
                i++;
            }


            //populate userData objects
            var attributeCollection = (ICollection<KeyValuePair<string, object>>)jason.userData;
            foreach (var kvp in attDict)
            {
                attributeCollection.Add(kvp);
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
                return Resources.va3c_cyan;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{981ed24e-945b-44b9-988e-4f17381c3072}"); }
        }
    }
}