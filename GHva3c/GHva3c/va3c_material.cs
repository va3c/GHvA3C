using System;
using System.Collections.Generic;
using System.Dynamic;

//using GHva3c.Properties;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Newtonsoft.Json;


namespace GHva3c
{
    public class va3c_material : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the va3c_material class.
        /// </summary>
        public va3c_material()
            : base("CreateMaterial", "CreateMaterial", "CreateMaterial", "va3c", "va3c")
        {
        }

        
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("Color", "Color", "Material Color", GH_ParamAccess.item);
            pManager.AddNumberParameter("[Opacity]", "[Opacity]", "Material Opacity", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddTextParameter("[Name]", "[Name]", "Material Name", GH_ParamAccess.item);
            pManager[2].Optional = true;
        }


        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Material", "Material", "Geometry Matherial", GH_ParamAccess.item);
            pManager.Register_StringParam("Material Names", "Material Names", "Material Name", GH_ParamAccess.item);
        }


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            System.Drawing.Color inColor = System.Drawing.Color.White;
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
            DA.SetData(1, outName);
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:

                //return  Resources.MatIcon;
                return null;
            }
        }


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{afb20e86-13f1-4083-89e4-282299763f4c}"); }
        }


        public string ConstructMaterial(System.Drawing.Color Col, Double Opp, String Name)
        {
            dynamic JsonMat = new ExpandoObject();
            //JsonMat.metadata = new ExpandoObject();
            //JsonMat.metadata.version = 4.2;
            //JsonMat.metadata.type = "material";
            //JsonMat.metadata.generator = "MaterialExporter";

            JsonMat.uuid = Guid.NewGuid();
            JsonMat.type = "MeshPhongMaterial";
            JsonMat.color = Col.Name;
            JsonMat.ambient = Col.Name;
            JsonMat.emissive = Col.Name;
            JsonMat.specular = Col.Name;
            JsonMat.shininess = 50;
            JsonMat.opacity = Opp;
            JsonMat.transparent = false;
            JsonMat.wireframe = false;
            JsonMat.side = "THREE.DoubleSide";
            return JsonConvert.SerializeObject(JsonMat);
        }
    }
}