using System;
using System.Collections.Generic;
using System.Dynamic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using Newtonsoft.Json;
using GHva3c.Properties;

namespace GHva3c
{
    public class va3c_LineBasicMaterial : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the va3c_LineBasicMaterial class.
        /// </summary>
        public va3c_LineBasicMaterial()
            : base("vA3C_LineBasicMaterial", "vA3C_LineBasicMaterial",
                "Creates a THREE.js Basic Line Material to use with line geometries",
                "vA3C", "materials")
        {
        }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.primary;
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("Color", "C", "Material Color", GH_ParamAccess.item);
            pManager.AddNumberParameter("LineWeight", "LW", "The thickness, in pixels, of the line material.  Not supported yet.", GH_ParamAccess.item, 1.0);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Line Material", "Lm", "Line Material JSON representation.  Feed this into the scene compiler component.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //loacl varaibles
            GH_Colour inColor = null;
            GH_Number inNumber = new GH_Number(1.0);

            //get user data
            if (!DA.GetData(0, ref inColor))
            {
                return;
            }
            DA.GetData(1, ref inNumber);

            //spin up a JSON material from the inputs
            string outJSON = ConstructMaterial(inColor, inNumber);

            //output
            DA.SetData(0, outJSON);
            

        }

        private string ConstructMaterial(GH_Colour inColor, GH_Number inNumber)
        {
            //json object to populate
            dynamic jason = new ExpandoObject();

            //JSON properties
            jason.uuid = Guid.NewGuid();
            jason.type = "LineBasicMaterial";
            jason.color = _Utilities.hexColor(inColor);
            jason.linewidth = inNumber.Value;
            jason.opacity = 1;


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
                return Resources.LINE_MAT;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{dc90f883-fc2f-4f0b-955b-b276fda72c70}"); }
        }
    }
}