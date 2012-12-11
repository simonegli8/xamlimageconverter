using System;
using System.Windows.Media;
using System.Windows;

namespace SharpVectors.Dom.Svg
{
	public interface ISharpMarkerHost
	{
		Point[] MarkerPositions{get;}
		double GetStartAngle(int index);
		double GetEndAngle(int index);
	}
}
