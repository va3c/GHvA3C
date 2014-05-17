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
            pManager.AddTextParameter("Geometry", "G", "va3c geometry", GH_ParamAccess.list);
            pManager.AddTextParameter("Materials", "M", "va3c materials", GH_ParamAccess.list);
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

             List<string> inGeometry = new List<string>();
             List<string> inMaterials = new List<string>();

            //get user inputs
            if (!DA.GetDataList(0, inGeometry)) return;
            if (!DA.GetDataList(1, inMaterials)) return;

            //compile geometry + materials into one object with metadata etc.
            //https://raw.githubusercontent.com/mrdoob/three.js/master/examples/obj/blenderscene/scene.js

            //create json from lists of json:
            string outJSON = sceneJSON(inGeometry, inMaterials);

            DA.SetData(0, outJSON);

        }

        private string sceneJSON(List<string> geoList, List<string> materialList)
        {
            //create a dynamic object to populate
            dynamic jason = new ExpandoObject();

            jason.urlBaseType = "relativeToScene";

            jason.objects = new ExpandoObject();
            int objectCounter = 0;
            foreach (string s in geoList)
            {
                string objectName = string.Format("object.{0}", objectCounter.ToString());
                jason.objects.objectName = new ExpandoObject();   //need new expandoObject?
                jason.objects.objectName = s;   //need new expandoObject?
                objectCounter++;
            }

            jason.materials = new ExpandoObject();

            foreach (string s in materialList)
            {
                //get material name:
                string[] materialName  = s.Split(' ');
                string matName = materialName[0];
                jason.materials.matName = s;
            }


            //populate metadata object
            jason.metadata = new ExpandoObject();
            jason.metadata.formatVersion = 3.2;
            jason.metadata.type = "scene";
            jason.metadata.sourceFile = "scene.blend";
            jason.metadata.generatedBy = "GHva3c 0.01 Exporter";
            jason.metadata.objects = geoList.Count;
            jason.metadata.geometries = geoList.Count;
            jason.metadata.materials = materialList.Count;
            jason.metadata.textures = 0;

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
}