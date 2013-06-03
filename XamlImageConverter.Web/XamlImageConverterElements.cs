using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Markup;

[assembly: System.Windows.Markup.XmlnsDefinition("http://schemas.johnshope.com/XamlImageConverter/2012", "XamlImageConverter.Elements")]

namespace XamlImageConverter.Elements {

	[ContentProperty("Children")]
	[DefaultProperty("Children")]
	public class XamlImageConverter {
		public XamlImageConverter() : base() { }
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]	
		public List<Scene> Children { get; set; }
		[Description("The OS Theme to use for rendering. One of Aero, Aero2, AeroLite, Royale, Classic, Luna, Luna.Metallic, Luna.Homestead.")]
		public string Theme { get; set; }
		public string Skin { get; set; }
		public string TextMode { get; set; }
		public string OutputPath { get; set; }
	}

	[ContentProperty("Children")]
	[DefaultProperty("Children")]
	public class Scene {
		public double Width { get; set; }
		public double Height { get; set; }
		public string Cultures { get; set; }
		public string OutputPath { get; set; }
		public string Type { get; set; }
		public string Source { get; set; }
		public string File { get; set; }
		public string Assembly { get; set; }
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public List<ISceneElement> Children { get; set; }
		public string Theme { get; set; }
		public string Skin { get; set; }
		public string TextMode { get; set; }
	}

	public interface ISceneElement { }

	[ContentProperty("Children")]
	[DefaultProperty("Children")]
	public class Group : ISceneElement {
		public double Left { get; set; }
		public double Right { get; set; }
		public double Top { get; set; }
		public double Bottom { get; set; }
		public double Width { get; set; }
		public double Height { get; set; }
		public string Element { get; set; }
		public string OutputPath { get; set; }
		public DateTime Version { get; set; }
		public string Cultures { get; set; }
		public string Culture { get; set; }
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public List<ISceneElement> Children { get; set; }
		public string Theme { get; set; }
		public string Skin { get; set; }
		public string TextMode { get; set; }
	}

	[ContentProperty("Content")]
	[DefaultProperty("Content")]
	public class Xaml : ISceneElement {
		public string File { get; set; }
		public string Source { get; set; }
		public string Image { get; set; }
		public string Type { get; set; }
		public string Assembly { get; set; }
		public bool Dynamic { get; set; }
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public List<object> Content { get; set; }
	}

	public class Snapshot : Group, ISceneElement {
		[Description("The Storyboard for an animation.")]
		public string Storyboard { get; set; }
		public int Frames { get; set; }
		public bool Filmstrip { get; set; }
		public double Dpi { get; set; }
		public int? Quality { get; set; }
		public string File { get; set; }
		public string Filename { get; set; }
		public string Image { get; set; }
		public string Page { get; set; }
		public bool FitToPage { get; set; }
		public int Loop { get; set; }
		public double Pause { get; set; }
		public string Theme { get; set; }
		public string Skin { get; set; }
		public string TextMode { get; set; }
		public int? Layer { get; set; }
	}

	public interface IArea { }

	[ContentProperty("Areas")]
	[DefaultProperty("Areas")]
	public class ImageMap : System.Web.UI.WebControls.ImageMap, ISceneElement {
		public enum FileTypes { UserControl, IncludeFile, Insert };
		public enum Types { AspNet, Html };
		public enum IdentChars { Tab, Space };
		public string Image { get; set; }
		public string File { get; set; }
		public Types Type { get; set; }
		public double Flatness { get; set; }
		public FileTypes FileType { get; set; }
		public IdentChars Ident { get; set; }

		public double XOffset { get; set; }
		public double YOffset { get; set; }
		public double Scale { get; set; }
		public double XScale { get; set; }
		public double YScale { get; set; }
		public double Angle { get; set; }

		public string OutputPath { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public List<IArea> Areas { get; set; }
	}

	[ContentProperty("Areas")]
	[DefaultProperty("Areas")]
	public class Map : Group, ISceneElement {
		public string Image { get; set; }
		public string File { get; set; }
		public ImageMap.Types Type { get; set; }
		public double Flatness { get; set; }
		public ImageMap.FileTypes FileType { get; set; }
		public ImageMap.IdentChars Ident { get; set; }
		public string onclick { get; set; }
		public string onmouseout { get; set; }
		public string onmouseover { get; set; }
		public string style { get; set; }
		public string id { get; set; }
		public string ID { get; set; }
		public string name { get; set; }
		//public string class { get; set; }

		public double XOffset { get; set; }
		public double YOffset { get; set; }
		public double Scale { get; set; }
		public double XScale { get; set; }
		public double YScale { get; set; }
		public double Angle { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public List<IArea> Areas { get; set; }
	}

	public class Area : IArea {
		public string Element { get; set; }
		public string Elements { get; set; }

		public string href { get; set; }
		public string onclick { get; set; }
		public string onmouseout { get; set; }
		public string onmouseover { get; set; }
		public string alt { get; set; }
		public string title { get; set; }
		public string style { get; set; }
		public string id { get; set; }
		//public string class { get; set; }
	}
	public class Areas : Area, IArea { }

	public class HotSpot : System.Web.UI.WebControls.HotSpot, IArea {
		public string Element { get; set; }
		public string Elements { get; set; }

		public override string GetCoordinates() {
			throw new NotImplementedException();
		}

		protected override string MarkupName {
			get { throw new NotImplementedException(); }
		}
	}
	public class HotSpots : HotSpot, IArea { }

	[ContentProperty("Setters")]
	[DefaultProperty("Setters")]
	public class Set : ISceneElement {
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		List<object> Setters { get; set; }
	}

	public class Undo : ISceneElement {
	}

	public class Reset : ISceneElement {
	}

}