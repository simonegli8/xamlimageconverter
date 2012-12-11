using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

using SharpVectors.Collections;
using SharpVectors.Dom.Svg.Rendering;
using SharpVectors.Scripting;
using SharpVectors.Xml;
using System.Windows.Controls;
using System.Windows;

namespace SharpVectors.Dom.Svg
{
	public class SvgWindow : Canvas, ISvgWindow
	{
		#region Contructors
		public SvgWindow(double innerWidth, double innerHeight)
		{
			this.innerWidth = innerWidth;
			this.innerHeight = innerHeight;
		}

		public SvgWindow(double innerWidth, double innerHeight, SvgWindow parentWindow) : this(innerWidth, innerHeight)
		{
			this.parentWindow = parentWindow;
		}

		public SvgWindow(ISvgRenderer renderer)
		{
			this.renderer = renderer;
			this.renderer.Window = this;

            // setup default script engines
            // NOTE: I believe this is the only constructor where this is
            // needed since this is the only constructor involving a
            // potentially interactive SVG
            scriptEngineByMimeType = new TypeDictionary();

            SetMimeTypeEngineType("text/jscript.net", typeof(JScriptEngine));
		}
		#endregion
        
        #region Private fields
        //private Control control = null;
        private Hashtable referencedWindows = new Hashtable();
        private Hashtable referencedFiles = new Hashtable();
        private TypeDictionary scriptEngineByMimeType;
		#endregion

		#region Public properties
		private SvgWindow parentWindow = null;
		public SvgWindow ParentWindow
		{
			get
			{
				return parentWindow;
			}
		}

        private ISvgRenderer renderer;
        public ISvgRenderer Renderer
        {
            get { return renderer; }
            set { renderer = value; }
        }
		#endregion

		#region Public methods
        public void SetMimeTypeEngineType(string mimeType, Type engineType)
        {
            scriptEngineByMimeType[mimeType] = engineType;
        }

        /// <summary>
        /// Create and assign an empty SvgDocument to this window.  This is needed only in situations where the library user needs to create an SVG DOM tree outside of the usual LoadSvgDocument mechanism.
        /// </summary>
        public SvgDocument CreateEmptySvgDocument()
        {
            return document = new SvgDocument(this);
        }

        /*public void Render()
        {
            if ( this.document != null && renderer != null && InnerWidth > 0 && InnerHeight > 0 ) 
            {
                renderer.BeforeRender();
                document.Render(renderer);
                renderer.AfterRender();
            }
        }*/
        
        public void Alert(string message)
        {
            MessageBox.Show(message);
        }
        #endregion

        #region Private Methods
/*        private bool IsInWindowCache(Uri uri)
        {
            return referencedWindows.ContainsKey(GetAbsolutPath(uri));
        }

        private SvgWindow GetFromWindowCache(Uri uri)
        {
            return (SvgWindow)referencedWindows[GetAbsolutPath(uri)];
        }

        private void SaveToWindowCache(Uri uri, SvgWindow win)
        {
            referencedWindows[GetAbsolutPath(uri)] = win;
        }

        private string GetAbsolutPath(Uri uri)
        {
            return uri.GetLeftPart(UriPartial.Path);
        }*/

        /// <summary>
        /// Collect the text in all script elements, build engine and execute.  Note that this is very rough right now...just trying to get a simple case working - KL
        /// </summary>
        private void executeScripts()
        {
            Hashtable codeByMimeType = new Hashtable();
            StringBuilder codeBuilder;

            XmlNodeList scripts = document.GetElementsByTagName("script", SvgDocument.SvgNamespace);
            StringBuilder code = new StringBuilder();

            foreach ( XmlElement script in scripts )
            {
                string type = script.GetAttribute("type");

                if ( scriptEngineByMimeType.Contains(type) )
                {
                    // make sure we have a StringBuilder for this MIME type
                    if ( codeByMimeType.Contains(type) )
                        codeByMimeType[type] = new StringBuilder();

                    // grab this MIME type's codeBuilder
                    codeBuilder = (StringBuilder) codeByMimeType[type];

                    if ( script.HasChildNodes )
                    {
                        // process each child that is text node or a CDATA section
                        foreach ( XmlNode node in script.ChildNodes )
                        {
                            if ( node.NodeType == XmlNodeType.CDATA || node.NodeType == XmlNodeType.Text )
                            {
                                code.Append(node.Value);
                            }
                        }
                    }
                    else
                    {
                        // need to handle potential external reference here
                    }
                }
            }

            // execute code for all script engines
            foreach ( string mimeType in codeByMimeType.Keys )
            {
                codeBuilder = (StringBuilder) codeByMimeType[mimeType];

                if ( codeBuilder.Length > 0 )
                {
                    object[] args = new object[] { this };

                    ScriptEngine engine = (ScriptEngine) scriptEngineByMimeType.CreateInstance(mimeType, args);
                    engine.Execute( codeBuilder.ToString() );
                }
            }
        }

		#endregion

		#region Implementation of ISvgWindow
        private double innerWidth;
		public double InnerWidth
		{
			get
			{
                //if(control != null)
                //{
                //    return control.Width;
                //}
                //else
                //{
                //    return innerWidth;
                //}
                return this.DesiredSize.Width;
			}
		}

        private double innerHeight;
        public double InnerHeight
		{
			get
			{
                //if(control != null)
                //{
                //    return control.Height;
                //}
                //else
                //{
                //    return innerHeight;
                //}
                return this.DesiredSize.Height;
			}
		}

		public void ClearInterval(object interval)
		{
	
		}

		public void ClearTimeout(object timeout)
		{
	
		}

		public XmlDocumentFragment ParseXML(string source, XmlDocument document)
		{
			XmlDocumentFragment frag = document.CreateDocumentFragment();
			frag.InnerXml = source;
			return frag;
		}

		public string PrintNode(System.Xml.XmlNode node)
		{
			return node.OuterXml;
		}

		public object SetInterval(string code, double delay)
		{
			return null;
		}

		public object SetTimeout(string code, double delay)
		{
			return null;
		}

		public SharpVectors.Dom.Stylesheets.IStyleSheet DefaultStyleSheet
		{
			get
			{
				return null;
			}
		}

		private SvgDocument document;
		public ISvgDocument Document
		{
			get
			{
				return document;
			}
			set
			{
				document = (SvgDocument)value;
			}
		}

		public string Src
		{
			get
			{
				return (document != null) ? document.Url : String.Empty;
			}
			set
			{
				Uri uri = new Uri(new Uri(AppDomain.CurrentDomain.SetupInformation.ApplicationBase), value);

				document = new SvgDocument(this);
				document.Load(uri.AbsoluteUri);
				
                // Execute all script elements
                //executeScripts();
			}
		}
		#endregion
	}
}
