
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Threading;
using System.Linq;

namespace XamlImageConverter {

	public static class Capture {
		internal static double defaultDpi = 96;

		/// <summary>
		/// Save snapshots of a UIElement with an optional clipping rectangle
		/// </summary>
		/// <param name="snapshot">Snapshot definition</param>
		/// <returns>Returns the captured <see cref="BitmapSource"/>s
		public static IEnumerable<BitmapSource> GetBitmaps(Snapshot snapshot) {
			var rootElement = (snapshot.Scene ?? snapshot).Element as FrameworkElement;

			if ((!snapshot.Frames.HasValue || snapshot.Frames.Value >= 1) && snapshot.Storyboard != null && rootElement != null) {
				var storyboard = snapshot.Storyboard;

				if (storyboard != null) {
					storyboard.Begin(rootElement, true);
					var clock = storyboard.CreateClock(false);
					var duration = clock.NaturalDuration.TimeSpan.TotalMilliseconds;

					var frames = Math.Max(snapshot.Frames ?? 1, 1);
					var step = (frames > 1) ? duration / (frames - 1) : 0;
					var time = (frames > 1) ? 0d : duration;

					for (var index = 0; index < frames; ++index, time += step) {
						storyboard.SeekNow(rootElement, time);
						yield return GetBitmap(snapshot);
					}

					storyboard.SeekNow(rootElement, 0);
					storyboard.Stop(rootElement);

					yield break;
				}
			}

			yield return GetBitmap(snapshot);
		}

		/// <summary>
		/// Seek the Storyboard to the specified time and update the entire layout
		/// </summary>
		/// <param name="storyboard">Storyboard to seek</param>
		/// <param name="element">Root element for layout update</param>
		/// <param name="time">Time to seek (relative to beginning time of storyboard)</param>
		private static void SeekNow(this Storyboard storyboard, FrameworkElement element, double time) {
			storyboard.SeekAlignedToLastTick(element, TimeSpan.FromMilliseconds(time), TimeSeekOrigin.BeginTime);
			element.UpdateLayout();
		}

		/// <summary>
		/// Save a snapshot of a UIElement with an optional clipping rectangle
		/// </summary>
		/// <param name="element">Element to render</param>
		/// <param name="clipRect">Clipping rectangle (optional)</param>
		/// <returns>Returns a <see cref="BitmapSource"/> containing the snapshot</returns>
		public static BitmapSource GetBitmap(Snapshot snapshot) {
			using (snapshot.ApplyStyleWithLock()) {
				var scene = snapshot.Scene;
				var window = snapshot.Window;

				var element = scene.Element;
				var dpi = (snapshot.Dpi ?? defaultDpi) * (snapshot.Scale ?? 1);

				var bitmap = new RenderTargetBitmap(
						(int)Math.Round(window.ActualWidth * dpi / defaultDpi),
						(int)Math.Round(window.ActualHeight * dpi / defaultDpi),
						dpi, dpi, PixelFormats.Pbgra32);

				if (element != null) {
					var T = element.RenderTransform;
					element.RenderTransform = new MatrixTransform(Matrix.Multiply(T.Value, snapshot.Transform.Value)); ;
					element.MeasureAndArrange(new Size(element.ActualWidth, element.ActualHeight));
					bitmap.Render(element);
					element.RenderTransform = T;
				} else System.Diagnostics.Debugger.Break();

				return bitmap;
			}
		}

		/// <summary>
		/// Encode bitmap(s) to a file
		/// </summary>
		/// <param name="encoder">Bitmap encoder</param>
		/// <param name="filename">Target filename</param>
		public static void Save(this BitmapEncoder encoder, string filename) {
			using (Stream stream = File.Create(filename)) {
				encoder.Save(stream);
			}
		}
	}
}