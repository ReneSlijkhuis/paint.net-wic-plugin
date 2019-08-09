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

using System.Runtime.InteropServices;
using System.Text;

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace WicDecoder
{
    class IniFile
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
                                                          string key,
                                                          string def,
                                                          StringBuilder retVal,
                                                          int size,
                                                          string filePath);

        private string _path;

        public IniFile(string path)
        {
            _path = path;
        }

        public string ReadValue(string section, string key)
        {
            const int bufferSize = 4096;
            StringBuilder value = new StringBuilder(bufferSize);
            GetPrivateProfileString(section, key, "", value, bufferSize, _path);
            return value.ToString();
        }
    }
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////