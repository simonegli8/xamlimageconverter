using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Media;

using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;

using SharpVectors.Collections;
using SharpVectors.Dom.Css;
using SharpVectors.Xml;

using SharpVectors.Dom.Svg.Rendering;

namespace SharpVectors.Dom.Svg
{		
	/// <summary>
	///  When an 'svg'  element is embedded inline as a component of a document from another namespace, such as when an 'svg' element is embedded inline within an XHTML document [XHTML], then an SVGDocument object will not exist; instead, the root object in the document object hierarchy will be a Document object of a different type, such as an HTMLDocument object.
	/// However, an SVGDocument object will indeed exist when the root element of the XML document hierarchy is an 'svg' element, such as when viewing a stand-alone SVG file (i.e., a file with MIME type "image/svg+xml"). In this case, the SVGDocument object will be the root object of the document object model hierarchy.
	/// In the case where an SVG document is embedded by reference, such as when an XHTML document has an 'object' element whose href attribute references an SVG document (i.e., a document whose MIME type is "image/svg+xml" and whose root element is thus an 'svg' element), there will exist two distinct DOM hierarchies. The first DOM hierarchy will be for the referencing document (e.g., an XHTML document). The second DOM hierarchy will be for the referenced SVG document. In this second DOM hierarchy, the root object of the document object model hierarchy is an SVGDocument object.
	/// The SVGDocument interface contains a similar list of attributes and methods to the HTMLDocument interface described in the Document Object Model (HTML) Level 1 chapter of the [DOM1] specification. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>60</completed>
	public class SvgDocument : CssXmlDocument, ISvgDocument
	{
        #region Fields
        private SvgWindow _window;
        private TypeDictionary nodeByTagName;
        public XmlNamespaceManager NamespaceManager;
        #endregion

		#region Constructors
		public SvgDocument(SvgWindow window)
		{
			// Setup namespace manager and add default namespaces
			NamespaceManager = new XmlNamespaceManager(this.NameTable);
			NamespaceManager.AddNamespace(String.Empty, SvgDocument.SvgNamespace); 
			NamespaceManager.AddNamespace("svg", SvgDocument.SvgNamespace); 
			NamespaceManager.AddNamespace("xlink", SvgDocument.XLinkNamespace);

			// Setup resource handler
			//ResourceHandler = new ResourceHandler(this);

			_window = window;
			_window.Document = this;

			AddStyleElement(SvgDocument.SvgNamespace, "style");
			CssPropertyProfile = CssPropertyProfile.SvgProfile;

            // Turning off the resolver disables DTD downloading and gives a 2.5x performance improvement and increased security.
            //this.XmlResolver = new CachingXmlUrlResolver();
            this.XmlResolver = null;

            // build tagName to type dictionary
            nodeByTagName = new TypeDictionary();
            buildTypeDictionary();
 		}

		#endregion

        #region Protected methods
        /// <summary>

        /// buildTypeDictionary

        /// </summary>
        protected virtual void buildTypeDictionary()
        {
            SetTagNameNodeType(SvgNamespace, "a",              typeof(SvgTransformableElement));
            SetTagNameNodeType(SvgNamespace, "circle",         typeof(SvgCircleElement));
            SetTagNameNodeType(SvgNamespace, "clipPath",       typeof(SvgClipPathElement));
            SetTagNameNodeType(SvgNamespace, "defs",           typeof(SvgDefsElement));
            SetTagNameNodeType(SvgNamespace, "desc",           typeof(SvgDescElement));
            SetTagNameNodeType(SvgNamespace, "ellipse",        typeof(SvgEllipseElement));
            SetTagNameNodeType(SvgNamespace, "g",              typeof(SvgGElement));
            SetTagNameNodeType(SvgNamespace, "image",          typeof(SvgImageElement));
            SetTagNameNodeType(SvgNamespace, "line",           typeof(SvgLineElement));
            SetTagNameNodeType(SvgNamespace, "linearGradient", typeof(SvgLinearGradientElement));
            SetTagNameNodeType(SvgNamespace, "marker",         typeof(SvgMarkerElement));
			SetTagNameNodeType(SvgNamespace, "mask",           typeof(SvgMaskElement));
			SetTagNameNodeType(SvgNamespace, "rect",           typeof(SvgRectElement));
			SetTagNameNodeType(SvgNamespace, "path",           typeof(SvgPathElement));
			SetTagNameNodeType(SvgNamespace, "pattern",       typeof(SvgPatternElement));
			SetTagNameNodeType(SvgNamespace, "polyline",       typeof(SvgPolylineElement));
			SetTagNameNodeType(SvgNamespace, "polygon",        typeof(SvgPolygonElement));
            SetTagNameNodeType(SvgNamespace, "radialGradient", typeof(SvgRadialGradientElement));
            SetTagNameNodeType(SvgNamespace, "script",         typeof(SvgScriptElement));
            SetTagNameNodeType(SvgNamespace, "stop",           typeof(SvgStopElement));
			SetTagNameNodeType(SvgNamespace, "svg",            typeof(SvgSvgElement));
            SetTagNameNodeType(SvgNamespace, "switch",         typeof(SvgSwitchElement));
            SetTagNameNodeType(SvgNamespace, "symbol",         typeof(SvgSymbolElement));
			SetTagNameNodeType(SvgNamespace, "text",           typeof(SvgTextElement));
            SetTagNameNodeType(SvgNamespace, "title",          typeof(SvgTitleElement));
            SetTagNameNodeType(SvgNamespace, "tref",           typeof(SvgTRefElement));
			SetTagNameNodeType(SvgNamespace, "tspan",          typeof(SvgTSpanElement));
			SetTagNameNodeType(SvgNamespace, "use",            typeof(SvgUseElement));
        }
        #endregion

