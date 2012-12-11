using System;
using System.Collections;
using System.Xml;
using System.IO;
using SharpVectors.Dom.Css;
using SharpVectors.Dom.Events;

using SharpVectors.Dom.Svg.Rendering;
using System.Windows.Media;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Summary description for SvgElement.
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <developer>kevin@kevlindev.com</developer>
	/// <completed>75</completed>
	public class SvgElement : CssXmlElement, ISvgElement
	{

		#region Private Fields
		private SvgDocument ownerDocument;
        private RenderingNode renderingNode;
		#endregion

		#region Constructors
		protected internal SvgElement(string prefix, string localname, string ns, SvgDocument doc) : base(prefix, localname, ns, doc) 
		{
			ownerDocument = doc;

            // create associated rendering node if associated window has a renderer
            ISvgRenderer renderer = ((SvgWindow) doc.Window).Renderer;
            if ( renderer != null )
                renderingNode = renderer.GetRenderingNode(this);

            
		}

		#endregion

		#region Rendering 
		public RenderingNode RenderingNode
		{
			get{return renderingNode;}
		}

        public virtual void Render(ISvgRenderer renderer)
        {
            if ( !(this is ISharpDoNotPaint) && renderingNode != null ) 
            {
                renderingNode.BeforeRender(renderer);
                renderingNode.Render(renderer);
                RenderChildren(renderer);
                renderingNode.AfterRender(renderer);
            }
        }

        public virtual void RenderChildren(ISvgRenderer renderer)
        {
            if (RenderingNode == null || RenderingNode.CanRenderChildren(renderer))
            {
                foreach (XmlNode node in ChildNodes)
                {
                    SvgElement element = node as SvgElement;
                    if (element != null)
                    {
                        element.Render(renderer);
                    }
                }
            }
        }

		#endregion
		
		#region Public members
		public new SvgDocument OwnerDocument
		{
			get
			{
				return ownerDocument;
			}
		}

		public string Id
		{
			get
			{
				return this.GetAttribute("id");
			}
			set
			{
				this.SetAttribute("id",value);
			}
		}

		/*public string Xmlbase
		{
			get
			{
				return this.BaseURI;
			}
			set
			{
				throw new NotSupportedException();
			}
		}*/

		/*public ISvgSvgElement OwnerSvgElement
		{
			get
			{
				return ownerSvgElement;
			}
		}*/

		public string XmlSpace
		{
			get
			{
				string s = this.GetAttribute("xml:space");
				if(s.Length == 0)
				{
					if(ParentNode is SvgElement){
						SvgElement par = (SvgElement) ParentNode;
						s = par.XmlSpace;
					}
					else s = "default";
				}
				
				return s;
			}
		}

		public string XmlLang
		{
			get
			{
				string s = this.GetAttribute("xml:lang");
				if(s.Length == 0)
				{
					if(ParentNode is SvgElement)
					{
						SvgElement par = (SvgElement) ParentNode;
						s = par.XmlLang;
					}
					else s = String.Empty;
				}
				
				return s;
			}
		}

		public ISvgSvgElement OwnerSvgElement{
			get
			{
				//return (SvgSvgElement)this.SelectSingleNode("ancestor::svg:svg", this.OwnerDocument.namespaceManager);
				if(this.Equals(this.OwnerDocument.DocumentElement))
				{
					return null;
				}
				else
				{
					XmlNode parent = this.ParentNode;
					while(parent != null)
					{
						if(parent is SvgSvgElement) return (SvgSvgElement) parent;
						parent = parent.ParentNode;
					}
					return null;
				}
			}
		}

		public ISvgElement ViewportElement
		{
			get
			{
				//return (SvgElement) this.SelectSingleNode("/svg:*[@viewport]", this.OwnerDocument.namespaceManager);
				XmlNode parent = ParentNode;
				while(parent != null)
				{
					if(parent is ISvgSvgElement) 
					{
						return (SvgElement)parent;
					}
					parent = parent.ParentNode;
				}
				return null;
			}
		}

		#endregion

		public Uri ResolveUri(string uri)
		{
			string baseUri = BaseURI;
			if(baseUri.Length == 0)
			{
				baseUri = "file://" + new DirectoryInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase).Parent.FullName.Replace('\\', '/');
            }

			return new Uri(new Uri(baseUri), uri);
		}
	}
}
