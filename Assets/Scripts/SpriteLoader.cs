using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SpriteLoader
{
    private Dictionary<string, Sprite> spriteCache;

    public SpriteLoader()
    {
        spriteCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
    }

    public void ReadRGBA32TextureDataSeparate(ResourceFile resources, ResourceInfo resourceInfo, ref byte[] buffer, bool isMask)
    {
        byte[] data = resources.ReadResourceData(resourceInfo);
        using (BinaryReader2 reader = new BinaryReader2(new MemoryStream(data)))
        {
            bool isPalette = (reader.ReadByte() == 1);
            int stride = resourceInfo.width * 4;

            if (!isPalette)
            {
                for (int y = 0; y < resourceInfo.height; y++)
                {
                    for (int x = 0; x < resourceInfo.width; x++)
                    {
                        ushort rgb565 = reader.ReadUInt16();

                        if (isMask)
                        {
                            int red = ((rgb565 & 0xF800) >> 8);
                            red += red >> 5;

                            buffer[y * stride + x * 4 + 3] = (byte)red;
                        } else
                        {
                            int red = ((rgb565 & 0xF800) >> 8);
                            red += red >> 5;

                            int green = ((rgb565 & 0x07E0) >> 3);
                            green += green >> 6;

                            int blue = ((rgb565 & 0x001F) << 3);
                            blue += blue >> 5;

                            buffer[y * stride + x * 4] = (byte)red;
                            buffer[y * stride + x * 4 + 1] = (byte)green;
                            buffer[y * stride + x * 4 + 2] = (byte)blue;
                            buffer[y * stride + x * 4 + 3] = 255;
                        }
                    }
                }
            } else
            {
                ushort paletteCount = reader.ReadUInt16BE();

                ushort[] palettes = new ushort[paletteCount];
                for (ushort i = 0; i < paletteCount; i++)
                {
                    palettes[i] = reader.ReadUInt16();
                }

                for (int y = 0; y < resourceInfo.height; y++)
                {
                    for (int x = 0; x < resourceInfo.width; x++)
                    {
                        byte paletteIndex = reader.ReadByte();
                        ushort rgb565 = palettes[paletteIndex];

                        if (isMask)
                        {
                            int red = ((rgb565 & 0xF800) >> 8);
                            red += red >> 5;

                            buffer[y * stride + x * 4 + 3] = (byte)red;
                        }
                        else
                        {
                            int red = ((rgb565 & 0xF800) >> 8);
                            red += red >> 5;

                            int green = ((rgb565 & 0x07E0) >> 3);
                            green += green >> 6;

                            int blue = ((rgb565 & 0x001F) << 3);
                            blue += blue >> 5;

                            buffer[y * stride + x * 4] = (byte)red;
                            buffer[y * stride + x * 4 + 1] = (byte)green;
                            buffer[y * stride + x * 4 + 2] = (byte)blue;
                            buffer[y * stride + x * 4 + 3] = 255;
                        }
                    }
                }
            }
        }
    }

    public byte[] GetRGBA32TextureData(ResourceFile resources, string pathDsi, string pathAlphaDsi, out Vector2 textureSize)
    {
        bool dsiExists = resources.Exists(pathDsi);
        bool alphaDsiExists = resources.Exists(pathAlphaDsi);

        textureSize = new Vector2(0, 0);

        if (!dsiExists)
        {
            return null;
        }

        byte[] dataBuffer = null;

        if (dsiExists)
        {
            ResourceInfo dsiFileInfo = resources.Resources[pathDsi];
            textureSize.Set(dsiFileInfo.width, dsiFileInfo.height);

            dataBuffer = new byte[dsiFileInfo.width * dsiFileInfo.height * 4];
            ReadRGBA32TextureDataSeparate(resources, dsiFileInfo, ref dataBuffer, false);
        }

        if (alphaDsiExists)
        {
            ResourceInfo dsiAlphaFileInfo = resources.Resources[pathDsi];
            if ((dsiAlphaFileInfo.width != textureSize.x) || (dsiAlphaFileInfo.height != textureSize.y))
            {
                Debug.LogError("Width or height of mask vs normal image does not match!");
            } else
            {
                ReadRGBA32TextureDataSeparate(resources, dsiAlphaFileInfo, ref dataBuffer, true);
            }
        }

        return dataBuffer;
    }

    public Sprite Load(ResourceFile resources, string path)
    {
        string pathRaw = Path.ChangeExtension(path, "");

        if (spriteCache.ContainsKey(pathRaw))
        {
            return spriteCache[pathRaw];
        }

        string pathDsi = Path.ChangeExtension(pathRaw, ".dsi");
        string pathAlphaDsi = Path.ChangeExtension(pathRaw + "_a", ".dsi");

        byte[] data = GetRGBA32TextureData(resources, pathDsi, pathAlphaDsi, out Vector2 textureSize);

        if (data == null)
        {
            return null;
        }

        Texture2D tex = new Texture2D((int)textureSize.x, (int)textureSize.y, TextureFormat.RGBA32, false);
        tex.name = pathRaw;

        tex.LoadRawTextureData(data);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        sprite.name = pathRaw;
        spriteCache.Add(pathRaw, sprite);

        return sprite;
    }
}
