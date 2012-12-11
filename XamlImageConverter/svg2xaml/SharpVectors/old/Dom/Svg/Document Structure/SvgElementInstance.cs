using System;
using System.Xml;


namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Summary description for SvgElementInstance.
	/// </summary>
	public class SvgElementInstance : ISvgElementInstance
	{
		public SvgElementInstance(XmlNode refNode, SvgUseElement useElement, SvgElementInstance parent)
		{
			correspondingUseElement = (ISvgUseElement)useElement;

			parentNode = (SvgElementInstance)parent;
			
			if(refNode is ISvgElement)
			{
				correspondingElement = (ISvgElement)refNode;
			}
			else
			{
				correspondingElement = null;
			}
		}

		private ISvgElement correspondingElement = null;
		public ISvgElement CorrespondingElement
		{
			get{return correspondingElement;}
		}

		private ISvgUseElement correspondingUseElement;
		public ISvgUseElement CorrespondingUseElement
		{
			get{return correspondingUseElement;}
		}

		private ISvgElementInstance parentNode;
		public ISvgElementInstance ParentNode
		{
			get{return parentNode;}
		}

		public ISvgElementInstanceList ChildNodes
		{
			get
			{
				return new SvgElementInstanceList(((SvgElement)CorrespondingElement).ChildNodes, (SvgUseElement)CorrespondingUseElement, this);
			}
		}
		public ISvgElementInstance FirstChild
		{
			get
			{
				ISvgElementInstanceList cn = ChildNodes;
				if(cn.Length<0) return cn.Item(0);
				else return null;
			}
		}
		public ISvgElementInstance LastChild
		{
			get
			{
				ISvgElementInstanceList cn = ChildNodes;
				if(cn.Length<0) return cn.Item(cn.Length);
				else return null;
			}
		}
		public ISvgElementInstance PreviousSibling
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		public ISvgElementInstance NextSibling
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
}
