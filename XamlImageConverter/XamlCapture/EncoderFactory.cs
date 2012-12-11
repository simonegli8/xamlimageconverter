using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;

namespace XamlImageConverter {

	public interface IEncoder {
		void Save(string filename);
		IList<BitmapFrame> Frames { get; set; }
	}

	public class EncoderFactory {

		class Encoder: IEncoder {
			BitmapEncoder e;
			public Encoder(BitmapEncoder e) { this.e = e; }
			public void Save(string filename) { e.Save(filename); }
			public IList<BitmapFrame> Frames { get { return e.Frames; } set { e.Frames = value; } }
		}

		/// <summary>
		/// Create a <see cref="BitmapEncoder"/> based on the target file extension
		/// </summary>
		/// <param name="filename">Target filename</param>
		/// <param name="dpi">Dots per inch in the image</param>
		/// <returns>Returns a new <see cref="BitmapEncoder"/> based on the target file extension</returns>
		public static IEncoder Create(string filename, int? quality, double? dpi) {
			var ext = (filename.Contains('.') ? Path.GetExtension(filename) : "." + filename).ToLower();
			switch (ext) {
			case ".pdf":
			case ".eps":
			case ".ps":
				var enc = new ImageMagickEncoder();
				if (dpi.HasValue) enc.Options = "-density " + dpi.Value.ToString();
				else enc.Options = "-density 96";
				return enc;
			default:
				return new Encoder(CreateBitmapEncoder(ext, quality));
			}
		}

				/// <summary>
		/// Create a <see cref="BitmapEncoder"/> based on the target file extension
		/// </summary>
		/// <param name="filename">Target filename</param>
		/// <returns>Returns a new <see cref="BitmapEncoder"/> based on the target file extension</returns>
		public static BitmapEncoder CreateBitmapEncoder(string filename, int? quality) {
			var ext = (filename.Contains('.') ? Path.GetExtension(filename) : "." + filename).ToLower();
			switch (ext) {
			case ".jpeg":
			case ".jpg":
				return new JpegBitmapEncoder { QualityLevel = quality ?? 100 };
			case ".gif":
				return new GifBitmapEncoder();
			case ".tiff":
			case ".tif":
				return new TiffBitmapEncoder();
			case ".bmp":
				return new BmpBitmapEncoder();
			case ".wdp":
				return new WmpBitmapEncoder();
			default:
				return new PngBitmapEncoder();
			}
		}
	}
}
