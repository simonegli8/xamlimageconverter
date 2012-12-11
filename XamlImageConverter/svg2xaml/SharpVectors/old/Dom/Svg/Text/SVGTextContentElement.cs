using System;
using System.Diagnostics;
using System.Windows.Media;

using System.Text.RegularExpressions;
using System.Xml;
using System.Windows;
using System.Windows.Media.TextFormatting;
using System.Globalization;



namespace SharpVectors.Dom.Svg
{
	public enum SvgLengthAdjust
	{
		Unknown   = 0,
		Spacing     = 1,
		SpacingAndGlyphs     = 2
	}


	/// <summary>
	/// Summary description for SvgTextContentElement.
	/// </summary>
	public class SvgTextContentElement : SvgTransformableElement, ISharpPathGeometry, ISvgTextContentElement
	{
		internal SvgTextContentElement(string prefix, string localname, string ns, SvgDocument doc) : base(prefix, localname, ns, doc) 
		{
			svgExternalResourcesRequired = new SvgExternalResourcesRequired(this);
			svgTests = new SvgTests(this);
		}

		#region Implementation of ISvgExternalResourcesRequired
		private SvgExternalResourcesRequired svgExternalResourcesRequired;
		public ISvgAnimatedBoolean ExternalResourcesRequired
		{
			get
			{
				return svgExternalResourcesRequired.ExternalResourcesRequired;
			}
		}
		#endregion

		#region Implementation of ISvgTests
		private SvgTests svgTests;
		public ISvgStringList RequiredFeatures
		{
			get { return svgTests.RequiredFeatures; }
		}

		public ISvgStringList RequiredExtensions
		{
			get { return svgTests.RequiredExtensions; }
		}

		public ISvgStringList SystemLanguage
		{
			get { return svgTests.SystemLanguage; }
		}

		public bool HasExtension(string extension)
		{
			return svgTests.HasExtension(extension);
		}
        #endregion

		public ISvgAnimatedLength TextLength
		{
			get{throw new NotImplementedException();}
		}
		public ISvgAnimatedEnumeration LengthAdjust
		{
			get{throw new NotImplementedException();}
		}

		protected string TrimText(string val)
		{
			Regex tabNewline = new Regex(@"[\n\f\t]");
			if(this.XmlSpace != "preserve") val = val.Replace("\n", String.Empty);
			val = tabNewline.Replace(val, " ");

			if(this.XmlSpace == "preserve") return val;
			else return val.Trim();
		}

		public virtual string GetText(XmlNode child)
		{
			return this.TrimText(child.Value);
		}

		protected SvgTextElement OwnerTextElement
		{
			get
			{
				XmlNode node = (XmlNode) this;
				while(node != null)
				{
					if(node is SvgTextElement) return (SvgTextElement) node;
					node = node.ParentNode;
				}
				return null;
			}
		}

		public void Invalidate()
		{
		}

        public virtual Geometry GetGeometry()
		{
			Geometry gp = this.OwnerTextElement.GetGeometry();
			return gp;
		}

		protected void AddGraphicsPath(ref Point ctp, string text, GeometryGroup gp)
		{
            
            double emSize = _getComputedFontSize();
            FontFamily family = _getGDIFontFamily(emSize);
			//int style = _getGDIStyle();

            FontStyle fs = FontStyles.Normal;
            if(GetPropertyValue("font-style")=="italic")
			{
				fs = FontStyles.Italic;
			}

            FlowDirection fd = FlowDirection.LeftToRight;
            string dir = GetPropertyValue("direction");
            if (dir == "rtl")
                fd = FlowDirection.RightToLeft;

            FontWeight fw = FontWeights.Normal;

            string fontWeight = GetPropertyValue("font-weight");

			if(fontWeight == "bolder")
                fw = FontWeights.ExtraBold;
            if(fontWeight == "light")
                fw = FontWeights.Light;
            if(fontWeight == "lighter")
                fw = FontWeights.Thin;

            if(fontWeight == "100")
                fw = FontWeights.Thin;
            if(fontWeight == "200")
                fw = FontWeights.ExtraLight;
            if(fontWeight == "300")
                fw = FontWeights.Light;
            if(fontWeight == "400")
                fw = FontWeights.Regular;
            if(fontWeight == "500")
                fw = FontWeights.Medium;
            if(fontWeight == "600")
                fw = FontWeights.DemiBold;
            if(fontWeight == "700"
                || fontWeight == "bold")
                fw = FontWeights.Bold;
            if(fontWeight == "800"
                || fontWeight == "bolder")
                fw = FontWeights.ExtraBold;
            if(fontWeight == "900")
                fw = FontWeights.Heavy;


            System.Windows.Media.Typeface tf = new System.Windows.Media.Typeface(family, fs, fw, FontStretches.Normal);
            FormattedText ft = new FormattedText(text, CultureInfo.CurrentCulture, fd, tf, emSize, Brushes.Black);
            gp.Children.Add(ft.BuildGeometry(ctp));
            //StringFormat sf = _getGDIStringFormat();

            //GraphicsPath gp2 = new GraphicsPath();
            //gp2.StartFigure();

            //double xCorrection = 0;
            //if(sf.Alignment == StringAlignment.Near) xCorrection = emSize * 1 /6;
            //else if(sf.Alignment == StringAlignment.Far) xCorrection = -emSize * 1 /6;
			
            //double yCorrection = (double)(family.GetCellAscent(FontStyle.Regular)) / (double)(family.GetEmHeight(FontStyle.Regular)) * emSize;

            //// TODO: font property

            //Point p = new Point(ctp.X-xCorrection, ctp.Y - yCorrection);

            //gp2.AddString(text, family, style, emSize, p, sf);
            //if(!gp2.GetBounds().IsEmpty)
            //{
            //    double bboxWidth = gp2.GetBounds().Width;
            //    if(sf.Alignment == StringAlignment.Center) bboxWidth /= 2;
            //    else if(sf.Alignment == StringAlignment.Far) bboxWidth = 0;

            //    ctp.X += bboxWidth + emSize/4;
            //}

            //gp.AddPath(gp2, false);
            //gp2.Dispose();
		}

