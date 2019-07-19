using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBX2FLVER
{
    class Program
    {
        public static void DebugPrintBufferlayouts(SoulsFormats.FLVER f)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            int i = 0;
            foreach (var layout in f.BufferLayouts)
            {
                var sb = new StringBuilder();
                foreach (var member in layout)
                {
                    //Member(MBT.Float3, MBS.Position);
                    sb.AppendLine($"Member(MBT.{member.Type}, MBS.{member.Semantic}{(member.Index > 0 ? $", {member.Index}" : "")});");
                }
                
                Console.WriteLine($"Buffer Layout {i}: \n{sb.ToString()}\n");
                

                i++;
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        [STAThread]
        public static void Main()
        {
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }

        public static void NotMain(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;

            var importer = new FBX2FLVERImporter();

            importer.ErrorTextOutputted += Importer_ErrorTextOutputted;
            importer.WarningTextOutputted += Importer_WarningTextOutputted;

            //var flverTestLoad = SoulsFormats.FLVER.Read(@"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls II Scholar of the First Sin\Game\model\parts\body\bd_6650_m-bnd\BD_6650_M.flv");

            //flverTestLoad.Header.Version = 0x2000C;

            //flverTestLoad.Write(@"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls II Scholar of the First Sin\Game\model\parts\body\bd_6650_m-bnd\BD_6650_M_ds1.flver");

            //return;


            //DebugPrintBufferlayouts(SoulsFormats.FLVER.Read(@"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA\chr\c4100-chrbnd\chr\c4100\c4100.flver"));
            //DebugPrintBufferlayouts(SoulsFormats.FLVER.Read(@"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA\chr\c4100_bak-chrbnd\chr\c4100\c4100.flver"));
            //DebugPrintBufferlayouts(SoulsFormats.FLVER.Read(@"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls II Scholar of the First Sin\Game\model\parts\body\bd_6650_m-bnd\BD_6650_M.flv"));
            //Console.WriteLine("\nPress any key to exit.");
            //Console.ReadKey(true);
            //return;

            //importer.JOBCONFIG.ChooseGamePreset(FBX2FLVERImportJobConfig.FlverGamePreset.DS2Static);
            //importer.JOBCONFIG.LoadMTDBND(@"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls II Scholar of the First Sin\Game\material\allmaterialbnd.bnd", isBND4: true);
            //importer.JOBCONFIG.LoadFBX(@"D:\FRPG_MOD\FBX Import Test\changeling_ugs.FBX");
            //importer.JOBCONFIG.OutputFlverPath = @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls II Scholar of the First Sin\Game\model\parts\weapon\wp_1800_m-bnd\WP_1800_M.flv";
            //importer.JOBCONFIG.OutputTpfPath = @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls II Scholar of the First Sin\Game\model\parts\weapon\wp_1800_m-bnd\WP_1800_M.tpf";
            //importer.Import();

            importer.JOBCONFIG.Preset = FBX2FLVERImportJobConfig.FlverGamePreset.DS2Skinned;
            importer.JOBCONFIG.MTDBNDPath = @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls II Scholar of the First Sin\Game\material\allmaterialbnd.bnd";
            importer.JOBCONFIG.FBXPath = @"D:\FRPG_MOD\FBX Import Test\CJDS2TEST.FBX";
            importer.JOBCONFIG.ScalePercent = 645;
            //importer.JOBCONFIG.SkeletonRotation = new Microsoft.Xna.Framework.Vector3(0,gi 0, -Microsoft.Xna.Framework.MathHelper.PiOver2);
            //importer.JOBCONFIG.ImportSkeletonFromFLVER = @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls II Scholar of the First Sin\Game\model\parts\body\bd_6650_m-bnd\BD_6650_M.flv";
            importer.JOBCONFIG.OutputFlverPath = @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls II Scholar of the First Sin\Game\model\parts\body\bd_6750_m-bnd\BD_6750_M.flv";
            importer.JOBCONFIG.OutputTpfPath = @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls II Scholar of the First Sin\Game\model\parts\body\bd_6750_m-bnd\BD_6750_M.tpf";

            importer.JOBCONFIG.BeforeSaveAction = (f, t) =>
            {
                foreach (var mat in f.Materials)
                {
                    //0b00000000_00000000_00000000_10000000 no apparent effect
                    //0b00000000_00000000_00000001_00000000 crashes
                    //0b00000000_00000000_00000010_00000000 crashes
                    mat.Flags = -1;
                }
            };

            importer.Import();



            //importer.JOBCONFIG.ChooseGamePreset(FBX2FLVERImportJobConfig.FlverGamePreset.DS1Skinned);
            //importer.JOBCONFIG.LoadMTDBND(@"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA\mtd\Mtd.mtdbnd", isBND4: false);
            //importer.JOBCONFIG.LoadFBX(@"D:\FRPG_MOD\FBX Import Test\ArtoriasSkeletonTest.fbx");
            ////importer.JOBCONFIG.UseAbsoluteVertPositions = false;
            //importer.JOBCONFIG.ScalePercent = 39.37f;
            //importer.JOBCONFIG.OutputFlverPath = @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA\chr\c4100-chrbnd\chr\c4100\c4100.flver";
            //importer.JOBCONFIG.OutputTpfPath = @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA\chr\c4100-chrbnd\chr\c4100\c4100.tpf";
            //importer.JOBCONFIG.GenerateLodAndMotionBlur = true;
            //importer.Import();

            //importer.JOBCONFIG.ChooseGamePreset(FBX2FLVERImportJobConfig.FlverGamePreset.DS3Static);
            //importer.JOBCONFIG.LoadMTDBND(@"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game\mtd\allmaterialbnd.mtdbnd.dcx", isBND4: true);
            //importer.JOBCONFIG.LoadFBX(@"D:\FRPG_MOD\FBX Import Test\darklordsword3.fbx");
            ////importer.JOBCONFIG.UseAbsoluteVertPositions = false;
            //importer.JOBCONFIG.ScalePercent = 200;
            //importer.JOBCONFIG.SceneRotation = new Microsoft.Xna.Framework.Vector3(0, 0, -Microsoft.Xna.Framework.MathHelper.PiOver2);
            //importer.JOBCONFIG.OutputFlverPath = @"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game\parts\wp_a_0201-partsbnd-dcx\parts\Weapon\WP_A_0201\WP_A_0201.flver";
            //importer.JOBCONFIG.OutputTpfPath = @"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game\parts\wp_a_0201-partsbnd-dcx\parts\Weapon\WP_A_0201\WP_A_0201_bad.tpf";
            //importer.JOBCONFIG.GenerateLodAndMotionBlur = true;



            //importer.JOBCONFIG.ChooseGamePreset(FBX2FLVERImportJobConfig.FlverGamePreset.DS3Static);
            //importer.JOBCONFIG.LoadMTDBND(@"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game\mtd\allmaterialbnd.mtdbnd.dcx", isBND4: true);
            //importer.JOBCONFIG.LoadFBX(@"D:\FRPG_MOD\FBX Import Test\darklordsword3.fbx");
            ////importer.JOBCONFIG.UseAbsoluteVertPositions = false;
            //importer.JOBCONFIG.ScalePercent = 10;
            //importer.JOBCONFIG.SceneRotation = new Microsoft.Xna.Framework.Vector3(0, 0, -Microsoft.Xna.Framework.MathHelper.PiOver2);
            //importer.JOBCONFIG.OutputFlverPath = @"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game\parts\wp_a_0827-partsbnd-dcx\parts\Weapon\WP_A_0827\WP_A_0827.flver";
            //importer.JOBCONFIG.OutputTpfPath = @"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game\parts\wp_a_0827-partsbnd-dcx\parts\Weapon\WP_A_0827\WP_A_0827_bad.tpf";
            //importer.JOBCONFIG.GenerateLodAndMotionBlur = true;
            //importer.JOBCONFIG.BeforeSaveAction = (flver, tpf) =>
            //{
            //    var dmy120 = flver.Dummies.First(d => d.ReferenceID == 120);
            //    var dmy100 = flver.Dummies.First(d => d.ReferenceID == 100);

            //    dmy120.ReferenceID = 300;
            //    dmy100.ReferenceID = 301;

            //    //flver.Dummies.Clear();

            //    //flver.Dummies.Add(dmy120);
            //    //flver.Dummies.Add(dmy100);
            //};
            //importer.Import();



            //importer.JOBCONFIG.ChooseGamePreset(FBX2FLVERImportJobConfig.FlverGamePreset.DS3Skinned);
            //importer.JOBCONFIG.LoadMTDBND(@"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game\mtd\allmaterialbnd.mtdbnd.dcx", isBND4: true);
            //importer.JOBCONFIG.LoadFBX(@"D:\FRPG_MOD\FBX Import Test\CJ_DS3.fbx");
            ////importer.JOBCONFIG.ScalePercent = 39.37f;
            //importer.JOBCONFIG.SceneRotation = new Microsoft.Xna.Framework.Vector3(0, 0, -Microsoft.Xna.Framework.MathHelper.PiOver2);
            //importer.JOBCONFIG.OutputFlverPath = @"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game\parts\bd_m_1950-partsbnd-dcx\parts\FullBody\BD_M_1950\BD_M_1950.flver";
            //importer.JOBCONFIG.OutputTpfPath = @"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game\parts\bd_m_1950-partsbnd-dcx\parts\FullBody\BD_M_1950\BD_M_1950.tpf";
            //importer.JOBCONFIG.GenerateLodAndMotionBlur = true;
            //importer.Import();



            Console.WriteLine("\nPress any key to exit.");

            Console.ReadKey(true);
        }

        private static void Importer_WarningTextOutputted(object sender, FBX2FLVERGenericEventArgs<string> e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(e.Parameter);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void Importer_ErrorTextOutputted(object sender, FBX2FLVERGenericEventArgs<string> e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Parameter);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
