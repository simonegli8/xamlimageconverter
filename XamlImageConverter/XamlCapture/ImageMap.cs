using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;
using System.Text.RegularExpressions;

namespace XamlImageConverter {

	public class AttributeCollection: KeyedCollection<string, XAttribute> {
		protected override string GetKeyForItem(XAttribute a) { return a.Name.ToString(); }
		public AttributeCollection() : base(StringComparer.OrdinalIgnoreCase) { }
	}
	
	public class ImageMap: Group, Step {

		public enum FileTypes { UserControl, IncludeFile, Insert };

		public enum Types { AspNet, Html };
		public enum IdentChars { Tab, Space };

		public string Image { get; set; }
		public double Scale { get; set; }
		public string File { get { return Filename; } set { Filename = value; } }
		public Types Type { get; set; }
		public double Flattness { get; set; }
		public FileTypes FileType { get; set; }
		public IdentChars Ident { get; set; }
		public double Dpi { get; set; }

		AttributeCollection attributes = new AttributeCollection();
		public AttributeCollection Attributes { get { return attributes; } }

		List<XElement> areas = new List<XElement>();
		public List<XElement> Areas { get { return areas; } }

		XElement Map;

		public ImageMap() {
			Flattness = 0.5;
			FileType = FileTypes.UserControl;
			Scale = 1;
		}

		public override bool ParseChildren { get { return false; } }

		public Snapshot Snapshot { get { return Scene.Steps().OfType<Snapshot>().FirstOrDefault(s => s.Filename == Image); } }

		private enum Shapes { Rectangle, Circle, Polygon };

		// format points to a comma separated list string
		private string Coordinates(params double[] coords) {
			var s = new StringBuilder();
			int n = 0;
			foreach (var p in coords) {
				if (n++ > 0) s.AppendFormat(",{0:D}", (int)(p+0.5));
				else s.AppendFormat("{0:D}", (int)(p+0.5));
			}
			return s.ToString();
		}

		private void AddAreaAttributes(XElement area, string name, string id, XElement source) {
			var idattr = area.Attribute("id") ?? area.Attribute("ID");
			if (idattr != null) name = idattr.Value;
			if (Type == Types.AspNet) area.SetAttributeValue("ID", name);
			else area.SetAttributeValue("id", name);
			var reserverd = new string[] { "element", "elements", "id" };
			foreach (var a in source.Attributes().Where(a => reserverd.All(r => a.Name.ToString().ToLower() != r))) area.SetAttributeValue(a.Name, a.Value.Replace("%ID%", id));
		}

		private void AddArea(Shapes type, XElement source, string name, ref int n, params double[] coords) {

			var id = name ?? ("area" + n);

			for (int i = 0; i < coords.Length; i++) coords[i] *= Scale;

			if (n++ > 0) name = id + "." + n;

			// create XElement for area element
			XElement area = null;
			if (Type == Types.AspNet) {
				switch (type) {
					case Shapes.Circle:
						area = new XElement("_asp_CircleHotSpot");
						AddAreaAttributes(area, name, id, source);
						area.SetAttributeValue("X", coords[0].ToString("D"));
						area.SetAttributeValue("Y", coords[1].ToString("D"));
						area.SetAttributeValue("Radius", coords[2].ToString("D"));
						break;
					case Shapes.Rectangle:
						area = new XElement("_asp_RectangleHotSpot");
						AddAreaAttributes(area, name, id, source);
						area.SetAttributeValue("Left", coords[0].ToString("D"));
						area.SetAttributeValue("Top", coords[1].ToString("D"));
						area.SetAttributeValue("Right", coords[2].ToString("D"));
						area.SetAttributeValue("Bottom", coords[3].ToString("D"));
						break;
					case Shapes.Polygon:
						area = new XElement("_asp_PolygonHotSpot");
						AddAreaAttributes(area, name, id, source);
						area.SetAttributeValue("Coordiantes", Coordinates(coords));
						break;
				}
				area.SetAttributeValue("runat", "server");
			} else {
				area = new XElement("area");
				AddAreaAttributes(area, name, id, source);
				switch (type) {
					case Shapes.Circle: area.SetAttributeValue("shape", "circle"); break;
					case Shapes.Rectangle: area.SetAttributeValue("shape", "rect"); break;
					case Shapes.Polygon: area.SetAttributeValue("shape", "poly"); break;
				}
				area.SetAttributeValue("coords", Coordinates(coords));
			}
			Map.Add(area);
		}

		private void AddRectangleArea(XElement source, string name, ref int n, double Left, double Top, double Right, double Bottom) {
			AddArea(Shapes.Rectangle, source, name, ref n, Left, Top, Right, Bottom);
		}