		protected virtual GeometryGroup GetGeometry(ref Point ctp)
		{
            GeometryGroup gp = new GeometryGroup();

			if(this is SvgTextPositioningElement)
			{
				SvgTextPositioningElement tpElm = (SvgTextPositioningElement) this;
				ctp = this.GetCurrentTextPosition(tpElm, ctp);
			}
			string sBaselineShift = GetPropertyValue("baseline-shift").Trim();
			double shiftBy = 0;

			if(sBaselineShift.Length > 0)
			{
				SvgTextElement textElement = this as SvgTextElement;
				if(textElement == null)
				{
					textElement = (SvgTextElement)this.SelectSingleNode("ancestor::svg:text", this.OwnerDocument.NamespaceManager);
				}

				double textFontSize = textElement._getComputedFontSize();
				if(sBaselineShift.EndsWith("%"))
				{
					shiftBy = SvgNumber.ParseToFloat(sBaselineShift.Substring(0, sBaselineShift.Length-1)) / 100 * textFontSize;
				}
				else if(sBaselineShift == "sub")
				{
					shiftBy = -0.6F * textFontSize;
				}
				else if(sBaselineShift == "super")
				{
					shiftBy = 0.6F * textFontSize;
				}
				else if(sBaselineShift == "baseline")
				{
					shiftBy = 0;
				}
				else
				{
					shiftBy = SvgNumber.ParseToFloat(sBaselineShift);
				}
            }
			

			foreach(XmlNode child in this.ChildNodes)
			{
				if(child.NodeType == XmlNodeType.Text)
				{
					ctp.Y -= (double)shiftBy;
					this.AddGraphicsPath(ref ctp, GetText(child), gp);
					ctp.Y += (double)shiftBy;
				}
				else if(child is SvgTRefElement)
				{
					SvgTRefElement trChild = (SvgTRefElement) child;
					trChild.GetGeometry(ref ctp);
				}
				else if(child is SvgTextContentElement)
				{
					SvgTextContentElement tcChild = (SvgTextContentElement) child;
					tcChild.GetGeometry(ref ctp);
				}
			}
            return gp;
		}

		protected Point GetCurrentTextPosition(SvgTextPositioningElement posElement, Point p)
		{
			if(posElement.X.AnimVal.NumberOfItems>0)
			{
				p.X = (double)posElement.X.AnimVal.GetItem(0).Value;
			}
			if(posElement.Y.AnimVal.NumberOfItems>0)
			{
				p.Y = (double)posElement.Y.AnimVal.GetItem(0).Value;
			}
			if(posElement.Dx.AnimVal.NumberOfItems>0)
			{
				p.X += (double)posElement.Dx.AnimVal.GetItem(0).Value;
			}
			if(posElement.Dy.AnimVal.NumberOfItems>0)
			{
				p.Y += (double)posElement.Dy.AnimVal.GetItem(0).Value;
			}
			return p;
		}

        //private int _getGDIStyle()
        //{
        //    int style = (int)FontStyle.Regular;
        //    string fontWeight = GetPropertyValue("font-weight");
        //    if(fontWeight == "bold" || fontWeight == "bolder" || fontWeight == "600" || fontWeight == "700" || fontWeight == "800" || fontWeight == "900")
        //    {
        //        style = style | (int)FontStyle.Bold;
        //    }

