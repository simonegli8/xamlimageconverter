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

namespace XamlImageConverter.Web.UI {

	[ToolboxData("<{0}:XamlImage runat=\"server\" />")]
	[ParseChildren(ChildrenAsProperties = false)]
	[PersistChildren(true)]
	public class XamlImage: Image {

		static XNamespace xic = "http://schemas.johnshope.com/XamlImageConverter/2012";
		static XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
		static XNamespace xamlx = "http://schemas.microsoft.com/winfx/2006/xaml";
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
		public double? Scale { get { return (double?)ViewState["Scale"]; } set { ViewState["Scale"] = value; } }
		protected string SessionID { get { return SessionPrefix + Guid.ToString(); } }
		protected string Output {
			get {
				var hashsb = new StringBuilder();
				hashsb.Append(Storyboard ?? "");
				hashsb.Append(Theme ?? "");
				hashsb.Append(Skin ?? "");
				hashsb.Append(Cultures ?? "");
				hashsb.Append(Type ?? "");
				hashsb.Append(Scale ?? 1);
				hashsb.Append(Content ?? "");
				var hash = Hash.Compute(hashsb.ToString());
				hash += 10 * (Quality ?? 90) + 1000 * (Loops ?? 1) + (int)(10000 * (Pause ?? 0)) + 100000 * (Dpi ?? 96);
				var image = ImageUrl;
				var lastSlash = image.LastIndexOf('/');
				if (lastSlash > -1) image = image.Substring(lastSlash + 1);
				return Path.ChangeExtension(image, hash.ToString("X") + Path.GetExtension(image));
			}
		}

		XElement element = null;
		public XElement XElement {
			get { return element ?? (element = XElement.Parse(Content)); }
			set {
				using (var w = new StringWriter()) {
					value.Save(w, SaveOptions.OmitDuplicateNamespaces);
					Content = w.ToString();
				}
			}
		}

		public string Content { get; set; }
		public string Source { get; set; }

		string Url {
			get {
				var sb = new StringBuilder();
				if (string.IsNullOrEmpty(Source)) {
					var path = Image ?? "";
					path = path.Substring(0, path.LastIndexOf('/')+1);
					sb.Append(path + "xic.axd?Verbose=true&Source="); sb.Append(HttpUtility.UrlEncode(SessionID));
				} else {
					sb.Append(Source + "?");
				}
				var e = XElement;
				if (e.Name != xic + "XamlImageConverter") {
					if (!string.IsNullOrEmpty(Storyboard)) { sb.Append("&Storyboard="); sb.Append(HttpUtility.UrlEncode(Storyboard)); }
					if (!string.IsNullOrEmpty(Theme)) { sb.Append("&Theme="); sb.Append(HttpUtility.UrlEncode(Theme)); }
					if (!string.IsNullOrEmpty(Skin)) { sb.Append("&Skin="); sb.Append(HttpUtility.UrlEncode(Skin)); }
					if (!string.IsNullOrEmpty(Cultures)) { sb.Append("&Cultures="); sb.Append(HttpUtility.UrlEncode(Cultures)); }
					if (Quality.HasValue) { sb.Append("&Quality="); sb.Append(Quality.Value); }
					if (Loops.HasValue) { sb.Append("&Loops="); sb.Append(Loops.Value); }
					if (Pause.HasValue) { sb.Append("&Pause="); sb.Append(Pause.Value); }
					if (Dpi.HasValue) { sb.Append("&Dpi="); sb.Append(Dpi.Value); }
					if (Scale.HasValue) { sb.Append("&Scale="); sb.Append(Scale.Value); }
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
					sb.Append("&Image="); sb.Append(HttpUtility.UrlEncode(Output));
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

	/*
	public class Map: XamlImage {
		
		public enum SelectModes = { Multiple, Single };

		public SelectModes Select { get { return (SelectModes)(ViewState["Select"] ?? SelectModes.Multiple; } set { ViewState["Select"] = value; } }
		public string Source { get { return Image; } set { Image = value; } }
		public bool Legend { get { return (bool)(ViewState["Legend"] ?? false); } set { ViewState["Legend"] = value; } } 
		public int Columns { get { return (int)(ViewState["Columns"] ?? 1); } set { ViewState["Columns"] = value; } }
		public string SubmitText { get { return (string)ViewState["SubmitText"]; } set { ViewState["SubmitText"] = value; } }
		public string CancelText { get { return (string)ViewState["CancelText"]; } set { ViewState["CancelText"] = value; } }
		public string IDs { get { return (string)ViewState["IDs"]; } set { ViewState["IDs"] = value; } }
		public string Regions { get { return (string)ViewState["Regions"]; } set { ViewState["Regions"] = value; } }

		public event EventHandler OnSubmit;
		public event EventHandler OnCancel;
		
		protected override void CreateChildControls() {
			var map = new StringBuilder();
			map.Append("<xic:XamlImageConverter xmlns:xic=\"http://schemas.johnshope.com/XamlImageConverter/2012\"><xic:Scene ");
			map.Append((string.IsNullOrEmpty(Type) ? "Source=\"" + Source : "Type=\"" + Type));
			map.Append("\"><xic:Snapshot File=\"");
			map.Append(Image); map.Append("\" ");
			if (!string.IsNullOrEmpty(Theme)) { map.Append("Theme=\""); map.Append(HttpUtility.UrlEncode(Theme)); map.Append("\" "); }
			if (!string.IsNullOrEmpty(Skin)) { map.Append("Skin="); map.Append(HttpUtility.UrlEncode(Skin)); map.Append("\" "); }
			if (!string.IsNullOrEmpty(Cultures)) { map.Append("Cultures="); map.Append(HttpUtility.UrlEncode(Cultures)); map.Append("\" "); }
			if (Quality.HasValue) { map.Append("Quality="); map.Append(Quality.Value); map.Append("\" "); }
			if (Dpi.HasValue) { map.Append("Dpi="); map.Append(Dpi.Value); map.Append("\" "); }
			if (Scale.HasValue) { map.Append("Scale="); map.Append(Scale.Value); map.Append("\" "); }
			if (!string.IsNullOrEmpty(Type)) { map.Append("Type="); map.Append(HttpUtility.UrlEncode(Type)); map.Append("\" "); }
			map.Append("/><xic:Map Type=\"Html\" FileType=\"IncludeFile\" Image=\""); map.Append(Image); map.Append("\" File=\""); map.Append(Output); map.Append(".map\" ");

			base.CreateChildControls();
		}

		<xic:Map runat="server" Source="Usa.Map.svg" Scale="0.5" Mode="Multiple" CssClass="map" Legend="true" Columns="3" SubmitText="Submit" 
			IDs="WA,OR,CA,AK,ID,NV,AZ,UT,MT,WY,CO,NM,TX,OK,KS,NE,SD,ND,MN,IA,MO,AR,LA,WI,IL,TN,MS,MI,IN,KY,AL,FL,GA,SC,NC,VA,WV,OH,PA,MD,NJ,NY,CT,MA,VT,NH,ME,RI,DE,HI"
			Regions="Washington,Oregon,California,Arkansas,Utah,Montana,Wyoming,Colorado,New Mexico,Texas,Oklahoma,Kansas,Nebraska,South Dakota,North Dakota,Minnesota,Iowa,Mississippi,Michigan,Indiana,Kentucky,Alabama,Florida,Georgia,South Carolina,North Carolina,Virginia,West Virginia,Ohio,Pennsylvania,Maryland,New Jersey,New York,Connecticut,Massachusetts,Vermont,New Hampshire,Maine,Rhode Island, Delaware,Hawaii" />
	}
	*/
}
