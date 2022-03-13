using IONET.Core;
using JJsUSF4Library;
using JJsUSF4Library.FileClasses;
using JJsUSF4Library.FileClasses.ScriptClasses;
using JJsUSF4Library.FileClasses.SubfileClasses;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace JJsUSF4LibraryTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            #region ionet test code

            string tbInputDirectory = $"D:\\Program Files (x86)\\Steam\\steamapps\\common\\Super Street Fighter IV - Arcade Edition\\resource\\battle\\chara\\SKR\\";
            string tbOutputDirectory = $"D:\\Program Files (x86)\\Steam\\steamapps\\common\\Super Street Fighter IV - Arcade Edition\\patch_ae2_tu3\\battle\\chara\\SKR\\";
            string tbColladaDirectory = $"C:\\Users\\Durandal\\Desktop\\SF4\\Import Export Test Directory\\";

            EMO emo = (EMO)USF4Utils.OpenFileStreamCheckCompression(tbInputDirectory + "SKR_03.obj.emo");

            IOScene ioS = IONET.IOManager.LoadScene(tbColladaDirectory + "sak_03_bigjersey.dae", new IONET.ImportSettings());

            emo.EMGs.Add(Collada_import_test.GenerateEMGfromIOMesh(ioS.Models[0].Meshes[0], emo));

            using (MemoryStream ms = new MemoryStream(emo.GenerateBytes()))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    emo.ReadFromStream(br);
                }
            }

            emo.SaveFile(tbOutputDirectory + "SKR_03.obj.emo");

            //TODO verify this works...
            emo.SaveAsSFxTEMO(tbOutputDirectory + "SKR_03_SFxT.obj.emo");

            #endregion




            string emzString = $"D:\\Program Files (x86)\\Steam\\steamapps\\common\\Super Street Fighter IV - Arcade Edition\\resource\\battle\\stage\\RVR.vfx.emz";

            string characterCode = "SKR";
            string fileNameTemplate = ".bcm";

            string directory = $"D:\\Program Files (x86)\\Steam\\steamapps\\common\\Street Fighter X Tekken\\resource\\CMN\\battle\\chara\\";
            string patch_directory = $"D:\\Program Files (x86)\\Steam\\steamapps\\common\\Street Fighter X Tekken\\patch\\CMN\\battle\\chara\\";

            string USF4characterCode = "GEN";
            string USF4fileNameTemplate = ".bcm";
            string USF4Directory = $"D:\\Program Files (x86)\\Steam\\steamapps\\common\\Super Street Fighter IV - Arcade Edition\\resource\\battle\\chara\\";

            string USF4fullPath = $"{USF4Directory}{USF4characterCode}\\{USF4characterCode}{USF4fileNameTemplate}";

            USF4BCM uSF4BCM = (USF4BCM)USF4Utils.OpenFileStreamCheckCompression(USF4fullPath);

            //EMO emo = (EMO)USF4Utils.OpenFileStreamCheckCompression(USF4fullPath);

            //emo.SaveFile("test.obj.emo");


            string patch_fullPath = $"{patch_directory}{characterCode}\\{characterCode}{fileNameTemplate}";
        }
    }
}