		private void AddPolygonArea(XElement source, string name, ref int n, params double[] coords) {
			if (coords.Length < 6) throw new ArgumentException("A polygon must consist of at least 3 points.");
			if (coords[0] != coords[coords.Length-2] || coords[1] != coords[coords.Length-1]) { // append first point at end.
				coords = coords.Append(coords[0], coords[1]).ToArray();
			}
			AddArea(Shapes.Polygon, source, name, ref n, coords);
		}

		private void AddCircleArea(XElement source, string name, ref int n, double X, double Y, double Radius) {
			AddArea(Shapes.Circle, source, name, ref n, X, Y, Radius);
		}

		private class Renderer: UIElement {

			List<Geometry> Geometries = new List<Geometry>();

			private void AddGeometry(DrawingGroup group, Geometry g) {
				if (group.ClipGeometry != null && !group.ClipGeometry.FillContains(g)) {
					var cg = new CombinedGeometry();
					cg.Geometry1 = group.ClipGeometry;
					cg.Geometry2 = g;
					cg.GeometryCombineMode = GeometryCombineMode.Intersect;
					g = cg.GetOutlinedPathGeometry(Flattness, ToleranceType.Relative);
				}

				var intersections = Geometries.Where(gg => gg.FillContainsWithDetail(g) != IntersectionDetail.Empty);
				foreach (var ig in intersections) {
					Geometries.Remove(ig);
					var cg = new CombinedGeometry();
					cg.Geometry1 = g;
					cg.Geometry2 = ig;
					cg.GeometryCombineMode = GeometryCombineMode.Union;
					g = cg.GetOutlinedPathGeometry(Flattness, ToleranceType.Relative);
				}
				Geometries.Add(g);
			}

			private void ApplyTransform(Transform t, DependencyObject d) {
				if (t != null && t.Value != Matrix.Identity) {
					if (d is Geometry) {
						var g = (Geometry)d;
						if (g.Transform == null || g.Transform.Value == Matrix.Identity) g.Transform = t;
						else g.Transform = new MatrixTransform(Matrix.Multiply(t.Value, g.Transform.Value));
					} else if (d is DrawingGroup) {
						var dg = (DrawingGroup)d;
						if (dg.Transform == null || dg.Transform.Value == Matrix.Identity) dg.Transform = t;
						else dg.Transform = new MatrixTransform(Matrix.Multiply(t.Value, dg.Transform.Value));
					}
				}
			}

			private void AnalyzeGeometry(DrawingGroup group, Geometry g, Brush brush, Pen pen) {
				g = g.Clone();
				ApplyTransform(group.Transform, g);

				if (brush != null) {
					var eg = g as EllipseGeometry;
					var rg = g as RectangleGeometry;
					if (g is EllipseGeometry && eg.RadiusX == eg.RadiusY && g.Transform.Value.M12 == 0.0 && g.Transform.Value.M21 == 0.0) { // circle
						if (pen != null && pen.Thickness != 0) {
							eg.RadiusX += Math.Abs(pen.Thickness);
							eg.RadiusY += Math.Abs(pen.Thickness);
						}
						pen = null;
						AddGeometry(group, g);
					} else if (g is RectangleGeometry && (pen == null || Math.Abs(pen.Thickness) <= 1) && g.Transform.Value.M12 == 0.0 && g.Transform.Value.M21 == 0.0) { // rectangle
						pen = null;
						AddGeometry(group, g);
					} else { // other
						AddGeometry(group, g.GetOutlinedPathGeometry(Flattness, ToleranceType.Relative));
					}
				}
				if (pen != null && pen.Thickness > 1 ) {
					AddGeometry(group, g.GetWidenedPathGeometry(pen, Flattness, ToleranceType.Relative));
				}
			}

			private void AnalyzeRect(DrawingGroup group, Rect rect) {
				var rg = new RectangleGeometry();
				rg.Rect = rect;
				var drawing = new GeometryDrawing();
				drawing.Geometry = rg;
				group.Children.Add(drawing);
				AnalyzeGeometry(group, rg, null, null);
			}

			private void AnalyzeDrawingGroup(DrawingGroup group) {
				if (group == null) return;

				foreach (var drawing in group.Children) {
					if (drawing is ImageDrawing || drawing is VideoDrawing || drawing is GlyphRunDrawing) {
						AnalyzeRect(group, drawing.Bounds);
					} else if (drawing is GeometryDrawing) {
						var gd = (GeometryDrawing)drawing;
						AnalyzeGeometry(group, gd.Geometry, gd.Brush, gd.Pen);
					} else if (drawing is DrawingGroup) {
						ApplyTransform(group.Transform, drawing);
						AnalyzeDrawingGroup((DrawingGroup)drawing);
					}
				}
			}

