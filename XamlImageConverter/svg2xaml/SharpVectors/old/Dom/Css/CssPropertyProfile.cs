using System;
using System.Collections;

namespace SharpVectors.Dom.Css
{
	internal struct CssProperty
	{
		internal CssProperty(bool isInherited, string initialValue)
		{
			IsInherited = isInherited;
			InitialValue = initialValue;
			InitialCssValue = null;
		}
        internal bool IsInherited;
		internal string InitialValue;
		internal CssValue InitialCssValue;
	}

		public class CssPropertyProfile
		{
		#region Static fields
			private static CssPropertyProfile _svgProfile = null;
			public static CssPropertyProfile SvgProfile
			{
				get
				{
					if(_svgProfile == null)
					{
						_svgProfile = new CssPropertyProfile();
						_svgProfile.Add("alignment-baseline", false, String.Empty);
						_svgProfile.Add("baseline-shift", false, "baseline");
						_svgProfile.Add("clip", false, "auto");
						_svgProfile.Add("clip-path", false, "none");
						_svgProfile.Add("clip-rule", true, "nonzero");
						_svgProfile.Add("color", true, "black");
						_svgProfile.Add("color-interpolation", true, "sRGB");
						_svgProfile.Add("color-interpolation-filters", true, "linearRGB");
						_svgProfile.Add("color-profile", true, "auto");
						_svgProfile.Add("color-rendering", true, "auto");
						_svgProfile.Add("cursor", true, "auto");
						_svgProfile.Add("direction", true, "ltr");
						_svgProfile.Add("display", false, "inline");
						_svgProfile.Add("dominant-baseline", false, "auto");
						_svgProfile.Add("enable-background", false, "accumulate");
						_svgProfile.Add("fill", true, "black");
						_svgProfile.Add("fill-opacity", true, "1");
						_svgProfile.Add("fill-rule", true, "nonzero");
						_svgProfile.Add("filter", false, "none");
						_svgProfile.Add("flood-color", false, "black");
						_svgProfile.Add("flood-opacity", false, "1");
						_svgProfile.Add("font", true, String.Empty);
						_svgProfile.Add("font-family", true, "Arial");
						_svgProfile.Add("font-size", true, "medium");
						_svgProfile.Add("font-size-adjust", true, "none");
						_svgProfile.Add("font-stretch", true, "normal");
						_svgProfile.Add("font-style", true, "normal");
						_svgProfile.Add("font-variant", true, "normal");
						_svgProfile.Add("font-weight", true, "normal");
						_svgProfile.Add("glyph-orientation-horizontal", true, "0deg");
						_svgProfile.Add("glyph-orientation-vertical", true, "auto");
						_svgProfile.Add("image-rendering", true, "auto");
						_svgProfile.Add("kerning", true, "auto");
						_svgProfile.Add("letter-spacing", true, "normal");
						_svgProfile.Add("lighting-color", false, "white");
						_svgProfile.Add("marker", true, "none");
						_svgProfile.Add("marker-end", true, "none");
						_svgProfile.Add("marker-mid", true, "none");
						_svgProfile.Add("marker-start", true, "none");
						_svgProfile.Add("mask", false, "none");
						_svgProfile.Add("opacity", false, "1");
						_svgProfile.Add("overflow", false, String.Empty);
						_svgProfile.Add("pointer-events", true, "visiblePainted");
						_svgProfile.Add("shape-rendering", true, "auto");
						_svgProfile.Add("stop-color", false, "black");
						_svgProfile.Add("stop-opacity", false, "1");
						_svgProfile.Add("stroke", true, "none");
						_svgProfile.Add("stroke-dasharray", true, "none");
						_svgProfile.Add("stroke-dashoffset", true, "0");
						_svgProfile.Add("stroke-linecap", true, "butt");
						_svgProfile.Add("stroke-linejoin", true, "miter");
						_svgProfile.Add("stroke-miterlimit", true, "4");
						_svgProfile.Add("stroke-opacity", true, "1");
						_svgProfile.Add("stroke-width", true, "1");
						_svgProfile.Add("text-anchor", true, "start");
						_svgProfile.Add("text-decoration", false, "none");
						_svgProfile.Add("text-rendering", true, "auto");
						_svgProfile.Add("unicode-bidi", false, "normal");
						_svgProfile.Add("visibility", true, "visible");
						_svgProfile.Add("word-spacing", true, "normal");
						_svgProfile.Add("writing-mode", true, "lr-tb");
					}

					return _svgProfile;
				}
			}

		#endregion

			private Hashtable properties = new Hashtable();

			public CssPropertyProfile()
			{

			}

			public ICollection GetAllPropertyNames()
			{
				return properties.Keys;
			}
			public string GetInitialValue(string propertyName)
			{
				if(properties.ContainsKey(propertyName))
				{
                    return ((CssProperty)properties[propertyName]).InitialValue;
				}
				else
				{
					return null;
				}
			}

			public CssValue GetInitialCssValue(string propertyName)
			{
				if(properties.ContainsKey(propertyName))
				{
					CssProperty cssProp = (CssProperty)properties[propertyName];
					if(cssProp.InitialCssValue == null)
					{
						cssProp.InitialCssValue = CssValue.GetCssValue(cssProp.InitialValue, false);
					}
					return cssProp.InitialCssValue;
				}
				else
				{
					return null;
				}
			}

			public bool IsInherited(string propertyName)
			{
				if(properties.ContainsKey(propertyName))
				{
					return ((CssProperty)properties[propertyName]).IsInherited;
				}
				else
				{
					return true;
				}
			}


			public void Add(string propertyName, bool isInherited, string initialValue)
			{
				properties.Add(propertyName, new CssProperty(isInherited, initialValue));
			}

			public int Length
			{
				get
				{
					return properties.Count;
				}
			}


		}
	}
