using System.IO;

public static class ActionLibraryLoader
{
    public static readonly string FileExtension = ".action";

    public static ActionLibrary Load(string filename)
    {
        ResourceFile resourcePack = ResourceManager.Instance.PickBestResourcePackForFile(filename);
        if (!resourcePack.Exists(filename))
        {
            resourcePack = ResourceManager.Instance.ProtectedGeneralResources;
            if (!resourcePack.Exists(filename))
            {
                throw new FileNotFoundException("Can't find action script file " + filename);
            }
        }
        byte[] data = resourcePack.ReadResourceData(resourcePack.Resources[filename]);

        using (MemoryStream stream = new MemoryStream(data))
        {
            return ActionParser.Parse(stream);
        }
    }
}