		#region Public methods
        public void SetTagNameNodeType(string prefix, string localName, Type type) 
        {
            nodeByTagName[prefix + ":" + localName] = type;
        }

		public override XmlElement CreateElement(string prefix, string localName, string ns)
		{
            string name = ns + ":" + localName;
            XmlElement result;

            if ( nodeByTagName.ContainsKey(name) )
            {
                Type type = nodeByTagName[name];
                object[] args = new object[] { prefix, localName, ns, this };

                result = (XmlElement) nodeByTagName.CreateInstance(
                    name, args, BindingFlags.Instance | BindingFlags.NonPublic);
            }
			else
			{
				result = base.CreateElement(prefix, localName, ns);
			}

			return result;
		}

        public void Render(ISvgRenderer renderer)
        {
            SvgSvgElement root = RootElement as SvgSvgElement;
            if ( root != null ) 
                root.Render(renderer);
        }
		#endregion

		#region Public properties
		public new SvgDocument OwnerDocument
		{
			get
			{
				return (SvgDocument)base.OwnerDocument;
			}
		}

		public ISvgWindow Window
		{
			get
			{
				return _window;
			}
		}
		#endregion

		#region Static properties
		public static string SvgNamespace = "http://www.w3.org/2000/svg";
		public static string XLinkNamespace = "http://www.w3.org/1999/xlink";
		public static ArrayList SupportedFeatures = new ArrayList(new string[4]{
			"org.w3c.svg.static",
			"http://www.w3.org/TR/Svg11/feature#Shape",
			"http://www.w3.org/TR/Svg11/feature#BasicText",
			"http://www.w3.org/TR/Svg11/feature#OpacityAttribute"
		   });
		static public ArrayList SupportedExtensions = new ArrayList();
		#endregion

		#region Overrides of Load()
//        private void prepareReader(XmlReader reader)
//        {
//            reader.ValidationType = ValidationType.None;

//            LocalDtdXmlUrlResolver localDtdXmlUrlResolver = new LocalDtdXmlUrlResolver();
//            localDtdXmlUrlResolver.AddDtd("http://www.w3.org/TR/2001/REC-SVG-20010904/DTD/svg10.dtd", @"dtd\svg10.dtd");
//            localDtdXmlUrlResolver.AddDtd("http://www.w3.org/TR/SVG/DTD/svg10.dtd", @"dtd\svg10.dtd");
//            localDtdXmlUrlResolver.AddDtd("http://www.w3.org/Graphics/SVG/1.1/DTD/svg11-tiny.dtd", @"dtd\svg11-tiny.dtd");

//            // Basic exchanged for full DTD due to the Basic DTD problem / nikgus
//            localDtdXmlUrlResolver.AddDtd("http://www.w3.org/Graphics/SVG/1.1/DTD/svg11-basic.dtd", @"dtd\svg11.dtd");
//            localDtdXmlUrlResolver.AddDtd("http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", @"dtd\svg11.dtd");

//            ((XmlValidatingReader)reader).XmlResolver = localDtdXmlUrlResolver;
////            ((XmlValidatingReader)vr).XmlResolver = new CachingXmlUrlResolver();
//        }

