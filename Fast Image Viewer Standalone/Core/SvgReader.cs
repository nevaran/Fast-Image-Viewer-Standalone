
//TODO: INCOMPLETE - NEEDS MORE THAN LOADING GEOMETRY (color mainly)

using System.IO;
using System.Windows;

namespace FIVStandard.Core
{
    static class SvgReader
    {
        static public string LoadData(string path)
        {
            string dataStart = "d=\"";
            string contents = File.ReadAllText(path);

            int dataStartIndex = contents.IndexOf(dataStart) + dataStart.Length;
            int dataEndIndex = contents.IndexOf('"', dataStartIndex);

            string svgData = contents.Substring(dataStartIndex, dataEndIndex - dataStartIndex);
            MessageBox.Show(svgData);
            return svgData;
        }
    }
}
