using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBX2FLVER
{
    public class FBX2FLVERMaterialLibrary
    {
        public class MTDInstance
        {
            public string Name;
            public List<string> RequiredTextureChannels;
        }

        private Dictionary<string, List<string>> MTDsAndRequiredChannels 
            = new Dictionary<string, List<string>>();

        private static readonly char[] _dirSep = new char[] { '\\', '/' };
        private static string ShortName(string fileName)
        {
            if (fileName.EndsWith("\\") || fileName.EndsWith("/"))
                fileName = fileName.TrimEnd(_dirSep);

            if (fileName.Contains("\\") || fileName.Contains("/"))
                fileName = fileName.Substring(fileName.LastIndexOfAny(_dirSep) + 1);

            if (fileName.Contains("."))
                fileName = fileName.Substring(0, fileName.LastIndexOf('.'));

            return fileName;
        }

        public bool DoesMTDExist(string mtdName)
        {
            return MTDsAndRequiredChannels.ContainsKey(ShortName(mtdName));
        }

        public List<string> GetRequiredTextures(string mtdName)
        {
            if (MTDsAndRequiredChannels.ContainsKey(mtdName))
                return MTDsAndRequiredChannels[mtdName];
            else
                return null;
        }

        private void RegisterMTDInfo(string mtdName, byte[] mtdBytes)
        {
            var shortName = ShortName(mtdName);
            var reqTex = SoulsFormats.MTD.Read(mtdBytes).Textures
                .Where(t => !string.IsNullOrEmpty(t.Path))
                .Select(t => t.Type)
                .ToList();

            if (!MTDsAndRequiredChannels.ContainsKey(shortName))
            {
                MTDsAndRequiredChannels.Add(shortName, reqTex);
            }
            else
            {
                MTDsAndRequiredChannels[shortName] = reqTex;
            }
        }

        public void LoadMTDBND(string path, bool isBND4)
        {
            MTDsAndRequiredChannels.Clear();

            if (!isBND4)
            {
                var bnd = SoulsFormats.BND3.Read(path);
                foreach (var entry in bnd.Files)
                {
                    RegisterMTDInfo(entry.Name, entry.Bytes);
                }
            }
            else
            {
                var bnd = SoulsFormats.BND4.Read(path);
                foreach (var entry in bnd.Files)
                {
                    RegisterMTDInfo(entry.Name, entry.Bytes);
                }
            }
        }
    }
}
