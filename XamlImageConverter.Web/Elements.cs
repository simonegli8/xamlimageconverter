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
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public List<Scene> Children { get; set; }
		public string Theme { get; set; }
		public string Skin { get; set; }
		public string TextMode { get; set; }
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
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
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
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
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
		public string Type { get; set; }
		public string Assembly { get; set; }
		public bool Dynamic { get; set; }
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public List<object> Content { get; set; }
	}

	public class Snapshot : Group {

		public string Storyboard { get; set; }
		public int Frames { get; set; }
		public bool Filmstrip { get; set; }
		public double Dpi { get; set; }
		public int? Quality { get; set; }
		public string File { get; set; }
		public string Page { get; set; }
		public bool FitToPage { get; set; }
		public int Loop { get; set; }
		public double Pause { get; set; }
		public string Theme { get; set; }
		public string Skin { get; set; }
		public string TextMode { get; set; }
	}


	[ContentProperty("Areas")]
	[DefaultProperty("Areas")]
	public class ImageMap : System.Web.UI.WebControls.ImageMap {
		public enum FileTypes { UserControl, IncludeFile, Insert };
		public enum Types { AspNet, Html };
		public enum IdentChars { Tab, Space };
		public string Image { get; set; }
		public double Scale { get; set; }
		public string File { get; set; }
		public Types Type { get; set; }
		public double Flattness { get; set; }
		public FileTypes FileType { get; set; }
		public IdentChars Ident { get; set; }

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
		public string Theme { get; set; }
		public string Skin { get; set; }
		public string TextMode { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public List<Area> Areas { get; set; }
	}

	[ContentProperty("Areas")]
	[DefaultProperty("Areas")]
	public class Map : Group {
		public string Image { get; set; }
		public double Scale { get; set; }
		public string File { get; set; }
		public ImageMap.Types Type { get; set; }
		public double Flattness { get; set; }
		public ImageMap.FileTypes FileType { get; set; }
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public List<Area> Areas { get; set; }
	}

	public class Area : System.Web.UI.WebControls.HotSpot {
		public string Element { get; set; }
		public override string GetCoordinates() { throw new NotImplementedException(); }
		protected override string MarkupName { get { throw new NotImplementedException(); } }

		public string href { get; set; }
		public string onclick { get; set; }
		public string onmouseout { get; set; }
		public string onmouseover { get; set; }
		public string alt { get; set; }
		public string title { get; set; }
	}

	public class Areas : System.Web.UI.WebControls.HotSpot {
		public string Elements { get; set; }
		public override string GetCoordinates() { throw new NotImplementedException(); }
		protected override string MarkupName { get { throw new NotImplementedException(); } }

		public string href { get; set; }
		public string onclick { get; set; }
		public string onmouseout { get; set; }
		public string onmouseover { get; set; }
		public string alt { get; set; }
		public string title { get; set; }
	}

	[ContentProperty("Setters")]
	[DefaultProperty("Setters")]
	public class Set : ISceneElement {
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		List<object> Setters { get; set; }
	}

	public class Undo : ISceneElement {
	}

	public class Reset : ISceneElement {
	}

}
