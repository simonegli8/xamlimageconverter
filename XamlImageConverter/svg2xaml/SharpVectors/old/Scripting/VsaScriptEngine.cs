using System;

using Microsoft.JScript.Vsa;



namespace SharpVectors.Scripting

{

	/// <summary>

	/// Summary description for VsaScriptEngine.

	/// </summary>

	public abstract class VsaScriptEngine : ScriptEngine, IJSVsaSite

	{

        #region Fields

		protected IJSVsaEngine engine;

		protected IJSVsaItems items;

        #endregion



        #region Constructor

		protected VsaScriptEngine(object scriptGlobal) : base(scriptGlobal) {}

        #endregion



		#region VsaSite interface

		void IJSVsaSite.GetCompiledState(out Byte[] pe, out Byte[] debugInfo)

        {

            pe = debugInfo = null;

        }



		object IJSVsaSite.GetEventSourceInstance(string itemName, string eventSourceName)

        {

            return this.scriptGlobal;

        }



		object IJSVsaSite.GetGlobalInstance(string name)

        {

            object result;



            switch (name) 

            {

                case "window":

                    result = this.scriptGlobal;

                    break;

                default:

                    Console.WriteLine("GlobalInstance not found: " + name);

                    result = null;

                    break;

            }



            return result;

        }



		void IJSVsaSite.Notify(string notify, object info)

        {

        }



		bool IJSVsaSite.OnCompilerError(IJSVsaError e)

        {

            Console.WriteLine(

                String.Format("Error of severity {0} on line {1}: {2}", e.Severity, e.Line, e.Description));



            // Continue to report errors

            return true;

        }

        #endregion

	}

}

