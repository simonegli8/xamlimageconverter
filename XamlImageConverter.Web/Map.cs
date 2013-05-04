using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XamlImageConverter.Web.UI;

namespace XamlImageConverter.Web.UI {

	public class Map: UserControl {

		public enum Modes { Click, Select }; 
		public string Source { get; set; }
		public string IDs { get; set; }
		public string Regions { get; set; }
		public Modes Mode { get; set; }
		public bool Legend { get; set; }
		public string SubmitText { get; set; }
		public string Selected { get; set; }
		public double Scale { get; set; }
		public int LegendTop { get; set; }
		public int LegendLeft {get; set; }
		public string CssClass { get; set; }
		public event EventHandler Submit;
		
		static int no = 0;

		public Map() {
			Mode = Modes.Click;
			Source = null;
			Regions = "*";
			Legend = false;
			Selected = "";
			Scale = 1;
		}

		protected override void CreateChildControls() {
			base.CreateChildControls();
			var xaml = new XamlImage();

			var cssclass = CssClass;
			if (string.IsNullOrEmpty(cssclass)) cssclass = "";
			else cssclass = " " + cssclass;

			var source = Source;
			if (!Source.StartsWith("~") && !Source.StartsWith("http")) {
				var path = Page.AppRelativeVirtualPath;
				var slash = path.LastIndexOf('/');
				if (slash >= 0) path = path.Substring(0, slash+1);
				Source = path + Source;
			}

			if (Mode == Modes.Select) {
				var boxlist = new CheckBoxList();
				int n = 0;
				if (string.IsNullOrEmpty(IDs)) IDs = Regions;
				
				var ids = IDs.Split(new char[] { ',',';' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(id => id.Trim())
					.ToList();
				foreach (var region in Regions.Split(new char[] { ',',';' }, StringSplitOptions.RemoveEmptyEntries).Select(reg => reg.Trim())) {
					var listItem = new ListItem();
					listItem.Attributes["id"] = "checkbox_" + IDs;
					boxlist.Items.Add(new ListItem(region, n.ToString()));
				}
			
				Page.ClientScript.RegisterClientScriptBlock(typeof(Map), ClientID, "var xic_selected = function(id) { var box = $('#checkbox_' + id); box.prop(\"checked\", !box.attr(\"checked\")); };");
				var src = "<XamlImageConverter xmlns=\"http://schemas.johnshope.com/XamlImageConverter/2012\"><Scene Source=\"" + Source + "\"><Snapshot Type=\"png\" />" +
					"<Map File=\"" + Source + ".ascx\" Scale=\"" + Scale + "\" class=\"xic_map" + cssclass + "\"><Areas Elements=\"" + IDs + "\" onclick=\"xic_selected(%ID%);\" href=\"#\" /></Map></Scene></XamlImageConverter>";
				Compiler.Compile(src);

				var div = new Panel();
				div.Style[HtmlTextWriterStyle.Position] = "absolute";
				var div2 = new Panel();
				div2.Style[HtmlTextWriterStyle.Position] = "relative; top=" + LegendTop.ToString() + "left=" + LegendLeft.ToString();

				div.Controls.Add(div2);
				div2.Controls.Add(boxlist);

				var img = new Image();
				img.ImageUrl = Source + ".png";
				var submit = new Button();
				submit.Text = SubmitText;
				submit.Click += (sender, args) => { if (Submit != null) Submit(sender, args); };
				div2.Controls.Add(submit);

				var map = LoadControl(Source + ".ascx");
				var box = new Panel();
				box.Controls.Add(img);
				box.Controls.Add(div);
				box.Controls.Add(map);

				Controls.Add(box);
			} else {
				var box = new Panel();

				if (string.IsNullOrEmpty(ID)) ID = "map." + (no++).ToString();
				var src = "<XamlImageConverter xmlns=\"http://schemas.johnshope.com/XamlImageConverter/2012\"><Scene Source=\"" + Source + "\"><Snapshot Type=\"png\" />" +
					"<Map File=\"" + Source + ".ascx\" Scale=\"" + Scale + "\" class=\"xic_map" + cssclass + "\"><Areas Elements=\"" + IDs + "\" onclick=\"_doPostBack('" + box.ClientID + "','%ID%');\" /></Map></Scene></XamlImageConverter>";
				Compiler.Compile(src);

				var img = new Image();
				img.ImageUrl  = Source + ".png";
				var map = LoadControl(Source + ".ascx");

				box.Controls.Add(img);
				box.Controls.Add(map);		
				
				Controls.Add(box);
			}

		}
	}

}