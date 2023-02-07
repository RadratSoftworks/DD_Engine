using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ResourceInfo
{
    public String filename { get; set; }
    public bool isCompressed { get; set; }
    public int compressedSize { get; set; }
    public int uncompressedSize { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public int offsetInDataChunk { get; set; }

    public byte[] cachedData { get; set; }
}
