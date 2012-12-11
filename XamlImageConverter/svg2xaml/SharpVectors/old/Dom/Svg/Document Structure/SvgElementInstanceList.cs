using System;
using System.Collections;
using System.Xml;


namespace SharpVectors.Dom.Svg
{
	public class SvgElementInstanceList : ISvgElementInstanceList
	{
		#region Constructors
		public SvgElementInstanceList(XmlNodeList xmlChildNodes, SvgUseElement useElement, SvgElementInstance parent)
		{
			items = new SvgElementInstance[xmlChildNodes.Count];
			int counter = 0;
			foreach(XmlNode xmlChild in xmlChildNodes)
			{
				items.SetValue(new SvgElementInstance(xmlChild, useElement, parent), counter);
				counter++;
			}
		}
		#endregion

		#region Private fields
		private SvgElementInstance[] items;
		#endregion

		#region Implementation of ISvgElementInstanceList
		public double Length
		{
			get
			{
				return (double)items.GetLength(0);
			}
		}

		public ISvgElementInstance Item(double index)
		{
			if(index<Length)
			{
				return (ISvgElementInstance)items.GetValue((int) index);
			}
			else return null;
		}
		#endregion
	}
}