			public IEnumerable<Geometry> Areas() {
				var group = new DrawingGroup();
				if (Element is Drawing) group.Children.Add((Drawing)Element);
				else if (Element is UIElement) {
					//((UIElement)Element).OnRender(group.Open());
					var type = Element.GetType();
					var onRender = type.GetMethod("OnRender", System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
					onRender.Invoke(Element, new object[] { group.Open() });
				} else Errors.Error(string.Format("Unsupported Elementtype {0}.", Element.GetType().FullName), "27", Area);

				AnalyzeDrawingGroup(group);
				return Geometries;
			}

			DependencyObject Element;
			XElement Area;
			Errors Errors;
			double Flattness;

			public Renderer(ImageMap map, DependencyObject element, XElement area) {
				Errors = map.Errors;
				Flattness = map.Flattness;
				Element = element;
				Area = area;
			}
		}

		private void ProcessArea(DependencyObject element, XElement area, string name, ref int n) {
			var renderer = new Renderer(this, element, area); // create a renderer that analyzes all shapes
			foreach (var a in renderer.Areas()) { // proccess analyzed geometries.
				var g = a;
				var m = Matrix.Identity;
				if (g.Transform != null) m = g.Transform.Value;
				m.Scale(Scale, Scale);
				var t = new MatrixTransform(m);
				if (g is RectangleGeometry) { // rectangle
					var r = ((RectangleGeometry)g).Rect;
					var topLeft = t.Transform(r.TopLeft);
					var bottomRight = t.Transform(r.BottomRight);
					AddRectangleArea(area, name, ref n, topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
				} else if (g is EllipseGeometry) { // circle
					var eg = (EllipseGeometry)g;
					var center = t.Transform(eg.Center);
					var radius = eg.RadiusX * t.Value.M11;
					AddCircleArea(area, name, ref n, center.X, center.Y, radius);
				} else if (g is StreamGeometry) { // stream geometry, convert to path
					var s = (StreamGeometry)g;
					g = s.GetFlattenedPathGeometry(Flattness, ToleranceType.Relative);
				}
				if (g is PathGeometry) { // path geometry
					var path = (PathGeometry)g;
					path = path.GetFlattenedPathGeometry(Flattness, ToleranceType.Relative);
					foreach (var figure in path.Figures) { // process all figures & segments
						var points = new PointCollection();
						foreach (var segment in figure.Segments) {
							if (segment is LineSegment) { // segment is a LineSegment
								var ls = (LineSegment)segment;
								points.Add(t.Transform(ls.Point)); // add point to points
							} else if (segment is PolyLineSegment) { // segment is a PolyLineSegment
								var pls = (PolyLineSegment)segment;
								foreach (var point in pls.Points) points.Add(t.Transform(point)); // add point to points 
							} else { // path should be a polygon
								throw new NotSupportedException("Internal error.");
							}
						}
						// convert points into an array of doubles
						var coords = new List<double>();
						foreach (var point in points) { coords.Add(point.X); coords.Add(point.Y); }

						AddPolygonArea(area, name, ref n, coords.ToArray());
					}
				}
			}
		}

		public void Save() {
			var str = new StringBuilder();

			if (FileType == FileTypes.UserControl) {
				str.Append("<%@ Control ");
				var res = new List<string>() { "AutoEventWireup", "ClassName", "ClientIDMode", "CodeBehind", "CodeFile", "CodeFileBaseClass", "CompilationMode", "CompilerOptions", "Debug", "EnableTheming",
					"EnableViewState", "Explicit", "Inherits", "Language", "LinePragmas", "Src", "Strict", "ViewStateMode", "WarningLevel" };
				foreach (var a in Attributes.ToList()) {
					if (res.Contains(a.Name.ToString())) {
						str.Append(a.Name);
						if (a.Value != null) {
							str.Append("=\"");
							str.Append(a.Value);
							str.Append("\" ");
						}
						Attributes.Remove(a);
					}
				}
				str.AppendLine("%>");
				str.AppendLine();
			}

			int nid = ((Image.GetHashCode() % 100) + 100) % 100;
			string id = null;
			var idattr = Attributes.FirstOrDefault(a => a.Name.ToString().ToLower() == "id");
			if (idattr == null) id = "map" + nid;
			else id = idattr.Value;
			Regex insertTag = null;
			if (Type == Types.AspNet) {
				Map = new XElement("_asp_ImageMap", new XAttribute("ID", id), new XAttribute("runat", "server"), new XAttribute("ImageUrl", Image));
				Attributes.Remove(idattr);
				Attributes.Remove("runat");
				Attributes.Remove("imageurl");
				if (FileType == FileTypes.Insert){
					insertTag = new Regex("<asp:ImageMap\\s[.\\n]*(id\\s*=\\s*(\"|')" + Regex.Escape(id) + "(\"|')).*((>[.\\n]*</asp:ImageMap>)|(/>))",
						RegexOptions.IgnoreCase | RegexOptions.Multiline);
				}
			} else {
				Map = new XElement("map", new XAttribute("id", id), new XAttribute("name", id));
				if (FileType == FileTypes.Insert) {
					insertTag = new Regex("<map\\s[.\\n]*(name\\s*=\\s*(\"|')" + Regex.Escape(id) + "(\"|')).*((>[.\\n]*</map>)|(/>))",
						RegexOptions.IgnoreCase | RegexOptions.Multiline);
				}
			}

			// copy all attributes to Map
			foreach (var a in Attributes) Map.SetAttributeValue(a.Name, a.Value);

			var s = Snapshot;
			//if (s == null) throw new CompilerException("ImageMap has no related Snapshot", 27, this.XElement);
			if (s != null) Dpi = s.Dpi ?? 96.0;
			Scale = Scale * Dpi / 96.0;

			FrameworkElement e = null;
			if (string.IsNullOrEmpty(s.ElementName)) e = s.Scene.Element;
			else e = s.Element;

			foreach (XElement child in Areas) {
				if (child.Name.LocalName.Equals("area", StringComparison.OrdinalIgnoreCase) || child.Name.LocalName.Equals("areas", StringComparison.OrdinalIgnoreCase)) {
					var elementAttr = child.Attribute("element") ?? child.Attribute("Element") ?? child.Attribute("elements") ?? child.Attribute("Elements");
					string elements = null;
					if (elementAttr == null) {
						if (child.Attribute("shape") == null && child.Attribute("coords") == null) elements = null;
						else {
							Map.Add(child);
							continue;
						}
					} else {
						elements = elementAttr.Value ?? "";
					}

					var list = new List<string>();
					if (elements == "*") list = e.FindAllNames().Distinct().ToList();
					else if (elements != null) list = elements.Split(';').ToList();

					if (elements != null) {
						foreach (var elementName in list) {
							DependencyObject element = e;
							if (elements != "") element = e.FindName<DependencyObject>(elementName);
							int n = 0;
							if (element != null) ProcessArea(element, child, elementName, ref n);
						}
					} else Map.Add(child);
				} else Map.Add(child);
			}

			var writerSettings = new XmlWriterSettings();
			writerSettings.Indent = true;
			writerSettings.IndentChars = Ident == IdentChars.Space ? "  " : "\t";
			writerSettings.Encoding = Encoding.UTF8;
			writerSettings.OmitXmlDeclaration = true;
			writerSettings.CloseOutput = true;
			writerSettings.CheckCharacters = true;
			string source = null;
			Match match = null;
			if (FileType == FileTypes.Insert) {
				source = System.IO.File.ReadAllText(LocalFilename);
				var identationsMatch = new Regex("($|\\n|\\n\\r|\\r\\n)(\\s*)");
				var identations = identationsMatch.Matches(source)
					.OfType<Match>()
					.Select(m => m.Groups[1].Value);
				if (identations.All(t => t[0] == '\t')) writerSettings.IndentChars = "\t";
				else {
					var lengths = identations.Select(t => t.Length).ToArray();
					int sum = 0, n = 0, avg = 2;
					for (int i = 1; i < lengths.Length; i++) {
						if (lengths[i] > lengths[i-1]) { sum += lengths[i] - lengths[i-1]; n++; }
					}
					if (n != 0) avg = sum / n;
					writerSettings.IndentChars = string.Concat(Enumerable.Repeat(' ', avg));
				}
				
				match = insertTag.Match(source);
				if (match.Success) {
					var previousNewline = source.LastIndexOf(Environment.NewLine, 0, match.Index);
					if (previousNewline == -1) writerSettings.NewLineChars = Environment.NewLine + source.Substring(0, match.Index);
					else writerSettings.NewLineChars = source.Substring(previousNewline, match.Index);
				} else {
					Errors.Error(string.Format("No image map with id {0} found in target {1}.", id, Path.GetFileName(Filename)), "29", XElement);
				}
			}
			if (match == null || match.Success) {

				using (var w = XmlWriter.Create(str, writerSettings)) Map.Save(w);

				str = str.Replace("_asp_", "asp:");

				if (FileType == FileTypes.Insert) {
					str.Insert(0, source.Substring(0, match.Index));
					str.Append(source.Substring(match.Index + match.Length));
				}

				System.IO.File.WriteAllText(LocalFilename, str.ToString(), Encoding.UTF8);

				Errors.Message("Created {0} html image map.", Path.GetFileName(Filename));
			}
		}

		/// <summary>
		/// Return this and all child snapshots
		/// </summary>
		/// <returns>Returns this and all child snapshots</returns>
		public override IEnumerable<Step> Flatten() {
			return new Step[] { this };
		}

	}
}
