using System;
using System.Xml;

using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;
using System.Windows.Media;

namespace SharpVectors.Renderer.Xaml
{
	/// <summary>
	/// Summary description for SvgImageGraphicsNode.
	/// </summary>
	public class SvgUseElementGraphicsNode : GraphicsNode
	{
        #region Constructor
		public SvgUseElementGraphicsNode(SvgElement element) : base(element)
		{
		}
        #endregion

        #region Public Methods
		public override void Render(ISvgRenderer renderer)
		{
            if (this.Element.GetAttribute("display") == "none")
                return;

			XmlNode newNode = ((SvgUseElement) element).GetReplacedElement();
			XmlNode parent = element.ParentNode;
            XmlNode replacedNode = parent.ReplaceChild(newNode, element);

            ((SvgElement)newNode).Render(renderer);

			parent.ReplaceChild(replacedNode, newNode);
		}
        #endregion;
	}
}
