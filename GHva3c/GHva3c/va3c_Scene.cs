using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Timers;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using Newtonsoft.Json;


namespace GHva3c
{
    public class va3c_Scene : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the va3c_Scene class.
        /// </summary>
        public va3c_Scene()
            : base("va3c_Scene", "va3c_Scene","va3c_Scene","va3c", "va3c")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("MeshGeo", "Mesh", "va3c geometry", GH_ParamAccess.list);
            pManager.AddTextParameter("Materials", "Mat", "va3c materials", GH_ParamAccess.list);
            //pManager.AddTextParameter("[Lights]", "[L]", "va3c light sources", GH_ParamAccess.list);
            //pManager.AddTextParameter("[Cameras]", "[C]", "va3c cameras", GH_ParamAccess.list);
            //pManager[2].Optional = true;
            //pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Message", "Out", "Message", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

             List<string> inMeshGeometry = new List<string>();
             List<string> inMaterials = new List<string>();

            //get user inputs
            if (!DA.GetDataList(0, inMeshGeometry)) return;
            if (!DA.GetDataList(1, inMaterials)) return;

            //compile geometry + materials into one object with metadata etc.
            //https://raw.githubusercontent.com/mrdoob/three.js/master/examples/obj/blenderscene/scene.js

            //create json from lists of json:
            string outJSON = sceneJSON(inMeshGeometry, inMaterials);

            DA.SetData(0, outJSON);

        }

        private string sceneJSON(List<string> geoList, List<string> materialList)
        {
            //create a dynamic object to populate
            dynamic jason = new ExpandoObject();

            //populate metadata object
            jason.metadata = new ExpandoObject();
            jason.metadata.version = 4.3;
            jason.metadata.type = "Object";
            jason.metadata.generator = "ObjectExporter";

            //populate mesh geometries:
            jason.geometries = new object[geoList.Count];   //array for geometry
            int meshCounter = 0;
            Dictionary<string, object> geos = new Dictionary<string, object>();
            foreach (string m in geoList)
            {
                jason.geometries[meshCounter++] = m;
                
                //pull out an object from JSON and add to a local dict
                va3cGeometryCatcher c = JsonConvert.DeserializeObject<va3cGeometryCatcher>(m);
                geos.Add(c.uuid, c);

            }
            
            //populate materials:
            jason.materials = new object[materialList.Count];
            int matCounter = 0;
            foreach (string m in materialList)
            {
                jason.materials[matCounter++] = m;
            }

       
            jason["object"] = new ExpandoObject();
            //create scene:
            jason["object"].uuid = System.Guid.NewGuid();
            jason["object"].type = "Scene";
            int[] numbers = new int[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            jason["object"].matrix = numbers;
            //jason["object"].children 

            //create childern
            //loop over meshes
            



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
            get { return new Guid("{392e8cc6-8e8d-41e6-96ce-cc39f1a5f31c}"); }
        }
    }

    public class va3cGeometryCatcher
    {
        public string uuid;
        public string type;
        public object data;
    }
}