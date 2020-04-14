﻿using Hexes;
using Managers;
using Microsoft.Win32;
using System;
using System.IO;
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

        public MainWindow()
        {
            _globalManager = new GlobalManager();
            InitializeComponent();
            PrintHexes();
        }

        public void LoadFile(object sender, RoutedEventArgs e)
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
                    PrintHexes();
                    if (_globalManager.FixManager.IsSet())
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

        public void LoadInstruction(object sender, RoutedEventArgs e)
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
                    string directory = Path.GetDirectoryName(openFileDialog.FileName);
                    _globalManager.FileManager.InstructionDir = directory;
                    _globalManager.FixManager.InstructionSet = _globalManager.FileManager.ReadInstructions();
                    //Loaded.Text = _globalManager.FixManager.InstructionSet.ToString();
                    if (_globalManager.FixManager.IsSet())
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
                    throw new Exception("Nie znaleziono uszkodzonych segmentóws.");
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

            if (_globalManager.FixManager.CorruptedHex.IsEmpty)
            {
                hexDocument = new FlowDocument();
                hexDocument.Blocks.Add(new Paragraph(new Run("Wczytany plik")));
                CorruptedText.Document = hexDocument;
            }
            else if ((!_globalManager.FixManager.CorruptedHex.IsEmpty) && _globalManager.FixManager.FixedHex.IsEmpty)
            {
                hexDocument = new FlowDocument();
                hexDocument.Blocks.Add(new Paragraph(new Run(_globalManager.FixManager.CorruptedHex.ToString())));
                CorruptedText.Document = hexDocument;
            }

            if (_globalManager.FixManager.FixedHex.IsEmpty)
            {
                hexDocument = new FlowDocument();
                hexDocument.Blocks.Add(new Paragraph(new Run("Naprawiony plik")));
                FixedText.Document = hexDocument;
            }
            else if ((!_globalManager.FixManager.FixedHex.IsEmpty) && _globalManager.FixManager.CorruptedHex.IsEmpty)
            {
                hexDocument = new FlowDocument();
                hexDocument.Blocks.Add(new Paragraph(new Run(_globalManager.FixManager.FixedHex.ToString())));
                FixedText.Document = hexDocument;
            }

            if (!_globalManager.FixManager.CorruptedHex.IsEmpty && !_globalManager.FixManager.FixedHex.IsEmpty)
            {
                hexDocument = new FlowDocument();
                var hexParagraph = new Paragraph();
                int i = 0;
                foreach (var corruptedByte in _globalManager.FixManager.CorruptedHex)
                {
                    if (i != 0 && i % 16 == 0)
                    {
                        hexDocument.Blocks.Add(hexParagraph);
                        hexParagraph = new Paragraph();
                    }
                    var hexByte = new Run(corruptedByte.ToString("X2") + " ");
                    if (corruptedByte != _globalManager.FixManager.FixedHex[i])
                    {
                        hexByte.Background = Brushes.Orange;
                    }
                    hexParagraph.Inlines.Add(hexByte);
                    i++;
                }
                CorruptedText.Document = hexDocument;

                hexDocument = new FlowDocument();
                hexParagraph = new Paragraph();
                i = 0;
                foreach (var fixedByte in _globalManager.FixManager.FixedHex)
                {
                    if (i != 0 && i % 16 == 0)
                    {
                        hexDocument.Blocks.Add(hexParagraph);
                        hexParagraph = new Paragraph();
                    }
                    var hexByte = new Run(fixedByte.ToString("X2") + " ");
                    if (fixedByte != _globalManager.FixManager.CorruptedHex[i])
                    {
                        hexByte.Background = Brushes.Orange;
                    }
                    hexParagraph.Inlines.Add(hexByte);
                    i++;
                }
                FixedText.Document = hexDocument;
            }
        }
    }
}
