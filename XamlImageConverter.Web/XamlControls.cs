using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Silversite.Web.UI {


	[ToolboxData("<{0}:XamlImage runat=\"server\" />")]
	[ParseChildren(ChildrenAsProperties = false, DefaultProperty="XamlContent")]
	[PersistChildren(true)]
	public class XamlImage: Image {

		XNamespace ns = "http://schemas.johnshope.com/XamlImageConverter/2012";
		XNamespace xaml ="http://schemas.microsoft.com/winfx/2006/xaml/presentation";
      XNamespace xamlx="http://schemas.microsoft.com/winfx/2006/xaml";

		protected override void OnInit(EventArgs e) {
			base.OnInit(e);
			if (XamlContent.Count == 1 && XamlContent[0] is Literal) {
				Xaml = ((Literal)XamlContent[0]).Text;
			}
		}
		protected override void CreateChildControls() {
			base.CreateChildControls();
		}

		public string Storyboard { get { return (string)ViewState["Storyboard"]; } set { ViewState["Storyboard"] = value; } }
		public string Theme { get { return (string)ViewState["Theme"]; } set { ViewState["Theme"] = value; } }
		public string Skin { get { return (string)ViewState["Skin"]; } set { ViewState["Skin"] = value; } }
		public string Cultures { get { return (string)ViewState["Cultures"]; } set { ViewState["Cultures"] = value; } }
		public string TextMode { get { return (string)ViewState["TextMode"]; } set { ViewState["TextMode"] = value; } }
		public int? Quality { get { return (int?)ViewState["Quality"]; } set { ViewState["Quality"] = value; } }
		public int? Loops { get { return (int?)ViewState["Loops"]; } set { ViewState["Loops"] = value; } }
		public double? Pause { get { return (double?)ViewState["Pause"]; } set { ViewState["Pause"] = value; } }
		public int? Dpi { get { return (int?)ViewState["Dpi"]; } set { ViewState["Dpi"] = value; } }

		public Guid Guid { get { return (Guid)(ViewState["Guid"] ?? (Guid = new Guid())); } set { ViewState["Guid"] = value; } }

		XElement element = null;
		public XElement XElement {
			get { return element ?? (element = new XElement(Xaml)); }
			set {
				using (var w = new StringWriter()) {
					value.Save(w , SaveOptions.OmitDuplicateNamespaces);
					Xaml = w.ToString();
				}
			}
		}

		public string Xaml {
			get { return (string)Page.Session["XamlImageConverter.Xaml:#XamlImage" + Guid.ToString()]; }
			set {
				if (Page.Session["XamlImageConverter.Xaml:#XamlImage" + Guid.ToString()] != value) {
					Page.Session["XamlImageConverter.Xaml:#XamlImage" + Guid.ToString()] = value;
					element = null;
					if (XElement.Name == ns + "XamlImageConverter") {
						var snapshot = XElement.Descendants()
							.FirstOrDefault(x => x.Name == ns + "Snapshot" && (string.IsNullOrEmpty(ImageUrl) || (string)x.Attribute("File") == ImageUrl || (string)x.Attribute("Filename") == ImageUrl));
						if (snapshot == null) {
							snapshot = XElement.Descendants()
								.FirstOrDefault(x => x.Name == ns + "Snapshot");
							ImageUrl = (string)(snapshot.Attribute("File") ?? snapshot.Attribute("Filename"));
						}
					} else {
						if (Storyboard != null) XElement.SetAttributeValue(ns + "Storyboard", Storyboard);
						if (Theme != null) XElement.SetAttributeValue(ns + "Theme", Theme);
						if (Skin != null) XElement.SetAttributeValue(ns + "Skin", Skin);
						if (Cultures != null) XElement.SetAttributeValue(ns + "Cultures", Cultures);
						if (TextMode != null) XElement.SetAttributeValue(ns + "TextMode", TextMode);
						if (Quality != null) XElement.SetAttributeValue(ns + "Quality", Quality);
						if (Loops != null) XElement.SetAttributeValue(ns + "Loops", Quality);
						if (Pause != null) XElement.SetAttributeValue(ns + "Pause", Quality);
						if (Dpi != null) XElement.SetAttributeValue(ns + "Dpi", Quality);
						if (!string.IsNullOrEmpty(ImageUrl)) {
							var namewhash = Path.GetFileNameWithoutExtension(ImageUrl);
							var name = Path.GetFileNameWithoutExtension(namewhash);
							var hashid = Path.GetExtension(namewhash);
							int id = 0;
							if (!int.TryParse(hashid, out id)) name = namewhash;
							XElement.SetAttributeValue(ns + "File", "__XamlImageConverter.ImageUrl" + name);
							var hash = Hash.Compute(XElement.ToString(SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces));
							ImageUrl = name + "." + hash + "." + Path.GetExtension(ImageUrl);
							XElement.SetAttributeValue(ns + "File", ImageUrl);
						} else if (!XElement.DescendantsAndSelf().Any(x => x.Attributes().Any(a => a.Name == ns + "Normal.View"))) {
							XElement.SetAttributeValue(ns + "Normal.View", "");
						}
						Page.Session["XamlImageConverter.Xaml:#XamlImage" + Guid.ToString()] = value = XElement.ToString(SaveOptions.OmitDuplicateNamespaces);
					}
				}
			}
		}

		public ControlCollection XamlContent { get; set; }

		public XamlImage() { XamlContent = new ControlCollection(this); }

		protected override object SaveViewState() {
			return base.SaveViewState();
		}

		protected override void Render(HtmlTextWriter writer) {
			var oldimage = ImageUrl;
			ImageUrl = "xic.axd?Image=" + ImageUrl + "&Xaml=#XamlImage" + Guid.ToString();
			base.Render(writer);
			ImageUrl = oldimage;
		}
	}
}
