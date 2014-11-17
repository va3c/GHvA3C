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
    public class va3c_MeshPhongMaterial : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the va3c_MeshPhongMaterial class.
        /// </summary>
        public va3c_MeshPhongMaterial()
            : base("va3c_MeshPhongMaterial", "va3c_MeshPhongMaterial",
                "Create a fancy material for meshes",
                "va3c", "materials")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("Color", "C", "Material Color", GH_ParamAccess.item);
            pManager.AddNumberParameter("[Opacity]", "[O]", "Material Opacity", GH_ParamAccess.item);
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
            //System.Drawing.Color inColor = System.Drawing.Color.White;
            GH_Colour inColor = null;
            Double inOpacity = 1;
            String inName = String.Empty;
            String outMaterial = null;
            String outName = null;

            //if (!DA.GetData(0, ref inColor)) { return; }
            if (!DA.GetData(0, ref inColor)) { return; }
            if (inColor == null) { return; }
            DA.GetData(1, ref inOpacity);
            DA.GetData(2, ref inName);

            if (inName == string.Empty) { inName = DateTime.Now.ToShortDateString(); }      //autogenerate name
            outName = inName;
            outMaterial = ConstructMaterial(inColor, inOpacity, inName);
            //call json conversion function

            DA.SetData(0, outMaterial);
        }

        private string ConstructMaterial(GH_Colour Col, Double Opp, String Name)
        {
            dynamic jason = new ExpandoObject();

            jason.uuid = Guid.NewGuid();
            jason.type = "MeshPhongMaterial";
            jason.color = _Utilities.hexColor(Col);
            jason.ambient = _Utilities.hexColor(Col);
            jason.emissive = _Utilities.hexColor(new GH_Colour(System.Drawing.Color.Black));
            jason.specular = _Utilities.hexColor(new GH_Colour(System.Drawing.Color.Gray));
            jason.shininess = 50;
            jason.opacity = Opp;
            jason.transparent = false;
            jason.wireframe = false;
            jason.side = 2;
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
            get { return new Guid("{9e566ce6-dc3f-4606-b895-48b90e0caf72}"); }
        }
    }
}