using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace XamlImageConverter
{
	public class PngHelper
	{
		// http://www.codeproject.com/KB/web-image/pnggammastrip.aspx?display=Print
		// http://morris-photographics.com/photoshop/articles/png-gamma.html
		public static void StripGAMA( Stream source, Stream destination )
		{
			// Copy header
			var header = new byte[ 8 ];
			source.Read( header, 0, header.Length );
			destination.Write( header, 0, header.Length );

			var chkLenBytes = new byte[ 4 ];
			var chkTypeBytes = new byte[ 4 ];

			var skipChunks = new[] { "gAMA", "iCCP", "cHRM", "sRGB", "IDAT", "PLTE" };

			while ( source.Read( chkLenBytes, 0, chkLenBytes.Length ) == chkLenBytes.Length )
			{
				var adjustedLenBytes = chkLenBytes.ToArray();

				// Reverse the byte order if we're little-endian
				if ( System.BitConverter.IsLittleEndian ) System.Array.Reverse( adjustedLenBytes );

				var chkLength = System.BitConverter.ToInt32( adjustedLenBytes, 0 );

				// Get the type
				source.Read( chkTypeBytes, 0, chkTypeBytes.Length );
				var chkType = Encoding.ASCII.GetString( chkTypeBytes );

				if ( chkType == "gAMA" )
				{
					// Skip the gamma information
					source.Seek( 4 + chkLength, SeekOrigin.Current );
				}
				else
				{
					// Write chunks we've read to destination
					destination.Write( chkLenBytes, 0, chkLenBytes.Length );
					destination.Write( chkTypeBytes, 0, chkTypeBytes.Length );
				}

				if ( skipChunks.Contains( chkType )/* chkType == "gAMA" || chkType == "IDAT" || chkType == "PLTE"*/ )
				{
					// Nothing else of interest; copy remainder to destination
					var buffer = new byte[ 64 * 1024 ];
					int count;

					while ( ( count = source.Read( buffer, 0, buffer.Length ) ) > 0 )
					{
						destination.Write( buffer, 0, count );
					}

					break;
				}
				else
				{
					// Write this chunk
					var buffer = new byte[ 4 + chkLength ];
					source.Read( buffer, 0, buffer.Length );
					destination.Write( buffer, 0, buffer.Length );
				}
			}
		}

		//PNGRemoveChunks(SourceData, New String() {"gAMA", "iCCP", "cHRM", "sRGB"})
/*
		private byte[] PNGRemoveChunks( ref byte[] SourceData, string[] ChunkIDsToRemove )
		{
			MemoryStream OutputStream = new MemoryStream();
			int CurrentOffset = 0;
			byte[] CurrentChunkLenData = new byte[ 4 ];
			int CurrentChunkLen = 0;
			string CurrentChunkType = null;
			byte[] RetVal = null;

			// Copy the 8 PNG header bytes directly to the output
			OutputStream.Write( SourceData, 0, 8 );
			CurrentOffset = 8;

			// Iterate through the 'Chunks' in the PNG. 
			// 12 = Chunk Length Bytes + Chunk Type Bytes + CRC Bytes
			while ( ( CurrentOffset <= SourceData.Length - 12 ) )
			{
				// Determine the length and type of this chunk (being careful to get the bytes in the right order)
				CurrentChunkLenData[ 0 ] = SourceData[ CurrentOffset + 0 ];
				CurrentChunkLenData[ 1 ] = SourceData[ CurrentOffset + 1 ];
				CurrentChunkLenData[ 2 ] = SourceData[ CurrentOffset + 2 ];
				CurrentChunkLenData[ 3 ] = SourceData[ CurrentOffset + 3 ];

				if ( ( System.BitConverter.IsLittleEndian ) )
				{
					System.Array.Reverse( CurrentChunkLenData );
				}

				CurrentChunkLen = System.BitConverter.ToInt32( CurrentChunkLenData, 0 );
				CurrentChunkType = System.Text.Encoding.ASCII.GetString( SourceData, CurrentOffset + 4, 4 );

				// If this chunk is not in the list of chunk types to remove, add it to the output
				if ( !ChunkIDsToRemove.Contains( CurrentChunkType ) )
				{
					OutputStream.Write( SourceData, CurrentOffset, CurrentChunkLen + 12 );
				}

				// Look at the next chunk in the file
				CurrentOffset += CurrentChunkLen + 12;
			}

			// Tidy up and return the resulting PNG data
			RetVal = OutputStream.ToArray();
			OutputStream.Close();

			return RetVal;
		}*/
	}
}