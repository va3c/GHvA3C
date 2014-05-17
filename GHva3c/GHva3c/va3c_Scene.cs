using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHva3c
{
    public class va3c_Scene : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the va3c_Scene class.
        /// </summary>
        public va3c_Scene()
            : base("va3c_Scene", "va3c_Scene",
                "va3c_Scene",
                "Category", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Geometry", "G", "va3c geometry", GH_ParamAccess.list);
            pManager.AddTextParameter("Materials", "M", "va3c materials", GH_ParamAccess.list);
            pManager.AddTextParameter("[Lights]", "[L]", "va3c light sources", GH_ParamAccess.list);
            pManager.AddTextParameter("[Cameras]", "[C]", "va3c cameras", GH_ParamAccess.list);
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

            //variables

             List<string> inGeometry = new List<string>();
             List<string> inMaterials = new List<string>();

            //get user inputs
            if (!DA.GetData(0, ref inGeometry)) return;
            if (!DA.GetData(1, ref inMaterials)) return;

            //compile geometry + materials into one object with metadata etc.
            //https://raw.githubusercontent.com/mrdoob/three.js/master/examples/obj/blenderscene/scene.js



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