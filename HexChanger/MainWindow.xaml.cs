using HexChanger.Properties;
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

        private enum Label_state
        {
            Valid,
            Invalid,
            Clear
        }

        public MainWindow()
        {
            _globalManager = new GlobalManager();
            InitializeComponent();
            ValidateSettings();
            PrintHexes();
            ValidateInstructionsCatalog();
            InitializeInstructionTree();
            LoadSettings();
            Application.Current.MainWindow.SizeChanged += MainWindow_SizeChanged;
        }

        private void LoadSettings()
        {
            Left = Settings.Default.MainWindowX;
            Top = Settings.Default.MainWindowY;
            Height = Settings.Default.MainWIndowHeight;
            Width = Settings.Default.MainWindowWidth;
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            Settings.Default.MainWindowX = Left;
            Settings.Default.MainWindowY = Top;
            Settings.Default.Save();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Settings.Default.MainWIndowHeight = Application.Current.MainWindow.Height;
            Settings.Default.MainWindowWidth = Application.Current.MainWindow.Width;
            Settings.Default.Save();
        }

        private void InitializeInstructionTree()
        {
            InstructionsTree.Items.Clear();
            TreeViewItemInstruction instructionTreeNode = CreateInstructionTreeNode(Settings.Default.InstructionsDirectory);
            InstructionsTree.Items.Add(instructionTreeNode);
            BuildInstructionTreeBranch(instructionTreeNode);
        }

        private void ValidateInstructionsCatalog()
        {
            string instructionsDirectory = Settings.Default.InstructionsDirectory;
            if (Directory.Exists(instructionsDirectory) && File.GetAttributes(instructionsDirectory).HasFlag(FileAttributes.Directory))
                return;
            Settings.Default.InstructionsDirectory = DriveInfo.GetDrives()[0].Name;
            Settings.Default.Save();
        }

        public void SelectInstrucionsCatalog(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Settings.Default.InstructionsDirectory = fbd.SelectedPath;
                    Settings.Default.Save();
                }
                _selectedInstructionPath = "";
                InstructionsTree.Items.Clear();
                ValidateInstructionsCatalog();
                InitializeInstructionTree();
            }
        }

        private void ValidateSettings()
        {
            RepairAfterSelectionSwitch.IsChecked = Settings.Default.RepairAfterSelection;
            IdentifyAfterSelectionSwitch.IsChecked = Settings.Default.IdentifyAfterSelection;
        }

        private void ChangeRepairAfterSelection(object sender, RoutedEventArgs e)
        {
            Settings.Default.RepairAfterSelection = RepairAfterSelectionSwitch.IsChecked.Value;
            Settings.Default.Save();
        }

        private void ChangeIdentifyAfterSelection(object sender, RoutedEventArgs e)
        {
            Settings.Default.IdentifyAfterSelection = IdentifyAfterSelectionSwitch.IsChecked.Value;
            Settings.Default.Save();
        }

        private void ChangeIdentifyLableState(Label_state state)
        {
            switch (state)
            {
                case Label_state.Valid:
                    IdentifiedLabel.Content = "Plik zidentyfikowany";
                    IdentifiedLabel.Background = Brushes.LightGreen;
                    break;
                case Label_state.Invalid:
                    IdentifiedLabel.Content = "Plik nie zidentyfikowany";
                    IdentifiedLabel.Background = Brushes.IndianRed;
                    break;
                case Label_state.Clear:
                    IdentifiedLabel.Content = "";
                    IdentifiedLabel.Background = Brushes.White;
                    break;
            }
        }

        private void ChangeFoundLabelState(Label_state state)
        {
            string fileName = _globalManager.FileManager.FindName;
            fileName = char.ToUpper(fileName[0]) + fileName.Substring(1);
            switch (state)
            {
                case Label_state.Valid:
                    SomethingFoundLabel.Content = fileName + " znaleziony";
                    SomethingFoundLabel.Background = Brushes.LightGreen;
                    break;
                case Label_state.Invalid:
                    SomethingFoundLabel.Content = fileName + " nie znaleziony";
                    SomethingFoundLabel.Background = Brushes.IndianRed;
                    break;
                case Label_state.Clear:
                    SomethingFoundLabel.Content = "";
                    SomethingFoundLabel.Background = Brushes.White;
                    break;
            }
        }

        private void BuildInstructionTreeBranch(TreeViewItemInstruction selectedNode)
        {
            string[] directories = Directory.GetDirectories(selectedNode.FullPath);
            foreach (string directory in directories)
            {
                try
                {
                    Directory.GetDirectories(directory);
                    TreeViewItemInstruction newChild = null;
                    foreach (TreeViewItemInstruction currentChild in selectedNode.Items)
                    {
                        if (currentChild.FullPath == directory)
                        {
                            newChild = currentChild;
                            break;
                        }
                    }
                    if (newChild == null)
                    {
                        newChild = CreateInstructionTreeNode(directory);
                        selectedNode.Items.Add(newChild);
                        if (directories.Length > 0 && !IsInstructionDirectory(newChild.FullPath))
                            newChild.Expanded += ExpandtInstructionDirectory;
                    }
                    if (!IsInstructionDirectory(directory))
                    {
                        foreach (string subDirectory in Directory.GetDirectories(directory))
                        {
                            try
                            {
                                //Check for permision.
                                Directory.GetDirectories(subDirectory);

                                TreeViewItemInstruction instructionTreeNode = CreateInstructionTreeNode(subDirectory);
                                newChild.Items.Add(instructionTreeNode);
                                if (directories.Length > 0)
                                    newChild.Expanded += ExpandtInstructionDirectory;
                            }
                            catch (Exception) { }
                        }
                    }
                }
                catch (Exception ex) { }
            }
        }

        private TreeViewItemInstruction CreateInstructionTreeNode(string directoryPath)
        {
            TreeViewItemInstruction newNode = new TreeViewItemInstruction();
            newNode.FullPath = directoryPath;
            if (Path.GetFileName(directoryPath).Trim() == "")
                newNode.Header = directoryPath;
            else
                newNode.Header = Path.GetFileName(directoryPath);
            if (IsInstructionDirectory(newNode.FullPath))
            {
                newNode.Selected += InstructionSelected;
                newNode.Unselected += InstructionUnselected;
                newNode.Background = Brushes.Orange;
            }
            return newNode;
        }

        private bool IsInstructionDirectory(string path)
        {
            Regex regex = new Regex(".*" + _globalManager.FileManager.IdentifyName + ".*");
            foreach (string file in Directory.GetFiles(path))
            {
                if (regex.IsMatch(file))
                    return true;
            }
            return false;
        }

        public void ExpandtInstructionDirectory(object sender, RoutedEventArgs e)
        {
            TreeViewItemInstruction selectedNode = (TreeViewItemInstruction)sender;
            if (!selectedNode.Items.IsEmpty)
            {
                foreach (TreeViewItemInstruction child in selectedNode.Items)
                {
                    if (!child.Items.IsEmpty)
                        return;
                }
            }
            BuildInstructionTreeBranch(selectedNode);
        }

        private void InstructionSelected(object sender, RoutedEventArgs e)
        {
            _selectedInstructionPath = ((TreeViewItemInstruction)sender).FullPath;
            GeneratePdfList();
            try
            {
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
            GeneratePdfList();
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

                    ChangeFoundLabelState(Label_state.Clear);
                    if (_globalManager.FixManager.IsInstructionSet() && (bool)IdentifyAfterSelectionSwitch.IsChecked)
                    {
                        if (_globalManager.FixManager.Identify())
                            ChangeIdentifyLableState(Label_state.Valid);
                        else
                            ChangeIdentifyLableState(Label_state.Invalid);
                    }

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
                    _selectedInstructionPath = Path.GetDirectoryName(openFileDialog.FileName);
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
            ChangeFoundLabelState(Label_state.Clear);
            if (_globalManager.FixManager.IsSet() && (bool)IdentifyAfterSelectionSwitch.IsChecked)
            {
                if (_globalManager.FixManager.Identify())
                {
                    ChangeIdentifyLableState(Label_state.Valid);
                }
                else
                {
                    ChangeIdentifyLableState(Label_state.Invalid);
                }
            }
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

                if (positionsFound != null)
                    ChangeFoundLabelState(Label_state.Valid);
                else
                    ChangeFoundLabelState(Label_state.Invalid);

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
                        ConflictDialog dialog = new ConflictDialog(positionsFound, _selectedInstructionPath);
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
                if (sender == FixedScroll || sender == FixedScrollAscii)
                {
                    CorruptedScroll.ScrollToVerticalOffset(e.VerticalOffset);
                    CorruptedScroll.ScrollToHorizontalOffset(e.HorizontalOffset);
                    CorruptedScrollAscii.ScrollToVerticalOffset(e.VerticalOffset);
                    if (sender == FixedScrollAscii)
                    {
                        FixedScroll.ScrollToVerticalOffset(e.VerticalOffset);
                    }
                }
                else if (sender == CorruptedScroll || sender == CorruptedScrollAscii)
                {
                    FixedScroll.ScrollToVerticalOffset(e.VerticalOffset);
                    FixedScroll.ScrollToHorizontalOffset(e.HorizontalOffset);
                    FixedScrollAscii.ScrollToVerticalOffset(e.VerticalOffset);
                    if(sender == CorruptedScrollAscii)
                    {
                        CorruptedScroll.ScrollToVerticalOffset(e.VerticalOffset);
                    }
                }
            }

            if (sender == FixedScroll)
            {
                FixedScrollAscii.ScrollToVerticalOffset(e.VerticalOffset);
                //FixedScrollAscii.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
            else if (sender == CorruptedScroll)
            {
                CorruptedScrollAscii.ScrollToVerticalOffset(e.VerticalOffset);
                //CorruptedScrollAscii.ScrollToHorizontalOffset(e.HorizontalOffset);
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
                InsertHexToBlock(CorruptedAscii, _globalManager.FixManager.CorruptedHex, null, true);
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
                InsertHexToBlock(FixedAscii, _globalManager.FixManager.FixedHex, null, true);
            }

            if (!_globalManager.FixManager.CorruptedHex.IsEmpty && !_globalManager.FixManager.FixedHex.IsEmpty)
            {
                InsertHexToBlock(CorruptedText, _globalManager.FixManager.CorruptedHex, _globalManager.FixManager.FixedHex);
                InsertHexToBlock(CorruptedAscii, _globalManager.FixManager.CorruptedHex, _globalManager.FixManager.FixedHex, true);
                InsertHexToBlock(FixedText, _globalManager.FixManager.FixedHex, _globalManager.FixManager.CorruptedHex);
                InsertHexToBlock(FixedAscii, _globalManager.FixManager.FixedHex, _globalManager.FixManager.CorruptedHex, true);
            }
        }

        private void InsertHexToBlock(RichTextBox targetTextBlock, Hex hexToInsert, Hex comparableHex = null, bool ascii = false)
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
                if (ascii)
                {
                    hexBuilder.Append(Encoding.ASCII.GetString(new byte[] { (byte)fixedByte }));
                }
                else
                {
                    hexBuilder.Append(fixedByte.ToString("X2") + " ");
                }
                byteIndex++;
            }
            hexRun.Text = hexBuilder.ToString();
            hexParagraph.Inlines.Add(hexRun);
            hexDocument.Blocks.Add(hexParagraph);
            targetTextBlock.Document = hexDocument;
        }

        private void GeneratePdfList()
        {
            OpenPdfSelector.Items.Clear();
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
        }
    }
}