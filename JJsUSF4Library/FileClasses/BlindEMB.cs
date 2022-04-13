using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses
{
    /// <summary>
    /// Variant of EMB which reads all contents as OtherFile, apart from enclosed EMB which will also be converted to BlindEMBs.
    /// <para>As a result, the exact byte representation of the original content files will be maintained.</para>
    /// </summary>
    public class BlindEMB : EMB
    {
        protected override USF4File GetFileType(int dword)
        {
            USF4File uf = USF4Methods.FetchClass((USF4Methods.FileType)dword);

            if (uf.GetType() == typeof(EMB))
            {
                return new BlindEMB();
            }
            else return new OtherFile();
        }
    }
}
