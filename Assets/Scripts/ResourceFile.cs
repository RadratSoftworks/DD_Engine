using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

public class ResourceFile
{
    private static readonly int OffsetStartInfoSection = 12;
    private static readonly int ResourceInfoBinarySize = 112;

    private String filename;
    private ProtectedFilePatcher filePatcher;
    private Dictionary<string, ResourceInfo> resourcesList;

    public Dictionary<string, ResourceInfo> Resources => resourcesList;

    public ResourceFile(ProtectedFilePatcher filePatcher, string filename)
    {
        this.filename = filename;
        this.filePatcher = filePatcher;

        resourcesList = new Dictionary<string, ResourceInfo>(StringComparer.OrdinalIgnoreCase);

        using (var fileStream = filePatcher.OpenFile(filename))
        {
            using (var fileBinaryReader = new BinaryReader2(fileStream))
            {
                byte[] headerCheck = new byte[4];
                if (fileStream.Read(headerCheck) == 4)
                {
                    if ((headerCheck[0] != 'o') || (headerCheck[1] != 'p') || (headerCheck[2] != 'e') || (headerCheck[3] != 's'))
                    {
                        throw new InvalidDataException("Invalid OPES header!");
                    }
                }

                ReadAllResourcesInfo(fileStream, fileBinaryReader);
            }
        }
    }

    private void ReadAllResourcesInfo(Stream fileStream, BinaryReader2 fileBinaryReader)
    {
        fileStream.Seek(4, SeekOrigin.Current);
        int numberOfResources = fileBinaryReader.ReadInt32();

        int offsetDataSection = OffsetStartInfoSection + ResourceInfoBinarySize * numberOfResources;

        for (uint i = 0; i < numberOfResources; i++)
        {
            fileStream.Seek(OffsetStartInfoSection + i * ResourceInfoBinarySize, SeekOrigin.Begin);

            string filename = fileBinaryReader.ReadZeroTerminatedString();

            fileStream.Seek(OffsetStartInfoSection + i * ResourceInfoBinarySize + 91, SeekOrigin.Begin);

            bool isCompressed = (fileBinaryReader.ReadByte() == 1);
            int compressedSize = fileBinaryReader.ReadInt32();
            int uncompressedSize = fileBinaryReader.ReadInt32();
            int width = fileBinaryReader.ReadInt32();
            int height = fileBinaryReader.ReadInt32();

            resourcesList.Add(filename, new ResourceInfo()
            {
                filename = filename,
                isCompressed = isCompressed,
                compressedSize = compressedSize,
                uncompressedSize = uncompressedSize,
                width = width,
                height = height,
                offsetInDataChunk = offsetDataSection
            });

            offsetDataSection += compressedSize;
        }
    }

    public byte[] ReadResourceData(ResourceInfo info)
    {
        using (var fileStream = filePatcher.OpenFile(filename))
        {
            using (var fileBinaryReader = new BinaryReader2(fileStream))
            {
                fileStream.Seek(info.offsetInDataChunk, SeekOrigin.Begin);
                byte[] dataRead = new byte[info.compressedSize];

                if (fileStream.Read(dataRead) != info.compressedSize)
                {
                    return null;
                }

                if (!info.isCompressed)
                {
                    return dataRead;
                }
                else
                {
                    using (InflaterInputStream stream = new InflaterInputStream(new MemoryStream(dataRead)))
                    {
                        byte[] uncomped = new byte[info.uncompressedSize];
                        if (stream.Read(uncomped) != info.uncompressedSize)
                        {
                            throw new InvalidDataException("The compressed resource data is corrupted!");
                        }

                        return uncomped;
                    }
                }
            }
        }
    }

    public bool Exists(string path)
    {
        return resourcesList.ContainsKey(path);
    }
}
