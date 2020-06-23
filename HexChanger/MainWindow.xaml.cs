using Hexes;
using Managers;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace HexChanger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GlobalManager _globalManager;
        private string _selectedInstructionPath;

        public MainWindow()
        {
            _globalManager = new GlobalManager();
            InitializeComponent();
            ValidateRepairAfterSelection();
            PrintHexes();
            ValidateInstructionsCatalog();
            BuildInstructionTree(Properties.Settings.Default.InstructionsDirectory);
        }

        private void ValidateInstructionsCatalog()
        {
            string instructionCatalogPath = Properties.Settings.Default.InstructionsDirectory;
            if (!Directory.Exists(instructionCatalogPath) || !File.GetAttributes(instructionCatalogPath).HasFlag(FileAttributes.Directory))
            {
                Properties.Settings.Default.InstructionsDirectory = DriveInfo.GetDrives()[0].Name;
                Properties.Settings.Default.Save();
            }
        }

        public void SelectInstrucionsCatalog(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Properties.Settings.Default.InstructionsDirectory = fbd.SelectedPath;
                    Properties.Settings.Default.Save();
                }
                _selectedInstructionPath = "";
                InstructionsTree.Items.Clear();
                ValidateInstructionsCatalog();
                BuildInstructionTree(Properties.Settings.Default.InstructionsDirectory);
            }
        }

        private void ValidateRepairAfterSelection()
        {
            RepairAfterSelectionSwitch.IsChecked = Properties.Settings.Default.RepairAfterSelection;
        }

        private void ChangeRepairAfterSelection(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.RepairAfterSelection = (bool)RepairAfterSelectionSwitch.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void BuildInstructionTree(string currentDirectory, TreeViewItem parentNode = null, bool isDirectoryInstruction = false)
        {
            var currentNode = new TreeViewItemInstruction();
            currentNode.FullPath = currentDirectory;

            if (Path.GetFileName(currentDirectory).Trim() == "")
                currentNode.Header = currentDirectory;
            else
                currentNode.Header = Path.GetFileName(currentDirectory);

            if (parentNode != null)
                parentNode.Items.Add(currentNode);
            else
                InstructionsTree.Items.Add(currentNode);

            if (isDirectoryInstruction)
            {
                currentNode.Selected += new RoutedEventHandler(InstructionSelected);
                currentNode.Unselected += new RoutedEventHandler(InstructionUnselected);
            }

            var subDirectories = Directory.GetDirectories(currentDirectory);
            for (int i = 0; i < subDirectories.Length; i++)
            {
                bool isNextDirectoryInstruction = false;
                try
                {
                    Regex rx = new Regex(@".*" + _globalManager.FileManager.IdentifyName + @".*");
                    foreach (var file in Directory.GetFiles(subDirectories[i]))
                    {
                        if (rx.IsMatch(file))
                        {
                            isNextDirectoryInstruction = true;
                            break;
                        }
                    }
                    if (!isDirectoryInstruction)
                        BuildInstructionTree(subDirectories[i], currentNode, isNextDirectoryInstruction);
                }
                catch (Exception e)
                {

                }
            }
        }

        private void InstructionSelected(object sender, RoutedEventArgs e)
        {
            _selectedInstructionPath = ((TreeViewItemInstruction)sender).FullPath;
            try
            {
                _selectedInstructionPath = ((TreeViewItemInstruction)sender).FullPath;
                LoadInstructions(_selectedInstructionPath);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void InstructionUnselected(object sender, RoutedEventArgs e)
        {
            _selectedInstructionPath = "";
        }

        public void LoadCorruptedFile(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    DefaultExt = ".bin",
                    Filter = "Pliki binarne (*.bin)|*.bin"
                };

                Nullable<bool> result = openFileDialog.ShowDialog();
                if (openFileDialog.FileName != "")
                {
                    Hex corruptedHex = _globalManager.FileManager.HexIO.ReadHex(openFileDialog.FileName);
                    _globalManager.FixManager.CorruptedHex = corruptedHex;
                    _globalManager.FixManager.FixedHex.Clear();
                    PrintHexes();
                    if (_globalManager.FixManager.IsSet() && (bool)RepairAfterSelectionSwitch.IsChecked)
                    {
                        PrintAndFix();
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        public void SelectInstructionDirectory(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    DefaultExt = ".bin",
                    Filter = "Plik identyfikacyjny|" + _globalManager.FileManager.IdentifyName + "*.bin"
                };
                //openFileDialog.InitialDirectory = @"C:\";

                Nullable<bool> result = openFileDialog.ShowDialog();
                if (openFileDialog.FileName != "")
                {
                    LoadInstructions(Path.GetDirectoryName(openFileDialog.FileName));
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void LoadInstructions(string directory)
        {
            _globalManager.FileManager.InstructionDir = directory;
            _globalManager.FixManager.InstructionSet = _globalManager.FileManager.ReadInstructions();
            if (_globalManager.FixManager.IsSet() && (bool)RepairAfterSelectionSwitch.IsChecked)
            {
                PrintAndFix();
            }
        }

        public void Fix(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintAndFix();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void PrintAndFix()
        {
            try
            {
                if (!_globalManager.Identify())
                {
                    if (MessageBox.Show("Brak dopasowania pliku identyfikacyjnego z plikiem uszkodzonym. Wykonać instrukcje mimo to?", "Brak dopasowania", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                        throw new Exception("Plik identyfikacyjy oraz plik uszkodzony nie zostały dopasowane.");
                    }
                }

                var positionsFound = _globalManager.Find();
                if (positionsFound == null)
                {
                    throw new Exception("Nie znaleziono uszkodzonych segmentów.");
                }

                if (positionsFound != null)
                {
                    //conflict check
                    bool isConflict = false;
                    foreach (var position in positionsFound)
                    {
                        if (position.Value.Count > 1)
                        {
                            isConflict = true;
                            break;
                        }
                    }
                    if (isConflict)
                    {
                        ConflictDialog dialog = new ConflictDialog(positionsFound);
                        dialog.ShowDialog();
                        if (dialog.SolvedConflicts != null)
                        {
                            positionsFound = dialog.SolvedConflicts;
                        }
                    }
                    Hex fixedHex = _globalManager.Fix(positionsFound);
                    PrintHexes();
                }
                else
                {
                    PrintHexes();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        public void SaveFile(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_globalManager.FixManager.FixedHex.IsEmpty)
                {
                    var saveFileDialog = new SaveFileDialog
                    {
                        DefaultExt = ".bin",
                        Filter = "Pliki binarne (*.bin)|*.bin"
                    };
                    Nullable<bool> result = saveFileDialog.ShowDialog();
                    if (saveFileDialog.FileName != "")
                    {
                        _globalManager.FileManager.HexIO.WriteHex(saveFileDialog.FileName, _globalManager.FixManager.FixedHex);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if ((bool)SynchScrolls.IsChecked)
            {
                if (sender == FixedScroll)
                {
                    CorruptedScroll.ScrollToVerticalOffset(e.VerticalOffset);
                    CorruptedScroll.ScrollToHorizontalOffset(e.HorizontalOffset);
                }
                else
                {
                    FixedScroll.ScrollToVerticalOffset(e.VerticalOffset);
                    FixedScroll.ScrollToHorizontalOffset(e.HorizontalOffset);
                }
            }
        }

        private void PrintHexes()
        {
            FlowDocument hexDocument;
            hexDocument = new FlowDocument();
            if (_globalManager.FixManager.CorruptedHex.IsEmpty)
            {
                hexDocument.Blocks.Add(new Paragraph(new Run("Wczytany plik")));
                CorruptedText.Document = hexDocument;
            }
            else if ((!_globalManager.FixManager.CorruptedHex.IsEmpty) && _globalManager.FixManager.FixedHex.IsEmpty)
            {
                InsertHexToBlock(CorruptedText, _globalManager.FixManager.CorruptedHex);
            }

            hexDocument = new FlowDocument();
            if (_globalManager.FixManager.FixedHex.IsEmpty)
            {
                hexDocument.Blocks.Add(new Paragraph(new Run("Naprawiony plik")));
                FixedText.Document = hexDocument;
            }
            else if ((!_globalManager.FixManager.FixedHex.IsEmpty) && _globalManager.FixManager.CorruptedHex.IsEmpty)
            {
                InsertHexToBlock(FixedText, _globalManager.FixManager.FixedHex);
            }

            if (!_globalManager.FixManager.CorruptedHex.IsEmpty && !_globalManager.FixManager.FixedHex.IsEmpty)
            {
                InsertHexToBlock(CorruptedText, _globalManager.FixManager.CorruptedHex, _globalManager.FixManager.FixedHex);
                InsertHexToBlock(FixedText, _globalManager.FixManager.FixedHex, _globalManager.FixManager.CorruptedHex);
            }
        }

        private void InsertHexToBlock(RichTextBox targetTextBlock, Hex hexToInsert, Hex comparableHex = null)
        {
            if (comparableHex == null)
            {
                comparableHex = hexToInsert;
            }
            bool isDifferend = false;
            var hexDocument = new FlowDocument();
            var hexParagraph = new Paragraph();
            var hexRun = new Run();
            var hexBuilder = new StringBuilder();
            int byteIndex = 0;
            foreach (var fixedByte in hexToInsert)
            {
                if (byteIndex != 0 && byteIndex % 16 == 0)
                {
                    //hexRun.Text += "\n";
                    hexBuilder.Append("\n");
                }

                if (fixedByte != comparableHex[byteIndex] && !isDifferend)
                {
                    isDifferend = true;
                    hexRun.Text = hexBuilder.ToString();
                    hexParagraph.Inlines.Add(hexRun);
                    hexRun = new Run();
                    hexRun.Background = Brushes.Orange;
                    hexBuilder.Clear();
                }
                else if (fixedByte == comparableHex[byteIndex] && isDifferend)
                {
                    isDifferend = false;
                    hexRun.Text = hexBuilder.ToString();
                    hexParagraph.Inlines.Add(hexRun);
                    hexRun = new Run();
                    hexBuilder.Clear();
                }
                hexBuilder.Append(fixedByte.ToString("X2") + " ");
                byteIndex++;
            }
            hexRun.Text = hexBuilder.ToString();
            hexParagraph.Inlines.Add(hexRun);
            hexDocument.Blocks.Add(hexParagraph);
            targetTextBlock.Document = hexDocument;
        }

        private void OpenPdfSelectorClicked(object sender, RoutedEventArgs e)
        {
            if (_selectedInstructionPath == null || _selectedInstructionPath.Trim() == "")
                return;
            if (Directory.GetFiles(_selectedInstructionPath) == null || Directory.GetFiles(_selectedInstructionPath).Length == 0)
                return;
            foreach (var file in Directory.GetFiles(_selectedInstructionPath))
            {
                if (file.Substring(file.Length - 3) == "pdf")
                {
                    var selectorChild = new MenuItem
                    {
                        Header = file
                    };
                    selectorChild.Click += RunPdf;
                    OpenPdfSelector.Items.Add(selectorChild);
                }
            }
        }

        private void RunPdf(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start((string)((MenuItem)sender).Header);
            OpenPdfSelector.Items.Clear();
        }
    }
}