using System.IO;
using UnityEditor;

public static class EmptyFolderDeleter
{
    [MenuItem("Tools/Delete Empty Folder")]
    private static void Delete()
    {
        DoDelete("Assets");

        AssetDatabase.Refresh();
    }

    private static void DoDelete(string path)
    {
        foreach (var dir in Directory.GetDirectories(path))
        {
            DoDelete(dir);

            var files = Directory.GetFiles(dir);

            if (files.Length != 0) continue;

            var dirs = Directory.GetDirectories(dir);

            if (dirs.Length != 0) continue;

            AssetDatabase.DeleteAsset(dir);
        }
    }
}