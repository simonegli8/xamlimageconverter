using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace XamlImageConverter {

	public class Parameters : Group, Step {

		Dictionary<string, object> values = new Dictionary<string, object>();
		Dictionary<string, XObject> xobjects = new Dictionary<string, XObject>();
		public bool First = true;

		public Parameters() { }

		public Parameters(Dictionary<string, string> p) {
			foreach (var key in p.Keys) Add(key, p[key]);
		}

		public Parameters(string args)
			: this() {
			if (!string.IsNullOrEmpty(args)) {
				string[] pars = args.Split(';');
				foreach (string p in pars) {
					int nx = p.IndexOf('=');
					string name = p.Substring(0, nx).Trim();
					string val = p.Substring(nx + 1).Trim();
					Add(name, val);
				}
			}
		}

		private class LateValue {
			XElement element;

			public LateValue(XElement e) { element = e; }

			FrameworkElement xaml = null;
			public FrameworkElement Xaml {
				get {
					if (xaml == null) xaml = XamlReader.Load(element.Descendants().First().CreateReader()) as FrameworkElement;
					return xaml;
				}
			}
		}

		public Parameters(XElement element)
			: this() {
			foreach (XAttribute attribute in element.Attributes()) {
				Add(attribute.Name.LocalName, attribute.Value, attribute);
			}
			foreach (XElement child in element.Elements()) {
				if (child.HasElements == false) {
					Add(child.Name.LocalName, child.Value, child);
				} else if (child.Elements().Count() == 1) {
					Add(child.Name.LocalName, new LateValue(child), child);
				}
			}
		}

		public override bool ParseChildren { get { return false; } }

		public override bool NeedsBuilding { get { return false; } }

		public object this[string key] {
			get { return values[key]; }
			set { values[key] = value; }
		}

		public void Add(string key, object value) { values.Add(key, value); }
		public void Remove(string key) {
			if (values.ContainsKey(key)) values.Remove(key);
			if (xobjects.ContainsKey(key)) xobjects.Remove(key);
		}
		public void Add(string key, object value, XObject xobj, bool xaml = false) { xobjects.Add(key, xobj); values.Add(key, value); }


		public XObject XObject(string key) {
			return xobjects.ContainsKey(key) ? xobjects[key] : null;
		}

		static Dictionary<FrameworkElement, Stack<Parameters>> undoStack = new Dictionary<FrameworkElement, Stack<Parameters>>();
		public Stack<Parameters> UndoStack {
			get {
				lock (undoStack) {
					if (Scene == null) return null;
					if (!undoStack.ContainsKey(Scene.Element)) undoStack.Add(Scene.Element, new Stack<Parameters>());
					return undoStack[Scene.Element];
				}
			}
		}

		public bool Undo(FrameworkElement element) {
			if (UndoStack.Count == 0) {
				//Errors.Error("Undo: No corresponding Set statement", "30", this.XElement);
				return false;
			} else {
				var par = UndoStack.Pop();
				par.Apply(element);
				return true;
			}
		}

		public bool Undo() { return Undo(GetElement()); }

		public void Reset(FrameworkElement element) {
			while (Undo(element)) ;
		}

		public void Reset() { Reset(GetElement()); }

		public static void Split(string name, out string elementName, out string propertyName) {
			int pos = name.LastIndexOf('.');
			if (pos == -1) {
				elementName = null;
				propertyName = name;
			} else {
				elementName = name.Substring(0, pos);
				propertyName = name.Substring(pos + 1);
			}
		}

		public FrameworkElement Apply(FrameworkElement element) {
			var undo = new Parameters();
			undo.Compiler = Compiler;
			undo.First = First;
			foreach (var p in values) {
				string elemName, propName, depPropName = null;
				Split(p.Key, out elemName, out propName);
				if (!propName.EndsWith("Property")) depPropName = propName + "Property";
				else depPropName = propName;

				DependencyObject elem;
				if (string.IsNullOrEmpty(elemName)) elem = element;
				else elem = element.FindName(elemName) as DependencyObject;

				if (null != elem) {
					//Get the type of the current replacementElement
					System.Type t = elem.GetType();
					//Get the field info of the current replacementElement

					object value = p.Value;
					if (value is LateValue) value = ((LateValue)value).Xaml;
					Type vType = value.GetType();
					Type pType = null;
					DependencyProperty dp = null;
					bool isDependencyProperty;
					FieldInfo fieldInfo = t.GetField(depPropName, BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
					isDependencyProperty = fieldInfo != null;
					if (isDependencyProperty) {
						dp = (DependencyProperty)fieldInfo.GetValue(null);
						pType = dp.PropertyType;
					} else {
						fieldInfo = t.GetField(propName, BindingFlags.FlattenHierarchy | BindingFlags.Public);
						if (fieldInfo == null) {
							if (First) Errors.Error(string.Format("  Set {0}: Element {1} has no property {2}.", p.Key, elemName, propName), "3", XObject(p.Key));
						}
						pType = fieldInfo.GetType();
					}
					if (vType != pType && !vType.IsSubclassOf(pType)) {
						TypeConverter converter = System.ComponentModel.TypeDescriptor.GetConverter(pType);
						if (converter.CanConvertFrom(vType)) {
							if (value is string) value = converter.ConvertFromInvariantString(value as string);
							else value = converter.ConvertFrom(value);
						} else {
							if (First) Errors.Error(string.Format("  Set {0}: Cannot convert from {1} to {2}.", p.Key, vType.Name, pType.Name), "4", XObject(p.Key));
							continue;
						}
					}
					if (dp != null) {
						undo[p.Key] = elem.GetValue(dp);
						elem.SetValue(dp, value);
					} else {
						undo[p.Key] = fieldInfo.GetValue(elem);
						fieldInfo.SetValue(elem, value);
					}
					if (First) Errors.Message("  {0}={1}", p.Key, value.ToString());
				} else {
					if (First) Errors.Error(string.Format("  Set {0}: There is no element {1}.", p.Key, elemName), "5", XObject(p.Key));
				}
			}
			if (UndoStack != null) UndoStack.Push(undo);

			if (element == GetElement() && Scene != null) element.MeasureAndArrange(Scene.PreferredSize);
			else element.MeasureAndArrange(new Size(double.PositiveInfinity, double.PositiveInfinity));
			
			return element;
		}

		public FrameworkElement Apply() { return Apply(GetElement()); }

		public FrameworkElement GetElement() {
			return (Scene ?? this).Element as FrameworkElement;
		}

		public override void Process() {
			First = First && Compiler.Cpus == 1;
			if (First) Errors.Message("Set");
			Apply();
			First = false;
		}
	} 

	public class Undo: Parameters {
		public override void  Process() {
			First = First && Compiler.Cpus == 1;
			if (First) Errors.Message("Undo");
			Undo();
			First = false;
		}
	}

	public class Reset: Parameters {
		public override void  Process() {
			First = First && Compiler.Cpus == 1;
			if (First) Errors.Message("Reset");
			Reset();
			First = false;
		}
	}

}
