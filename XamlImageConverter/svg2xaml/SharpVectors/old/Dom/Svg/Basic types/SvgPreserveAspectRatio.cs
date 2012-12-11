using System;
using System.Text.RegularExpressions;

namespace SharpVectors.Dom.Svg
{
	public class SvgPreserveAspectRatio : ISvgPreserveAspectRatio
	{
		#region Static members
		private static Regex parCheck = new Regex("^(?<align>[A-Za-z]+)\\s*(?<meet>[A-Za-z]*)$");
		#endregion

		#region Constructors
		public SvgPreserveAspectRatio(string attr)
		{
			Match match = parCheck.Match(attr.Trim());
			if(match.Groups["align"].Success)
			{
				switch(match.Groups["align"].Value)
				{
					case "none":
						align = SvgPreserveAspectRatioType.None;
						break;
					case "xMinYMin":
						align = SvgPreserveAspectRatioType.XMinYMin;
						break;
					case "xMidYMin":
						align = SvgPreserveAspectRatioType.XMidYMin;
						break;
					case "xMaxYMin":
						align = SvgPreserveAspectRatioType.XMaxYMin;
						break;
					case "xMinYMid":
						align = SvgPreserveAspectRatioType.XMinYMid;
						break;
					case "xMaxYMid":
						align = SvgPreserveAspectRatioType.XMaxYMid;
						break;
					case "xMinYMax":
						align = SvgPreserveAspectRatioType.XMinYMax;
						break;
					case "xMidYMax":
						align = SvgPreserveAspectRatioType.XMidYMax;
						break;
					case "xMaxYMax":
						align = SvgPreserveAspectRatioType.XMaxYMax;
						break;
					default:
						align = SvgPreserveAspectRatioType.XMidYMid;
						break;
				}
			}
			else
			{
                align = SvgPreserveAspectRatioType.XMidYMid;
			}
					
			if(match.Groups["meet"].Success)
			{
				switch(match.Groups["meet"].Value)
				{
					case "slice":
						meetOrSlice = SvgMeetOrSlice.Slice;
						break;
					case "meet":
						meetOrSlice = SvgMeetOrSlice.Meet;
						break;
					default:
						meetOrSlice = SvgMeetOrSlice.Unknown;
						break;
				}
			}
			else
			{
				meetOrSlice = SvgMeetOrSlice.Meet;
			}
		}

		#endregion

		public double[] FitToViewBox(SvgRect viewBox, SvgRect rectToFit)
		{
			double translateX = 0;
			double translateY = 0;
			double scaleX = 1;
			double scaleY = 1;

			if(!viewBox.IsEmpty)
			{
				// calculate scale values for non-uniform scaling
				scaleX = rectToFit.Width / viewBox.Width;
				scaleY = rectToFit.Height / viewBox.Height;

				if(Align != SvgPreserveAspectRatioType.None)
				{
					// uniform scaling
					if(MeetOrSlice == SvgMeetOrSlice.Meet) scaleX = Math.Min(scaleX, scaleY);
					else scaleX = Math.Max(scaleX, scaleY);

					scaleY = scaleX;

					if(Align == SvgPreserveAspectRatioType.XMidYMax || 
						Align == SvgPreserveAspectRatioType.XMidYMid || 
						Align == SvgPreserveAspectRatioType.XMidYMin)
					{
						// align to the Middle X
						translateX = (rectToFit.Width - viewBox.Width * scaleX) / 2;
					}
					else if(Align == SvgPreserveAspectRatioType.XMaxYMax || 
						Align == SvgPreserveAspectRatioType.XMaxYMid || 
						Align == SvgPreserveAspectRatioType.XMaxYMin)
					{
						// align to the right X
						translateX = (rectToFit.Width - viewBox.Width * scaleX);
					}

					if(Align == SvgPreserveAspectRatioType.XMaxYMid || 
						Align == SvgPreserveAspectRatioType.XMidYMid || 
						Align == SvgPreserveAspectRatioType.XMinYMid)
					{
						// align to the Middle Y
						translateY = (rectToFit.Height - viewBox.Height * scaleY) / 2;
					}
					else if(Align == SvgPreserveAspectRatioType.XMaxYMax || 
						Align == SvgPreserveAspectRatioType.XMidYMax || 
						Align == SvgPreserveAspectRatioType.XMinYMax)
					{
						// align to the bottom Y
						translateY = (rectToFit.Height - viewBox.Height * scaleY);
					}
				}
			}

			return new double[]{
                (double) translateX,
                (double) translateY,
                (double) scaleX,
                (double) scaleY };
		}

		#region Implementation of ISvgPreserveAspectRatio
		private SvgPreserveAspectRatioType align;
		public SvgPreserveAspectRatioType Align
		{
			get
			{
				return align;
			}
		}

		private SvgMeetOrSlice meetOrSlice;
		public SvgMeetOrSlice MeetOrSlice
		{
			get
			{
				return meetOrSlice;
			}
		}
		#endregion
	}
}