        //    if(GetPropertyValue("font-style")=="italic")
        //    {
        //        style = style | (int)FontStyle.Italic;
        //    }

        //    string textDeco = GetPropertyValue("text-decoration");
        //    if(textDeco=="line-through")
        //    {
        //        style = style | (int)FontStyle.Strikeout;
        //    }
        //    else if(textDeco=="underline")
        //    {
        //        style = style | (int)FontStyle.Underline;
        //    }
        //    return style;
        //}

		private FontFamily _getGDIFontFamily(double fontSize)
		{
			string fontFamily = GetPropertyValue("font-family");
            try
            {
                return new FontFamily(fontFamily);
            }
            catch
            {
            }
            //string[] fontNames = fontNames = fontFamily.Split(new char[1]{','});

            //FontFamily family;

            //foreach(string fn in fontNames)
            //{
            //    try
            //    {                    
            //        //Fonts.SystemTypefaces.
            //        //string fontName = fn.Trim(new char[2]{' ','\''});
            //        //if(fontName == "serif") family = FontFamily.GenericSerif;
            //        //else if(fontName == "sans-serif") family = FontFamily.GenericSansSerif;
            //        //else if(fontName == "monospace") family = FontFamily.GenericMonospace;
            //        //else family = new FontFamily(fontName);		// Font(,fontSize).FontFamily;	

					
            //        return family;
            //    }
            //    catch
            //    {
            //    }
            //}

			// no known font-family was found => default to arial
			return new FontFamily("Arial");
		}

        //private StringFormat _getGDIStringFormat()
        //{

        //    StringFormat sf = new StringFormat();

        //    bool doAlign = true;
        //    if (this is SvgTSpanElement || this is SvgTRefElement)
        //    {
        //        SvgTextPositioningElement posElement = (SvgTextPositioningElement)this;
        //        if (posElement.X.AnimVal.NumberOfItems == 0) doAlign = false;
        //    }

        //    if (doAlign)
        //    {
        //        string anchor = GetPropertyValue("text-anchor");
        //        if (anchor == "middle") sf.Alignment = StringAlignment.Center;
        //        if (anchor == "end") sf.Alignment = StringAlignment.Far;
        //    }

        //    string dir = GetPropertyValue("direction");
        //    if (dir == "rtl")
        //    {
        //        if (sf.Alignment == StringAlignment.Far) sf.Alignment = StringAlignment.Near;
        //        else if (sf.Alignment == StringAlignment.Near) sf.Alignment = StringAlignment.Far;
        //        sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
        //    }

        //    dir = GetPropertyValue("writing-mode");
        //    if (dir == "tb")
        //    {
        //        sf.FormatFlags = sf.FormatFlags | StringFormatFlags.DirectionVertical;
        //    }

        //    sf.FormatFlags = sf.FormatFlags | StringFormatFlags.MeasureTrailingSpaces;


        //    return sf;
        //}

		private double _getComputedFontSize()
		{
			string str = GetPropertyValue("font-size");
			double fontSize = 12;
			if(str.EndsWith("%"))
			{
				// percentage of inherited value
			}
			else if(new Regex(@"^\d").IsMatch(str))
			{
				// svg length
				fontSize = (double)new SvgLength("font-size", this, SvgLengthDirection.Viewport, "10px").Value;
			}
			else if(str == "larger")
			{
			}
			else if(str == "smaller")
			{

			}
			else
			{
				// check for absolute value
			}

			return fontSize;
		}


		public double GetNumberOfChars (  )
		{
			return this.InnerText.Length;
		}

		public double GetComputedTextLength ()
		{
			throw new NotImplementedException();
		}

		public double GetSubStringLength (double charnum, double nchars )
		{
			throw new NotImplementedException();
			//raises( DOMException );
		}

		public ISvgPoint GetStartPositionOfChar (double charnum )
		{
			throw new NotImplementedException();
			//raises( DOMException );
		}

		public ISvgPoint GetEndPositionOfChar (double charnum )
		{
			throw new NotImplementedException();
			//raises( DOMException );
		}

		public ISvgRect GetExtentOfChar (double charnum )
		{
			throw new NotImplementedException();
			//raises( DOMException );
		}
		public double GetRotationOfChar (double charnum )
		{
			throw new NotImplementedException();
			//raises( DOMException );
		}
		public double GetCharNumAtPosition (ISvgPoint point )
		{
			throw new NotImplementedException();
		}
		public void SelectSubString (double charnum, double nchars )
		{
			throw new NotImplementedException();
			//raises( DOMException );
		}
	}
}
