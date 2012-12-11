using System;
using System.Xml;
using System.Windows.Media;

using SharpVectors.Dom.Svg;



namespace SharpVectors.Dom.Svg.Rendering
{
	/// <summary>
	/// Defines the interface required by a renderer to render the SVG DOM.
	/// </summary>
    /// <developer>kevin@kevlindev.com</developer>
    /// <completed>0</completed>
	public interface ISvgRenderer
	{
        SvgWindow Window { get; set; }
		RenderingNode GetRenderingNode(XmlElement node);

        ISvgRenderer GetRenderer();
        //Bitmap Render(SvgElement node);
        //Bitmap Render(SvgDocument node);
        //void BeforeRender();
        //void AfterRender();
	}

}

