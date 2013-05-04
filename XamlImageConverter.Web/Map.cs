using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Silversite;
using Silversite.Web.UI;
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

			if (Mode == Modes.Select) {
				var boxlist = new CheckBoxList();
				int n = 0;
				if (string.IsNullOrEmpty(IDs)) IDs = Regions;
				
				var ids = IDs.Tokens();
				foreach (var region in Regions.Tokens()) {
					var listItem = new ListItem();
					listItem.Attributes["id"] = "checkbox_" + IDs;
					boxlist.Items.Add(new ListItem(region, n.ToString()));
				}

				var script = new Script();
				script.Text = "var selected function(id) { var box = $('#checkbox_' + id); box.prop(\"checked\", !box.attr(\"checked\")); };";
				var src = "<xic:XamlImageProvider xmlns=\"http://schemas.johnshope.com/XamlImageConverter/2012\"><xic:Scene Source=\"" + Source + "\"><xic:Snapshot Type=\"png\" >" +
					"<xic:Map File=\"" + Source + ".ascx\" Scale=\"" + Scale + "\"><xic:Areas Elements=\"" + IDs + "\" onclick=\"selected(%ID%);\" href=\"#\" /></xic:Map>";
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
				var src = "<xic:XamlImageProvider xmlns=\"http://schemas.johnshope.com/XamlImageConverter/2012\"><xic:Scene Source=\"" + Source + "\"><xic:Snapshot Type=\"png\" >" +
					"<xic:Map File=\"" + Source + ".ascx\" Scale=\"" + Scale + "\" ><xic:Areas Elements=\"" + IDs + "\" onclick=\"_doPostBack('" + box.ClientID + "','%ID%');\" /></xic:Map>";
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