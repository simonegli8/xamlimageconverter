using System;



namespace SharpVectors.Scripting

{

	/// <summary>

	/// The minimal interface a script engine must implement

	/// </summary>

	public abstract class ScriptEngine

	{

        #region Fields

        protected object scriptGlobal;

        #endregion



        #region Constructor

        protected ScriptEngine(object scriptGlobal)

        {

            this.scriptGlobal = scriptGlobal;

        }

        #endregion



        public abstract void Execute(string code);

	}

}

