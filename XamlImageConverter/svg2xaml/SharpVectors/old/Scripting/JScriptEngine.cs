using System;

using System.Reflection;

using System.Text;

using Microsoft.JScript.Vsa;



//using System.Windows.Forms;



namespace SharpVectors.Scripting

{

	/// <summary>

	/// Summary description for Class1.

	/// </summary>

	public class JScriptEngine : VsaScriptEngine

	{
        #region Fields
        private static int counter = 0;

        #endregion



        #region Constructors

        public JScriptEngine(object scriptGlobal) : base(scriptGlobal)

        {

            engine = new Microsoft.JScript.Vsa.VsaEngine();
            engine.RootMoniker = "sharpvectors://jsengine/" + counter++;

            engine.Site = this;

            engine.InitNew();

            engine.RootNamespace = "SharpVectors.Scripting.Runtime";

            engine.GenerateDebugInfo = true;

            items = engine.Items;



            // add all assemblies from this AppDomain

            foreach ( Assembly asm in AppDomain.CurrentDomain.GetAssemblies() )

            {

                string  assemName = asm.Location;

				IJSVsaReferenceItem refItem = (IJSVsaReferenceItem)items.CreateItem(assemName, JSVsaItemType.Reference, JSVsaItemFlag.None);



                refItem.AssemblyName = assemName;

            }



            // Load the preamble code

            string preamble = @"

import System.Xml;

function alert(msg : String) : void { window.alert(msg); }

function printNode(node : XmlNode) : String { return window.PrintNode(node); }

";

			IJSVsaCodeItem codeItem = (IJSVsaCodeItem)items.CreateItem("Preamble", JSVsaItemType.Code, JSVsaItemFlag.None);

            codeItem.SourceText = preamble;

        }

        #endregion



        #region Public Methods

        public override void Execute(string code)

        {

            // Load the script code

			IJSVsaCodeItem codeItem = (IJSVsaCodeItem)items.CreateItem("Script", JSVsaItemType.Code, JSVsaItemFlag.None);

            codeItem.SourceText = code;



            // Add the global "window" item

			IJSVsaGlobalItem globalItem = (IJSVsaGlobalItem)items.CreateItem("window", JSVsaItemType.AppGlobal, JSVsaItemFlag.None);

            globalItem.TypeString = "SharpVectors.Dom.Svg.SvgWindow";



            // compile and run

            engine.Compile();

            engine.Run();

        }

        #endregion



        #region Private Methods

        private void showTypeInfo() 

        {

            if ( engine.IsRunning ) 

            {

                // show types

                System.Type[] types = engine.Assembly.GetTypes();

                StringBuilder typeNames = new StringBuilder();

                foreach ( Type type in types )

                {

                    typeNames.Append(type.FullName);

                    typeNames.Append("\n");

                    MethodInfo[] methods = type.GetMethods();

                    foreach ( MethodInfo method in methods ) 

                    {

                        typeNames.Append("    ");

                        typeNames.Append(method.Name);

                        typeNames.Append("(");

                        foreach ( ParameterInfo param in method.GetParameters() ) 

                        {

                            typeNames.Append(" ");

                            typeNames.Append(param.ParameterType.Name);

                        }

                        typeNames.Append(" )\n");

                    }

                }

                //MessageBox.Show(typeNames.ToString());

            }

        }

        #endregion

	}

}

