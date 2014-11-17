using System;
using System.Collections.Generic;
using System.Dynamic;

//using GHva3c.Properties;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using Newtonsoft.Json;
using GHva3c.Properties;


namespace GHva3c
{
    public class va3c_MeshBasicMaterial : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the va3c_MeshBasicMaterial class.
        /// </summary>
        public va3c_MeshBasicMaterial()
            : base("va3c_MeshBasicMaterial", "va3c_MeshBasicMaterial",
                "Creates a basic mesh material",
                "va3c", "materials")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("Color", "C", "Diffuse color of the material", GH_ParamAccess.item);
            pManager.AddNumberParameter("[Opacity]", "[O]", "Number in the range of 0.0 - 1.0 indicating how transparent the material is. A value of 0.0 indicates fully transparent, 1.0 is fully opaque.", GH_ParamAccess.item, 1.0);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Mesh Material", "Mm", "Mesh Material JSON representation.  Feed this into the Scene Compiler component.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //local varaibles
            GH_Colour inColor = null;
            Double inOpacity = 1;

            //catch inputs and populate local variables
            if (!DA.GetData(0, ref inColor)) { return; }
            if (inColor == null) { return; }
            DA.GetData(1, ref inOpacity);
            if (inOpacity > 1 || inOpacity < 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The opacity input must be between 0 and 1, and has been defaulted back to 1.  Check your 'O' input.");
                inOpacity = 1.0;
            }

            //set the output - build up a basic material json string
            DA.SetData(0, CreateMaterial(inColor, inOpacity));
        }

        private object CreateMaterial(GH_Colour inColor, double inOpacity)
        {
            dynamic jason = new ExpandoObject();
            jason.uuid = Guid.NewGuid();
            jason.type = "MeshBasicMaterial";
            jason.color = _Utilities.hexColor(inColor);
            jason.side = 2;
            if (inOpacity < 1)
            {
                jason.transparent = true;
                jason.opacity = inOpacity;
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
            get { return new Guid("{17b172a6-cdb8-4dc0-b708-c54cd319876b}"); }
        }
    }
}