using System.IO;

public static class IconFileNamingUtility
{
    public static string GetSafeFileName(string objectName)
    {
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            objectName = objectName.Replace(invalidChar.ToString(), "");
        }

        return objectName.Trim();
    }

    public static string GetUniqueFilePath(
        string folderPath,
        string baseName,
        string extension)
    {
        string path =
            Path.Combine(folderPath, $"{baseName}.{extension}");

        int counter = 1;

        while (File.Exists(path))
        {
            path = Path.Combine(
                folderPath,
                $"{baseName}_{counter}.{extension}");

            counter++;
        }

        return path;
    }
}