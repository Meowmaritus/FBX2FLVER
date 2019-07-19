using Microsoft.Win32;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FBX2FLVER
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        FBX2FLVERImporter Importer = new FBX2FLVERImporter();

        void LoadConfig()
        {
            if (File.Exists("FBX2FLVER_Config.ini"))
            {
                try
                {
                    var iniData = File.ReadAllLines("FBX2FLVER_Config.ini")
                    .ToDictionary(
                        line => line.Substring(0, line.IndexOf("=")).Trim(),
                        line => line.Substring(line.IndexOf("=") + 1).Trim()
                    );

                    TextBoxMTDBNDPath.Text = iniData["MTDBNDPath"];
                    if (!string.IsNullOrEmpty(iniData["GamePreset"]))
                        ComboBoxGameSelect.SelectedItem = Enum.Parse(typeof(FBX2FLVERImportJobConfig.FlverGamePreset), iniData["GamePreset"]);
                    TextBoxFBXPath.Text = iniData["FBXPath"];
                    TextBoxFLVEROutputMain.Text = iniData["FLVEROutputMain"];
                    TextBoxTPFOutputMain.Text = iniData["TPFOutputMain"];
                    if (float.TryParse(iniData["ImportScalePercent"], out float result))
                        TextBoxImportScalePercent.Text = result.ToString();
                    else
                        TextBoxImportScalePercent.Text = "100";
                    TextBoxImportSkeletonFLVER.Text = iniData["ImportSkeletonFLVER"];
                }
                catch
                {
                    SaveConfig();
                }
            }
            else
            {
                SaveConfig();
            }
        }

        void SaveConfig()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"MTDBNDPath={TextBoxMTDBNDPath.Text}");
            sb.AppendLine($"GamePreset={(ComboBoxGameSelect.SelectedItem ?? "").ToString()}");
            sb.AppendLine($"FBXPath={TextBoxFBXPath.Text}");
            sb.AppendLine($"FLVEROutputMain={TextBoxFLVEROutputMain.Text}");
            sb.AppendLine($"TPFOutputMain={TextBoxTPFOutputMain.Text}");
            sb.AppendLine($"ImportScalePercent={TextBoxImportScalePercent.Text}");
            sb.AppendLine($"ImportSkeletonFLVER={TextBoxImportSkeletonFLVER.Text}");
            File.WriteAllText("FBX2FLVER_Config.ini", sb.ToString());
        }

        public MainWindow()
        {
            InitializeComponent();

            ComboBoxGameSelect.ItemsSource = (FBX2FLVERImportJobConfig.FlverGamePreset[])
                Enum.GetValues(typeof(FBX2FLVERImportJobConfig.FlverGamePreset));

            ComboBoxGameSelect.SelectedIndex = 0;

            LoadConfig();
        }

        private void ButtonBrowseMTDBNDPath_Click(object sender, RoutedEventArgs e)
        {
            var browseDialog = new OpenFileDialog()
            {
                Filter = "BNDs (*.*BND*)|*.*BND*",
                Title = "Open *.MTDBND, *.MTDBND.DCX, or *.BND",
            };

            if (browseDialog.ShowDialog() == true)
            {
                TextBoxMTDBNDPath.Text = browseDialog.FileName;
            }
        }

        private void ButtonBrowseFBXPath_Click(object sender, RoutedEventArgs e)
        {
            var browseDialog = new OpenFileDialog()
            {
                Filter = "FBX Scenes (*.FBX)|*.FBX",
                Title = "Open .FBX",
            };

            if (browseDialog.ShowDialog() == true)
            {
                TextBoxFBXPath.Text = browseDialog.FileName;
            }
        }

        private void ButtonBrowseFLVEROutputMain_Click(object sender, RoutedEventArgs e)
        {
            var browseDialog = new OpenFileDialog()
            {
                Filter = "All Files (*.*)|*.*",
                Title = "Choose where to save FLVER Model (supports saving inside BNDs)",
                CheckFileExists = false,
            };

            if (browseDialog.ShowDialog() == true)
            {
                if ((browseDialog.FileName.ToUpper().EndsWith(".FLVER") || browseDialog.FileName.ToUpper().EndsWith(".FLVER.DCX")) && 
                    File.Exists(browseDialog.FileName))
                {
                    TextBoxFLVEROutputMain.Text = browseDialog.FileName;
                }
                else
                {
                    var loadedFile = SFHelper.ReadFile<FLVER>(this, browseDialog.FileName);
                    TextBoxFLVEROutputMain.Text = loadedFile.Uri;
                }
            }
        }

        private void ButtonBrowseTPFOutputMain_Click(object sender, RoutedEventArgs e)
        {
            var browseDialog = new OpenFileDialog()
            {
                Filter = "All Files (*.*)|*.*",
                Title = "Choose where to save TPF Textures (supports saving inside BNDs)",
                CheckFileExists = false,
            };

            if (browseDialog.ShowDialog() == true)
            {
                if ((browseDialog.FileName.ToUpper().EndsWith(".TPF") || browseDialog.FileName.ToUpper().EndsWith(".TPF.DCX")) && 
                    File.Exists(browseDialog.FileName))
                {
                    TextBoxTPFOutputMain.Text = browseDialog.FileName;
                }
                else
                {
                    var loadedFile = SFHelper.ReadFile<TPF>(this, browseDialog.FileName);
                    TextBoxTPFOutputMain.Text = loadedFile.Uri;
                }
            }
        }

        private void ButtonIMPORT_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveConfig();

                Importer = new FBX2FLVERImporter();
                Importer.JOBCONFIG.MTDBNDPath = TextBoxMTDBNDPath.Text;
                Importer.JOBCONFIG.Preset = (FBX2FLVERImportJobConfig.FlverGamePreset)(ComboBoxGameSelect.SelectedItem);
                Importer.JOBCONFIG.FBXPath = TextBoxFBXPath.Text;
                Importer.JOBCONFIG.OutputFlverPath = TextBoxFLVEROutputMain.Text;
                Importer.JOBCONFIG.OutputTpfPath = TextBoxTPFOutputMain.Text;

                if (float.TryParse(TextBoxImportScalePercent.Text, out float scalePercent))
                {
                    Importer.JOBCONFIG.ScalePercent = scalePercent;
                }
                else
                {
                    MessageBox.Show("Value for Import Scale Percent is not a number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Importer.JOBCONFIG.ImportSkeletonFromFLVER = TextBoxImportSkeletonFLVER.Text;

                Importer.InfoTextOutputted += Importer_InfoTextOutputted;
                Importer.WarningTextOutputted += Importer_WarningTextOutputted;
                Importer.ErrorTextOutputted += Importer_ErrorTextOutputted;

                Importer.ImportStarted += Importer_ImportStarted;
                Importer.ImportEnding += Importer_ImportEnding;

                Importer.Import();
            }
            catch (Exception ex)
            {
                AddRunToConsole($"An error occurred while trying to import:\n\n{ex.ToString()}", Colors.Red, Colors.Black, isBold: true);
            }
        }

        private void Importer_ImportEnding(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MainGrid.IsEnabled = true;
                Mouse.OverrideCursor = null;

                Importer.InfoTextOutputted -= Importer_InfoTextOutputted;
                Importer.WarningTextOutputted -= Importer_WarningTextOutputted;
                Importer.ErrorTextOutputted -= Importer_ErrorTextOutputted;

                Importer.ImportStarted -= Importer_ImportStarted;
                Importer.ImportEnding -= Importer_ImportEnding;
            });
        }

        private void Importer_ImportStarted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MainGrid.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;
            });

            Dispatcher.Invoke(() =>
            {
                SaveConfig();
            });
        }

        private void AddRunToConsole(string text, Color? boxColor = null, Color? color = null, bool isBold = false)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var r = new Run()
                {
                    Text = text,
                    FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                };

                if (color.HasValue)
                {
                    r.Foreground = new SolidColorBrush(color.Value);
                }

                var p = new Paragraph(r);

                if (boxColor.HasValue)
                {
                    p.Margin = new Thickness(4);
                    p.Padding = new Thickness(4);
                    p.BorderBrush = SystemColors.ActiveBorderBrush;
                    p.BorderThickness = new Thickness(1);
                    p.Background = new SolidColorBrush(boxColor.Value);
                }
                else
                {
                    p.Margin = new Thickness(0);
                    p.Padding = new Thickness(0);
                }

                ConsoleOutputDocument.Blocks.Add(p);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    TextBoxConsoleOutput.ScrollToEnd();
                }), DispatcherPriority.Background);

            }), DispatcherPriority.Background);
        }

        private void Importer_ErrorTextOutputted(object sender, FBX2FLVERGenericEventArgs<string> e)
        {
            AddRunToConsole(e.Parameter, Colors.Red, Colors.White, true);
        }

        private void Importer_WarningTextOutputted(object sender, FBX2FLVERGenericEventArgs<string> e)
        {
            AddRunToConsole(e.Parameter, Colors.Yellow, Colors.Black);
        }

        private void Importer_InfoTextOutputted(object sender, FBX2FLVERGenericEventArgs<string> e)
        {
            AddRunToConsole(e.Parameter);
        }

        private void ButtonBrowseImportSkeletonFLVER_Click(object sender, RoutedEventArgs e)
        {
            var browseDialog = new OpenFileDialog()
            {
                Filter = "All Files (*.*)|*.*",
                Title = "Choose FLVER Model to import skeleton from (supports loading from inside BNDs)",
                CheckFileExists = false,
            };

            if (browseDialog.ShowDialog() == true)
            {
                var loadedFile = SFHelper.ReadFile<FLVER>(this, browseDialog.FileName);
                TextBoxImportSkeletonFLVER.Text = loadedFile.Uri;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveConfig();
        }

        private void ButtonDONATE_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.me/Meowmaritus");
        }

        private void ButtonViewGamePresetInfo_Click(object sender, RoutedEventArgs e)
        {
            var checkImporter = new FBX2FLVERImporter();
            checkImporter.JOBCONFIG.ChooseGamePreset((FBX2FLVERImportJobConfig.FlverGamePreset)(ComboBoxGameSelect.SelectedItem));
            var sb = new StringBuilder();
            sb.AppendLine($"Vertex Buffer Layout:\n{FBX2FLVERImportJobConfig.GetBufferLayoutStringFromStruct(checkImporter.JOBCONFIG.BufferLayout)}");
            sb.AppendLine();
            sb.AppendLine("FBX Material Channel Mapping:");
            foreach (var kvp in checkImporter.JOBCONFIG.FBXMaterialChannelMap)
            {
                sb.AppendLine($"    ['{kvp.Key}']: '{kvp.Value}'");
            }
            sb.AppendLine();
            sb.AppendLine("Hardcoded Texture Names:");
            foreach (var kvp in checkImporter.JOBCONFIG.MTDHardcodedTextureChannels)
            {
                sb.AppendLine($"    ['{kvp.Key}']: '{kvp.Value}'");
            }
            sb.AppendLine();
            sb.AppendLine("Misc. Settings:");
            sb.AppendLine($"    [FLVER Version]: 0x{checkImporter.JOBCONFIG.FlverVersion:X}");
            sb.AppendLine($"    [TPF Encoding]: {checkImporter.JOBCONFIG.TpfEncoding}");
            sb.AppendLine($"    [TPF Flag 2]: {checkImporter.JOBCONFIG.TpfFlag2}");
            sb.AppendLine($"    [Normal W-Field Value]: {checkImporter.JOBCONFIG.NormalWValue}");
            sb.AppendLine($"    [Placeholder Material Definition (MTD) Name]: '{checkImporter.JOBCONFIG.PlaceholderMaterialShaderName}.mtd'");
            sb.AppendLine($"    [Use Non-Relative Bone Indices]: {checkImporter.JOBCONFIG.UseDirectBoneIndices}");
            sb.AppendLine($"    [Unknown Value at 0x5C Offset in FLVER]: {checkImporter.JOBCONFIG.Unk0x5CValue}");
            sb.AppendLine($"    [Unknown Value at 0x68 Offset in FLVER]: {checkImporter.JOBCONFIG.Unk0x68Value}");

            var infoWindow = new PresetInfoWindow();
            infoWindow.Title = $"Details for {((FBX2FLVERImportJobConfig.FlverGamePreset)(ComboBoxGameSelect.SelectedItem))} Preset";
            infoWindow.TextBlockDetails.Text = sb.ToString();
            infoWindow.Owner = this;
            infoWindow.ShowDialog();
        }
    }
}
