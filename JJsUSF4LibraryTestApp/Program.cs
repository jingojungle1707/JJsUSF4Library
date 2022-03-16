using JJsUSF4Library;
using JJsUSF4Library.FileClasses;
using JJsUSF4Library.FileClasses.ScriptClasses;
using JJsUSF4Library.FileClasses.SubfileClasses;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JJsUSF4LibraryTestApp
{
    class Program
    {
        static void Main(string[] args)
        {

            string emzString = $"D:\\Program Files (x86)\\Steam\\steamapps\\common\\Super Street Fighter IV - Arcade Edition\\resource\\battle\\stage\\RVR.vfx.emz";

            string characterCode = "SKR";
            string fileNameTemplate = ".bcm";

            string directory = $"D:\\Program Files (x86)\\Steam\\steamapps\\common\\Street Fighter X Tekken\\resource\\CMN\\battle\\chara\\";
            string patch_directory = $"D:\\Program Files (x86)\\Steam\\steamapps\\common\\Street Fighter X Tekken\\patch\\CMN\\battle\\chara\\";

            string USF4characterCode = "SKR";
            string USF4fileNameTemplate = ".bcm";
            string USF4Directory = $"D:\\Program Files (x86)\\Steam\\steamapps\\common\\Super Street Fighter IV - Arcade Edition\\resource\\battle\\chara\\";

            string USF4fullPath = $"{USF4Directory}{USF4characterCode}\\{USF4characterCode}{USF4fileNameTemplate}";

            USF4BCM uSF4BCM = (USF4BCM)USF4Utils.OpenFileStreamCheckCompression(USF4fullPath);

            uSF4BCM.SaveFile("usf4BCM_test.bcm");

            SFxTBCM sFxTBCM = (SFxTBCM)USF4Utils.OpenFileStreamCheckCompression($"{directory}{characterCode}\\{characterCode}{fileNameTemplate}");

            XmlSerializer SFxTxmlSerializer = new XmlSerializer(typeof(SFxTBCM));

            using (FileStream fsSource = new FileStream("xmloutputtest.xml", FileMode.Create, FileAccess.Write))
            {
                SFxTxmlSerializer.Serialize(fsSource, sFxTBCM);
            }

            //EMO emo = (EMO)USF4Utils.OpenFileStreamCheckCompression(USF4fullPath);

            //emo.SaveFile("test.obj.emo");


            string patch_fullPath = $"{patch_directory}{characterCode}\\{characterCode}{fileNameTemplate}";
        }
    }
}
