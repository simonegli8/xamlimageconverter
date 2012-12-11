using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Media;


using SharpVectors.Net;
using System.Windows.Media.Imaging;


namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Summary description for SvgImageElement.
	/// </summary>
	public class SvgImageElement : SvgTransformableElement, ISvgImageElement
	{
		#region Constructors
		internal SvgImageElement(string prefix, string localname, string ns, SvgDocument doc) : base(prefix, localname, ns, doc) 
		{
			svgExternalResourcesRequired = new SvgExternalResourcesRequired(this);
			svgTests = new SvgTests(this);
		}
		#endregion

		#region Implementation of ISvgImageElement
		public ISvgAnimatedString Href
		{
			get{
				return new SvgAnimatedString(this.GetAttribute("href", SvgDocument.XLinkNamespace));
			}
		}

		public ISvgAnimatedLength Width
		{
			get
			{
				return new SvgAnimatedLength("width", GetAttribute("width"), "0", this, SvgLengthDirection.Horizontal);
			}
		}
  		
		public ISvgAnimatedLength Height
		{
			get
			{
				return new SvgAnimatedLength("height", GetAttribute("height"), "0", this, SvgLengthDirection.Vertical);
			}
  			
		}
  		
		public ISvgAnimatedLength X
		{
			get
			{
				return new SvgAnimatedLength("x", GetAttribute("x"), "0", this, SvgLengthDirection.Horizontal);
			}
  			
		}

		public ISvgAnimatedLength Y
		{
			get
			{
				return new SvgAnimatedLength("y", GetAttribute("y"), "0", this, SvgLengthDirection.Vertical);
			}
  			
		}
		public ISvgAnimatedPreserveAspectRatio PreserveAspectRatio
		{
			get
			{
				return new SvgAnimatedPreserveAspectRatio(GetAttribute("preserveAspectRatio"));
			}
		}
		#endregion

		#region Public methods
		public SvgDocument GetImageDocument()
		{
            SvgWindow window = SvgWindow;
			if(window == null)
			{
				return null;
			}
			else
			{
                return (SvgDocument)window.Document;
			}
		}

		#endregion

		#region Public properties
		public SvgRect CalulatedViewbox
		{
			get
			{
				SvgRect viewBox;

				if ( IsSvgImage )
				{
                    SvgDocument doc = GetImageDocument();
					SvgSvgElement outerSvg = (SvgSvgElement)doc.DocumentElement;

					if ( outerSvg.HasAttribute("viewBox") )
					{
						viewBox = (SvgRect)outerSvg.ViewBox.AnimVal;
					}
					else
					{
                        viewBox = SvgRect.Empty;                        
					}
				}
				else
				{
					viewBox = new SvgRect(0,0, Bitmap.Width, Bitmap.Height);
				}

				return viewBox;
			}

		}

		public bool IsSvgImage
		{
			get
			{
				HttpResource resource = ReferencedResource;

                // local files are returning as binary/octet-stream
                // this "fix" tests the file extension for .svg and .svgz
                string name = resource.Uri.ToString().ToLower(CultureInfo.InvariantCulture);
                return (
                    resource.ContentType.StartsWith("image/svg+xml") ||
                    name.EndsWith(".svg") ||
                    name.EndsWith(".svgz") );
			}
		}

        public ImageSource Bitmap
		{
			get
			{
				if(!IsSvgImage)
				{

                    if (ReferencedResource.Uri.Scheme == "data")
                        return null;
                    //if (ReferencedResource.Stream != null)
                    //{
                    //    BitmapFrame image = BitmapFrame.Create(ReferencedResource.Stream);
                    //    return image;
                    //}

                    //else
                    //{
                        BitmapImage bi = new BitmapImage();
                        
                        // BitmapImage.UriSource must be in a BeginInit/EndInit block.
                        bi.BeginInit();
                        bi.UriSource = ReferencedResource.Uri; //new Uri(@"/sampleImages/cherries_larger.jpg", UriKind.RelativeOrAbsolute);
                        bi.EndInit();

                        return bi;
                    //}
				}
				else
				{
					return null;
				}
			}
		}

		public SvgWindow SvgWindow
		{
			get
			{
				if(IsSvgImage)
				{
					SvgWindow parentWindow = (SvgWindow)OwnerDocument.Window;

					SvgWindow wnd = new SvgWindow((double)Width.AnimVal.Value, (double)Height.AnimVal.Value, parentWindow);

					SvgDocument doc = new SvgDocument(wnd);
					wnd.Document = doc;

					Uri absoluteUri = ResolveUri(Href.AnimVal);

					ReferencedResource.Stream.Position = 0;
					doc.Load(absoluteUri.AbsoluteUri, ReferencedResource.Stream);

					return wnd;
				}
				else
				{
					return null;
				}
			}
		}

		private HttpResource _referencedResource;
		public HttpResource ReferencedResource
		{
			get
			{
				if(_referencedResource == null)
				{
					Uri uri = ResolveUri(Href.AnimVal);
					_referencedResource = OwnerDocument.GetResource(uri);
				}
				return _referencedResource;
			}
		}
		#endregion

		#region Implementation of ISvgExternalResourcesRequired
		private SvgExternalResourcesRequired svgExternalResourcesRequired;
		public ISvgAnimatedBoolean ExternalResourcesRequired
		{
			get
			{
				return svgExternalResourcesRequired.ExternalResourcesRequired;
			}
		}
		#endregion

		#region Implementation of ISvgTests
		private SvgTests svgTests;
		public ISvgStringList RequiredFeatures
		{
			get { return svgTests.RequiredFeatures; }
		}

		public ISvgStringList RequiredExtensions
		{
			get { return svgTests.RequiredExtensions; }
		}

		public ISvgStringList SystemLanguage
		{
			get { return svgTests.SystemLanguage; }
		}

		public bool HasExtension(string extension)
		{
			return svgTests.HasExtension(extension);
		}
        #endregion
		
	}
}
