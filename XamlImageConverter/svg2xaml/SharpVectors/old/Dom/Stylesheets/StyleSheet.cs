using System;
using System.Diagnostics;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using SharpVectors.Dom.Css;

namespace SharpVectors.Dom.Stylesheets
{
	/// <summary>
	/// The StyleSheet interface is the abstract base interface for any type of style sheet. It represents a single style sheet associated with a structured document. In HTML, the StyleSheet interface represents either an external style sheet, included via the HTML LINK element, or an inline STYLE element. In XML, this interface represents an external style sheet, included via a style sheet processing instruction.
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>75</completed>
	public class StyleSheet : IStyleSheet
	{
		#region Constructors
		internal StyleSheet()
		{
		}
        private static Regex re = new Regex(@"(?<name>[a-z]+)=[""'](?<value>[^""']*)[""']", RegexOptions.Compiled);
        internal StyleSheet(XmlProcessingInstruction pi)
		{
            Match match = re.Match(pi.Data);

			while(match.Success)
			{
				string name = match.Groups["name"].Value;
				string val = match.Groups["value"].Value;

				switch(name)
				{
					case "href":
						_Href = val;
						break;
					case "type":
						_Type = val;
						break;
					case "title":
						_Title = val;
						break;
					case "media":
						_Media = new MediaList(val);
						break;
				}
				match = match.NextMatch();
			}

			ownerNode = (XmlNode)pi;
		}

		internal StyleSheet(XmlElement styleElement)
		{
			if(styleElement.HasAttribute("href")) _Href = styleElement.Attributes["href"].Value;
			if(styleElement.HasAttribute("type")) _Type = styleElement.Attributes["type"].Value;
			if(styleElement.HasAttribute("title")) _Title = styleElement.Attributes["title"].Value;
			if(styleElement.HasAttribute("media")) _Media = new MediaList(styleElement.Attributes["media"].Value);

			ownerNode = (XmlNode)styleElement;
		}

		internal StyleSheet(XmlNode ownerNode, string href, string type, string title, string media)
		{
			this.ownerNode = ownerNode;
			_Href = href;
			_Type = type;
			_Title = title;
			_Media = new MediaList(media);
		}

		#endregion

		#region Internal methods
		/// <summary>
		/// Used to find matching style rules in the cascading order
		/// </summary>
		/// <param name="elt">The element to find styles for</param>
		/// <param name="pseudoElt">The pseudo-element to find styles for</param>
		/// <param name="ml">The medialist that the document is using</param>
		/// <param name="csd">A CssStyleDeclaration that holds the collected styles</param>
		protected internal virtual void GetStylesForElement(XmlElement elt, string pseudoElt, MediaList ml, CssCollectedStyleDeclaration csd)
		{
			
		}
		#endregion

		#region Public methods
		internal XmlNode ResolveOwnerNode()
		{
			if(OwnerNode != null) return OwnerNode;
			else
			{
				return ((StyleSheet)ParentStyleSheet).ResolveOwnerNode();
			}
		}

		#endregion

		#region Protected methods
		internal void LoadSheet()
		{
			WebRequest request = (WebRequest)WebRequest.Create(AbsoluteHref);
			TriedDownload = true;
			try
			{
				WebResponse response = (WebResponse)request.GetResponse();

				SucceededDownload = true;
				System.IO.StreamReader str = new StreamReader(response.GetResponseStream(), System.Text.Encoding.Default, true);
				sheetContent = str.ReadToEnd();
				str.Close();
			}
			catch
			{
				SucceededDownload = false;
				sheetContent = String.Empty;
			}

		}

		#endregion

		#region Private fields
		private bool TriedDownload = false;
		private bool SucceededDownload = false;
		#endregion

		#region Protected properties
		private string sheetContent = null;
		internal string SheetContent
		{
			get
			{
				if(OwnerNode is XmlElement)
				{
                    return OwnerNode.InnerText;
				}
				else
				{
					// a PI
					if(!TriedDownload)
					{
						LoadSheet();
					}
					if(SucceededDownload) return sheetContent;
					else return String.Empty;
				}
			}

		}
		#endregion

		#region Implementation of IStyleSheet
		private MediaList _Media = new MediaList();
		/// <summary>
		/// The intended destination media for style information. The media is often specified in the ownerNode. If no media has been specified, the MediaList will be empty. See the media attribute definition for the LINK element in HTML 4.0, and the media pseudo-attribute for the XML style sheet processing instruction . Modifying the media list may cause a change to the attribute disabled.
		/// </summary>
		public IMediaList Media
		{
			get
			{
				return _Media;
			}
		}

		private string _Title = String.Empty;
		/// <summary>
		/// The advisory title. The title is often specified in the ownerNode. See the title attribute definition for the LINK element in HTML 4.0, and the title pseudo-attribute for the XML style sheet processing instruction.
		/// </summary>
		public string Title
		{
			get
			{
				return _Title;
			}
		}

		private string _Href = String.Empty;
		/// <summary>
		/// If the style sheet is a linked style sheet, the value of its attribute is its location. For inline style sheets, the value of this attribute is null. See the href attribute definition for the LINK element in HTML 4.0, and the href pseudo-attribute for the XML style sheet processing instruction.
		/// </summary>
		public string Href
		{
			get
			{
				return _Href;
			}
		}
		/// <summary>
		/// The resolved absolute URL to the stylesheet
		/// </summary>
		public Uri AbsoluteHref
		{
			get
			{
				Uri u;
				if(OwnerNode != null)
				{
					u = new Uri(new Uri(OwnerNode.BaseURI), Href);
				}
				else
				{
					u = new Uri(Href);
				}
				return u;
			}
		}

		private IStyleSheet _ParentStyleSheet = null;
		/// <summary>
		/// For style sheet languages that support the concept of style sheet inclusion, this attribute represents the including style sheet, if one exists. If the style sheet is a top-level style sheet, or the style sheet language does not support inclusion, the value of this attribute is null.
		/// </summary>
		public IStyleSheet ParentStyleSheet
		{
			get
			{
				return _ParentStyleSheet;
			}
		}

		private XmlNode ownerNode = null;
		/// <summary>
		/// The node that associates this style sheet with the document. For HTML, this may be the corresponding LINK or STYLE element. For XML, it may be the linking processing instruction. For style sheets that are included by other style sheets, the value of this attribute is null.
		/// </summary>
		public XmlNode OwnerNode
		{
			get
			{
				return ownerNode;
			}
		}

		private bool _Disabled = false;
		/// <summary>
		/// false if the style sheet is applied to the document. true if it is not. Modifying this attribute may cause a new resolution of style for the document. A stylesheet only applies if both an appropriate medium definition is present and the disabled attribute is false. So, if the media doesn't apply to the current user agent, the disabled attribute is ignored.
		/// </summary>
		public bool Disabled
		{
			get
			{
				return _Disabled;
			}
			set
			{
				_Disabled = value;
			}
		}

		private string _Type = String.Empty;
		/// <summary>
		/// This specifies the style sheet language for this style sheet. The style sheet language is specified as a content type (e.g. "text/css"). The content type is often specified in the ownerNode. Also see the type attribute definition for the LINK element in HTML 4.0, and the type pseudo-attribute for the XML style sheet processing instruction.
		/// </summary>
		public string Type
		{
			get
			{
				return _Type;
			}
		}
		#endregion
	}
}