        private static XmlReaderSettings readerSettings = null;
        private static XmlReaderSettings getReaderSettings()
        {
            if (readerSettings == null)
            {
                readerSettings = new XmlReaderSettings();
                readerSettings.ValidationType = ValidationType.DTD;
                readerSettings.ProhibitDtd = false;


                //XmlReaderSettings settings = new XmlReaderSettings();
                //settings.ProhibitDtd = false;
                //settings.ValidationType = ValidationType.DTD;
                
                //XmlTextReader xtr = new XmlTextReader("http://www.w3.org/TR/2001/REC-SVG-20010904/DTD/svg10.dtd");

                //readerSettings.Schemas.Add(null, "http://www.w3.org/TR/2001/REC-SVG-20010904/DTD/svg10.dtd");
                //readerSettings.Schemas.Add(null, "http://www.w3.org/TR/SVG/DTD/svg10.dtd");
                //readerSettings.Schemas.Add(null, "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11-tiny.dtd");

                //readerSettings.Schemas.Add(null, "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11-basic.dtd");
                //readerSettings.Schemas.Add(null, "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd");
            }
            return readerSettings;
        }

		public override void Load(string url)
		{
            XmlReader vr = XmlReader.Create(new XmlTextReader(url), getReaderSettings());
			
			base.Load(vr);
		}

		public void Load(string url, Stream stream)
		{            
            XmlReader vr = XmlReader.Create(new XmlTextReader(url, stream), getReaderSettings());
			base.Load(vr);
		}

		public override void Load(Stream stream)
		{
			Load("", stream);
		}
		#endregion

		public XmlNode GetNodeByUri(Uri absoluteUri)
		{
			Uri docUri = ResolveUri("");

			string fragment = absoluteUri.Fragment;

			if(fragment.Length == 0)
			{
				// no fragment => return entire document
				if(docUri.AbsolutePath == absoluteUri.AbsolutePath)
				{
					return this;
				}
				else
				{
					SvgDocument doc = new SvgDocument((SvgWindow)Window);

					XmlTextReader xtr = new XmlTextReader(absoluteUri.AbsolutePath, GetResource(absoluteUri).Stream );
					
                    XmlReaderSettings readerSettings = new XmlReaderSettings();
                    readerSettings.ValidationType = ValidationType.None;
                    XmlReader vr = XmlReader.Create(xtr, readerSettings);
                    //XmlValidatingReader vr = new XmlValidatingReader(xtr);
					//vr.ValidationType = ValidationType.None;
					doc.Load(vr);
					return doc;
				}
			}
			else
			{
				// got a fragment => return XmlElement
				string noFragment = absoluteUri.AbsoluteUri.Replace(fragment, "");
				SvgDocument doc = (SvgDocument)GetNodeByUri(new Uri(noFragment));
				return doc.SelectSingleNode("//*[@id='" + fragment.Substring(1) + "']");
			}
		}

		public Uri ResolveUri(string uri)
		{
			string baseUri = BaseURI;
			if(baseUri.Length == 0)
			{
				baseUri = "file:///" + new DirectoryInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase).Parent.FullName.Replace('\\', '/');
			}

			return new Uri(new Uri(baseUri), uri);
		}

        public Uri ResolveUri(string baseUri, string uri)
        {
            if (baseUri.Length == 0)
            {
                baseUri = "file:///" + new DirectoryInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase).Parent.FullName.Replace('\\', '/');
            }

            return new Uri(new Uri(baseUri), uri);
        }

		#region Implementation of ISvgDocument
		/// <summary>
		/// The title of the document which is the text content of the first child title element of the 'svg' root element.
		/// </summary>
		public string Title
        {
			get
			{
                string result = "";

                foreach ( XmlNode node in RootElement.ChildNodes )
                {
                    if ( node.NodeType     == XmlNodeType.Element      &&
                         node.NamespaceURI == SvgDocument.SvgNamespace &&
                         node.LocalName    == "title" )
                    {
                        if ( node.HasChildNodes )
                        {
                            node.Normalize();
                            // NOTE: should probably use spec-defined whitespace
                            result = Regex.Replace(node.FirstChild.Value, @"\s\s+", " ");
                        }
                        break;
                    }
                }

				return result;
			}
		}

		/// <summary>
		/// Returns the URI of the page that linked to this page. The value is an empty string if the user navigated to the page directly (not through a link, but, for example, via a bookmark).
		/// </summary>
		public string Referrer
        {
			get
			{
				return String.Empty;
			}
		}


		/// <summary>
		/// The domain name of the server that served the document, or a null string if the server cannot be identified by a domain name.
		/// </summary>
		public string Domain
		{
			get
			{
                return new Uri(Url).Host;
			}
		}

		/// <summary>
		/// The root 'svg' element in the document hierarchy
		/// </summary>
		public ISvgSvgElement RootElement
		{
			get
			{
				return DocumentElement as ISvgSvgElement;
			}
		}

		#endregion
	}
}
