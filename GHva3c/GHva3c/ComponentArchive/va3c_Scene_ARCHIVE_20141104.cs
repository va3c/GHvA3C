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
using System.IO;
using System.Text.RegularExpressions;
using GHva3c.Properties;


namespace GHva3c
{
    public class va3c_Scene_ARCHIVE_20141104 : GH_Component
    {
        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.hidden;
            }
        }

        /// <summary>
        /// Initializes a new instance of the va3c_Scene class.
        /// </summary>
        public va3c_Scene_ARCHIVE_20141104()
            : base("va3c_Scene", "va3c_Scene","va3c_Scene","va3c", "va3c")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("write?", "W?", "Write the va3c JSON file to disk?", GH_ParamAccess.item);
            pManager.AddTextParameter("filePath", "Fp", "Full filepath of the file you'd like to create.  Files will be overwritten automatically.", GH_ParamAccess.item); 
            pManager.AddTextParameter("Mesh Geo", "Me", "va3c geometry", GH_ParamAccess.list);
            pManager.AddTextParameter("Materials", "Mat", "va3c materials", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Message", "Out", "Message", GH_ParamAccess.item);
            pManager.AddTextParameter("Json Presentation of Scene", "J_Scene", "Json Presentation of Scene", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool write = false;
            string myFilePath = null;
            List<GH_String> inMeshGeometry = new List<GH_String>();
            List<GH_String> inMaterials = new List<GH_String>();

            //get user inputs
            if (!DA.GetData(0, ref write)) return;
            if (!DA.GetData(1, ref myFilePath)) return;
            if (!DA.GetDataList(2, inMeshGeometry)) return;
            if (!DA.GetDataList(3, inMaterials)) return;

            //if we are not told to run, return
            if (!write)
            {
                DA.SetData(0, "Set the 'W?' input to true to write the JSON file to disk.");
                return;
            }


            #region file path defense

            //check to see if the file path has any invalid characters
            try
            {
                //FIRST check to see if there is more than one semicolon in the path
                //or if there is a semiColon anywhere in there
                string[] colonFrags = myFilePath.Split(':');
                if (colonFrags.Length > 2 || myFilePath.Contains(";"))
                {
                    throw new Exception();
                }

                //SECOND test the file name for invalid characters using regular expressions
                //this method comes from the C# 4.0 in a nutshell book, p991
                string inputFileName = Path.GetFileName(myFilePath);
                char[] inValidChars = Path.GetInvalidFileNameChars();
                string inValidString = Regex.Escape(new string(inValidChars));
                string myNewValidFileName = Regex.Replace(inputFileName, "[" + inValidString + "]", "");

                //if the replace worked, throw an error at the user.
                if (inputFileName != myNewValidFileName)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                //warn the user
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Your file name is invalid - check your input and try again.");
                return;
            }


            //if neither the file or directory exist, throw a warning
            if (!File.Exists(myFilePath) && !Directory.Exists(Path.GetDirectoryName(myFilePath)))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "The directory you specified does not exist. Please double check your input. No file path will be set.");
                return;
            }

            //if the directory exists but the file type is not .xlsx, throw a warning and set pathString = noFIle
            if (Directory.Exists(Path.GetDirectoryName(myFilePath)) && !isJSONfile(Path.GetExtension(myFilePath)))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Please provide a file of type .js or .json.  Something like: 'myExampleFile.json'.");
                return;
            }
            #endregion



            //compile geometry + materials into one JSON object with metadata etc.
            //https://raw.githubusercontent.com/mrdoob/three.js/master/examples/obj/blenderscene/scene.js

            try
            {
                //create json from lists of json:
                string outJSON = sceneJSON(inMeshGeometry, inMaterials);
                outJSON = outJSON.Replace("OOO", "object");


                //write the file to disk
                File.WriteAllText(myFilePath, outJSON);

                //report success
                DA.SetData(0, "JSON file written successfully!");
                DA.SetData(1, outJSON);
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Something went wrong while trying to write the file to disk.  Here's the error:\n\n"
                    + e.ToString());
                return;
            }
        }

        private string sceneJSON(List<GH_String> geoList, List<GH_String> materialList)
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
            jason.materials = new object[materialList.Count];
            int matCounter = 0;
            Dictionary<string, object> UUIDdict = new Dictionary<string, object>();
            Dictionary<string, va3cAttributesCatcher> attrDict = new Dictionary<string, va3cAttributesCatcher>();
            foreach (GH_String m in geoList)
            {
                //get the last material if the list lengths don't match
                if (matCounter == materialList.Count)
                {
                    matCounter = materialList.Count - 1;
                }

                //deserialize everything
                va3cGeometryCatcher c = JsonConvert.DeserializeObject<va3cGeometryCatcher>(m.Value);
                va3cAttributesCatcher ac = JsonConvert.DeserializeObject<va3cAttributesCatcher>(m.Value);
                va3cMaterialCatcher mc = JsonConvert.DeserializeObject<va3cMaterialCatcher>(materialList[matCounter].Value);

                jason.geometries[meshCounter] = c;
                jason.materials[matCounter] = mc;

                //pull out an object from JSON and add to a local dict
                
                UUIDdict.Add(c.uuid, mc.uuid);
                attrDict.Add(c.uuid, ac);

                matCounter++;
                meshCounter++;


            }
       
            jason.OOO = new ExpandoObject();
            //create scene:
            jason.OOO.uuid = System.Guid.NewGuid();
            jason.OOO.type = "Scene";
            int[] numbers = new int[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            jason.OOO.matrix = numbers;
            jason.OOO.children = new object[geoList.Count];

            //create childern
            //loop over meshes
            int i = 0;
            foreach (var g in UUIDdict.Keys)
            {
                jason.OOO.children[i] = new ExpandoObject();
                jason.OOO.children[i].uuid = Guid.NewGuid();
                jason.OOO.children[i].name = "mesh" + i.ToString();
                jason.OOO.children[i].type = "Mesh";
                jason.OOO.children[i].geometry = g;
                jason.OOO.children[i].material = UUIDdict[g];
                jason.OOO.children[i].matrix = numbers;
                jason.OOO.children[i].userData = attrDict[g].userData;
                i++;
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
                return Resources.va3c_magenta;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{392e8cc6-8e8d-41e6-96ce-cc39f1a5f31c}"); }
        }

        private bool isJSONfile(string fileExtension)
        {
            if (fileExtension.ToLower() == ".js" ||
                fileExtension.ToLower() == ".json")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}