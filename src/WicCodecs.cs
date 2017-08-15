///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//    MIT License
//
//    Copyright(c) 2017 René Slijkhuis
//
//    Permission is hereby granted, free of charge, to any person obtaining a copy
//    of this software and associated documentation files (the "Software"), to deal
//    in the Software without restriction, including without limitation the rights
//    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//    copies of the Software, and to permit persons to whom the Software is
//    furnished to do so, subject to the following conditions:
//
//    The above copyright notice and this permission notice shall be included in all
//    copies or substantial portions of the Software.
//
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//    SOFTWARE.
//
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace PaintDotNet.WicDecoder
{
    public static class WicCodecs
    {
        public static string[] GetSupportedExtensions()
        {
            Dictionary<string, string> extensionDictionary = new Dictionary<string, string>();

            using (var regKeyClassesRoot = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64))
            using (var regKeyCodecs = regKeyClassesRoot.OpenSubKey(@"CLSID\{7ED96837-96F0-4812-B211-F13C24117ED3}\Instance", false))
            {
                if (regKeyCodecs != null)
                {
                    var subkeys = regKeyCodecs.GetSubKeyNames();

                    foreach (string subkey in subkeys)
                    {
                        using (var regKeyCodec = regKeyClassesRoot.OpenSubKey(@"CLSID\{7ED96837-96F0-4812-B211-F13C24117ED3}\Instance\" + subkey, false))
                        {
                            if (regKeyCodec != null)
                            {
                                var clsid = (string)regKeyCodec.GetValue("CLSID", "");
                                var extensionList = GetSupportedExtensionsFromCodec(clsid);

                                if (extensionList != null)
                                {
                                    foreach (string extension in extensionList)
                                    {
                                        if (!extensionDictionary.ContainsKey(extension))
                                        {
                                            extensionDictionary.Add(extension, extension);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (!extensionDictionary.ContainsKey(".bmp")) extensionDictionary.Add(".bmp", ".bmp");
            if (!extensionDictionary.ContainsKey(".dib")) extensionDictionary.Add(".dib", ".dib");
            if (!extensionDictionary.ContainsKey(".jpg")) extensionDictionary.Add(".jpg", ".jpg");
            if (!extensionDictionary.ContainsKey(".jpeg")) extensionDictionary.Add(".jpeg", ".jpeg");
            if (!extensionDictionary.ContainsKey(".gif")) extensionDictionary.Add(".gif", ".gif");
            if (!extensionDictionary.ContainsKey(".tif")) extensionDictionary.Add(".tif", ".tif");
            if (!extensionDictionary.ContainsKey(".tiff")) extensionDictionary.Add(".tiff", ".tiff");
            if (!extensionDictionary.ContainsKey(".png")) extensionDictionary.Add(".png", ".png");

            var finalList = new List<string>(extensionDictionary.Values);
            finalList.Sort();
            return finalList.ToArray();
        }

        private static List<string> GetSupportedExtensionsFromCodec(string clsid)
        {
            using (var regKeyClassesRoot = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64))
            using (var regKeyCodec = regKeyClassesRoot.OpenSubKey(@"CLSID\" + clsid, false))
            {
                if (regKeyCodec == null) return null;
                var extensions = (string)regKeyCodec.GetValue("FileExtensions", "");
                return extensions.ToLower().Split(',').ToList();
            }
        }
    }
}
