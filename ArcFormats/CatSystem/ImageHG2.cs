//! \file       ImageHG2.cs
//! \date       Sun Nov 29 06:33:49 2015
//! \brief      CatSystem HG2 image format implementation.
//
// Copyright (C) 2015 by morkt
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//

using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Media;

namespace GameRes.Formats.CatSystem
{
    internal class Hg2MetaData : HgMetaData
    {
        public int  DataPacked;
        public int  DataUnpacked;
        public int  CtlPacked;
        public int  CtlUnpacked;
    }

    [Export(typeof(ImageFormat))]
    public class Hg2Format : ImageFormat
    {
        public override string         Tag { get { return "HG2"; } }
        public override string Description { get { return "CatSystem engine image format"; } }
        public override uint     Signature { get { return 0x322D4748; } } // 'HG-2'

        public override ImageMetaData ReadMetaData (Stream stream)
        {
            stream.Position = 8;
            using (var header = new ArcView.Reader (stream))
            {
                var info = new Hg2MetaData();
                int type = header.ReadInt32();
                if (0x25 == type)
                    info.HeaderSize = 0x58;
                else if (0x20 == type)
                    info.HeaderSize = 0x50;
                else
                    return null;
                info.Width  = header.ReadUInt32();
                info.Height = header.ReadUInt32();
                info.BPP    = header.ReadInt32();
                header.BaseStream.Seek (8, SeekOrigin.Current);
                info.DataPacked     = header.ReadInt32();
                info.DataUnpacked   = header.ReadInt32();
                info.CtlPacked      = header.ReadInt32();
                info.CtlUnpacked    = header.ReadInt32();
                header.BaseStream.Seek (8, SeekOrigin.Current);
                info.CanvasWidth    = header.ReadUInt32();
                info.CanvasHeight   = header.ReadUInt32();
                info.OffsetX        = header.ReadInt32();
                info.OffsetY        = header.ReadInt32();
                return info;
            }
        }

        public override ImageData Read (Stream stream, ImageMetaData info)
        {
            using (var reader = new Hg2Reader (stream, (Hg2MetaData)info))
            {
                var pixels = reader.Unpack();
                var format = 24 == info.BPP ? PixelFormats.Bgr24 : PixelFormats.Bgra32;
                return ImageData.CreateFlipped (info, format, null, pixels, reader.Stride);
            }
        }

        public override void Write (Stream file, ImageData image)
        {
            throw new System.NotImplementedException ("Hg2Format.Write not implemented");
        }
    }

    internal sealed class Hg2Reader : HgReader
    {
        Hg2MetaData     m_hg2;

        public Hg2Reader (Stream input, Hg2MetaData info) : base (input, info)
        {
            m_hg2 = info;
        }

        public byte[] Unpack ()
        {
            return UnpackStream (m_hg2.HeaderSize, m_hg2.DataPacked, m_hg2.DataUnpacked,
                                 m_hg2.CtlPacked, m_hg2.CtlUnpacked);
        }
    }
}
