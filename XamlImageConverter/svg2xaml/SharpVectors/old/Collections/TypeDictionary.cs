using System;
using System.Collections;
using System.Reflection;

namespace SharpVectors.Collections
{
	/// <summary>
	/// Summary description for TypeDictionary.
	/// </summary>
	public class TypeDictionary : Hashtable
	{
        #region Properties
        public Type this[string key] 
        {
            get { return (Type) base[key];  }
            set { base[key] = value; }
        }
        #endregion

        #region Public Methods
        public Object CreateInstance(string key, object[] args, BindingFlags flags)
        {
            Type type = this[key];

            return type.Assembly.CreateInstance(
                type.FullName, false, flags, null, args, null, new object[0]);
        }

        public Object CreateInstance(string key, object[] args)
        {
            return this.CreateInstance(key, args, BindingFlags.Default);
        }
        #endregion
	}
}
