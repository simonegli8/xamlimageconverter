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
	[ParseChildren(ChildrenAsProperties = false)]
	[PersistChildren(true)]
	public class XamlImage: Image {

		static XNamespace xic = "http://schemas.johnshope.com/XamlImageConverter/2012";
		static XNamespace xaml ="http://schemas.microsoft.com/winfx/2006/xaml/presentation";
		static XNamespace xamlx="http://schemas.microsoft.com/winfx/2006/xaml";
		const string SessionPrefix = "#XamlImageConverter.Xaml:";

		protected override void OnInit(EventArgs e) {
			base.OnInit(e);
			if (Controls.Count == 1 && Controls[0] is LiteralControl) {
				Content = ((LiteralControl)Controls[0]).Text;
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
		public string Parameters { get { return (string)ViewState["Parameters"]; } set { ViewState["Parameters"] = value; } }
		public Guid Guid { get { return (Guid)(ViewState["Guid"] ?? (Guid = System.Guid.NewGuid())); } set { ViewState["Guid"] = value; } }
		public string Type { get { return (string)ViewState["Type"]; } set { ViewState["Type"] = value; } }
		public string Image { get { return ImageUrl; } set { ImageUrl = value; } }
		string SessionID { get { return SessionPrefix + Guid.ToString(); } }

		XElement element = null;
		public XElement XElement {
			get { return element ?? (element = XElement.Parse(Content)); }
			set {
				using (var w = new StringWriter()) {
					value.Save(w , SaveOptions.OmitDuplicateNamespaces);
					Content = w.ToString();
				}
			}
		}

		public string Content { get; set; }

		string Url {
			get {
				var sb = new StringBuilder();
				sb.Append("xic.axd?Source="); sb.Append(HttpUtility.UrlEncode(SessionID));
				var e = XElement;
				if (e.Name != xic+"XamlImageConverter") {
					if (!string.IsNullOrEmpty(Storyboard)) { sb.Append("&Storyboard="); sb.Append(HttpUtility.UrlEncode(Storyboard)); }
					if (!string.IsNullOrEmpty(Theme)) { sb.Append("&Theme="); sb.Append(HttpUtility.UrlEncode(Theme)); }
					if (!string.IsNullOrEmpty(Skin)) { sb.Append("&Skin="); sb.Append(HttpUtility.UrlEncode(Skin)); }
					if (!string.IsNullOrEmpty(Cultures)) { sb.Append("&Cultures="); sb.Append(HttpUtility.UrlEncode(Cultures)); }
					if (Quality.HasValue) { sb.Append("&Quality="); sb.Append(Quality.Value); }
					if (Loops.HasValue) { sb.Append("&Loops="); sb.Append(Loops.Value); }
					if (Pause.HasValue) { sb.Append("&Pause="); sb.Append(Pause.Value); }
					if (Dpi.HasValue) { sb.Append("&Dpi="); sb.Append(Dpi.Value); }
					if (!string.IsNullOrEmpty(Type)) { sb.Append("&Type="); sb.Append(HttpUtility.UrlEncode(Type)); }
					/*if (e.Name.NamespaceName == "") {
						e.Name = xaml + e.Name.LocalName;
						e.SetAttributeValue(XNamespace.Xmlns + "x", xamlx.NamespaceName);
					}
					var xml = new StringBuilder();
					using (var w = System.Xml.XmlWriter.Create(xml)) {
						e.Save(w);
					}*/
					Page.Session[SessionID] = Content.Trim(); //xml.ToString();
				} else {
					Page.Session[SessionID] = Content.Trim();
				}

				Page.Response.Cache.SetCacheability(HttpCacheability.NoCache);

				foreach (var par in (Parameters ?? "").Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries)) {
					sb.Append("&");
					if (par.Contains('=')) {
						var tokens = par.Split('=');
						sb.Append(tokens[0]);
						sb.Append(HttpUtility.UrlEncode(tokens[1]));
					} else {
						sb.Append(HttpUtility.UrlEncode(par));
					}
				}
				if (!string.IsNullOrEmpty(ImageUrl)) {
					var hashsb = new StringBuilder();
					hashsb.Append(Storyboard ?? "");
					hashsb.Append(Theme ?? "");
					hashsb.Append(Skin ?? "");
					hashsb.Append(Cultures ?? "");
					hashsb.Append(Type ?? "");
					hashsb.Append(Content ?? "");
					var hash = Hash.Compute(hashsb.ToString());
					hash += 10*(Quality ?? 90) + 1000*(Loops ?? 1) + (int)(10000*(Pause??0)) + 100000*(Dpi??96);
					sb.Append("&Image="); sb.Append(HttpUtility.UrlEncode(Path.ChangeExtension(ImageUrl, hash.ToString("X") + Path.GetExtension(ImageUrl))));
				}
				return sb.ToString();
			}
		}

		protected override object SaveViewState() {
			return base.SaveViewState();
		}

		protected override void Render(HtmlTextWriter writer) {
			var oldimage = ImageUrl;
			ImageUrl = Url;
			base.Render(writer);
			ImageUrl = oldimage;
		}
	}
}
