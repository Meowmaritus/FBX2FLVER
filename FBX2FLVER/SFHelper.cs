using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FBX2FLVER
{
    public static class SFHelper
    {
        private static readonly char[] _dirSep = new char[] { '\\', '/' };

        private static string GetFileNameWithoutDirectoryOrExtension(string fileName)
        {
            if (fileName.EndsWith("\\") || fileName.EndsWith("/"))
                fileName = fileName.TrimEnd(_dirSep);

            if (fileName.Contains("\\") || fileName.Contains("/"))
                fileName = fileName.Substring(fileName.LastIndexOfAny(_dirSep) + 1);

            //if (fileName.Contains("."))
            //    fileName = fileName.Substring(0, fileName.LastIndexOf('.'));

            return fileName;
        }

        public static void WriteFile<TFormat>(TFormat file, string path)
            where TFormat : SoulsFile<TFormat>, new()
        {
            if (path.Contains("|"))
            {
                var splitPath = path.Split('|');
                if (splitPath.Length != 2)
                    throw new Exception("Invalid internal BND path format. Expected 'C:\\Path\\To\\BND.bnd|Internal_File_Name.Extension'.");
                var bndPath = splitPath[0];
                var internalPath = splitPath[1];
                if (BND3.Is(bndPath))
                {
                    var bnd3 = BND3.Read(bndPath);
                    var internalFile = bnd3.Files.LastOrDefault(f => GetFileNameWithoutDirectoryOrExtension(f.Name) == internalPath);
                    if (internalFile == null)
                        throw new Exception($"Internal BND file path '{internalPath}' not found in BND '{bndPath}'.");
                    internalFile.Bytes = file.Write();
                    bnd3.Write(bndPath);
                }
                else if (BND4.Is(bndPath))
                {
                    var bnd4 = BND4.Read(bndPath);
                    var internalFile = bnd4.Files.LastOrDefault(f => GetFileNameWithoutDirectoryOrExtension(f.Name) == internalPath);
                    if (internalFile == null)
                        throw new Exception($"Internal BND file path '{internalPath}' not found in BND '{bndPath}'.");
                    internalFile.Bytes = file.Write();
                    bnd4.Write(bndPath);
                }
                else
                {
                    throw new Exception("Internal BND path specified but file is not a BND file!");
                }
            }
            else
            {
                file.Write(path);
            }
        }

        public static (string Uri, SoulsFormats.SoulsFile<TFormat> File) ReadFile<TFormat>(Window parentWindow, string path)
            where TFormat : SoulsFile<TFormat>, new()
        {
            if (path.Contains("|"))
            {
                var splitPath = path.Split('|');
                if (splitPath.Length != 2)
                    throw new Exception("Invalid internal BND path format. Expected 'C:\\Path\\To\\BND.bnd|Internal_File_Name.Extension'.");
                var bndPath = splitPath[0];
                var internalPath = splitPath[1];
                if (BND3.Is(bndPath))
                {
                    var bnd3 = BND3.Read(bndPath);
                    var internalFile = bnd3.Files.LastOrDefault(f => GetFileNameWithoutDirectoryOrExtension(f.Name) == internalPath);
                    if (internalFile == null)
                        throw new Exception($"Internal BND file path '{internalPath}' not found in BND '{bndPath}'.");
                    return (GetFileNameWithoutDirectoryOrExtension(internalFile.Name), SoulsFile<TFormat>.Read(internalFile.Bytes));
                }
                else if (BND4.Is(bndPath))
                {
                    var bnd4 = BND4.Read(bndPath);
                    var internalFile = bnd4.Files.LastOrDefault(f => GetFileNameWithoutDirectoryOrExtension(f.Name) == internalPath);
                    if (internalFile == null)
                        throw new Exception($"Internal BND file path '{internalPath}' not found in BND '{bndPath}'.");
                    return (GetFileNameWithoutDirectoryOrExtension(internalFile.Name), SoulsFile<TFormat>.Read(internalFile.Bytes));
                }
                else
                {
                    throw new Exception("Internal BND path specified but file is not a BND file!");
                }
            }
            else
            {
                if (BND3.Is(path))
                {
                    var bnd3 = BND3.Read(path);
                    var helperWindow = new SFHelperWindow();
                    if (parentWindow != null)
                        helperWindow.Owner = parentWindow;
                    helperWindow.SetBNDList(bnd3.Files.Select(x => GetFileNameWithoutDirectoryOrExtension(x.Name)));
                    helperWindow.ShowDialog();
                    var selected = bnd3.Files.Last(f => GetFileNameWithoutDirectoryOrExtension(f.Name) == helperWindow.Result);
                    return ($"{path}|{helperWindow.Result}", SoulsFile<TFormat>.Read(selected.Bytes));
                }
                else if (BND4.Is(path))
                {
                    var bnd4 = BND4.Read(path);
                    var helperWindow = new SFHelperWindow();
                    if (parentWindow != null)
                        helperWindow.Owner = parentWindow;
                    helperWindow.SetBNDList(bnd4.Files.Select(x => GetFileNameWithoutDirectoryOrExtension(x.Name)));
                    helperWindow.ShowDialog();
                    var selected = bnd4.Files.Last(f => GetFileNameWithoutDirectoryOrExtension(f.Name) == helperWindow.Result);
                    return ($"{path}|{helperWindow.Result}", SoulsFile<TFormat>.Read(selected.Bytes));
                }
                else
                {
                    return (path, SoulsFile<TFormat>.Read(path));
                }
            }
            
            
        }
    }
}
