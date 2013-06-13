using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XamlImageConverter {

	public interface IPostProcessor {
		IEnumerable<SnapshotImage> Process( IEnumerable<SnapshotImage> images, CaptureSettings settings );
		IEnumerable<string> Close( CaptureSettings settings );
	}

}