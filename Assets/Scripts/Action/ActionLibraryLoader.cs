using System.IO;

public static class ActionLibraryLoader
{
    public static readonly string FileExtension = ".action";

    public static ActionLibrary Load(string filename)
    {
        ResourceFile generalResources = ResourceManager.Instance.GeneralResources;
        if (!generalResources.Exists(filename))
        {
            throw new FileNotFoundException("Can't find action script file " + filename);
        }
        byte[] data = generalResources.ReadResourceData(generalResources.Resources[filename]);

        using (MemoryStream stream = new MemoryStream(data))
        {
            return ActionParser.Parse(stream);
        }
    }
}
