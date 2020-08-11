using HexChanger.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HexChanger
{
    /// <summary>
    /// Interaction logic for ConflictDialog.xaml
    /// </summary>
    public partial class ConflictDialog : Window
    {
        private Dictionary<int, List<CheckBox>> _checkBoxes = new Dictionary<int, List<CheckBox>>();
        public Dictionary<int, List<int>> SolvedConflicts { get; set; }
        private readonly Dictionary<int, List<int>> _inputData;
        private readonly string _currentInstructionPath;
        private CheckBox _lastClickedCheckBoxWithShift;
        private Dictionary<int, Grid> _internalGrids = new Dictionary<int, Grid>();

        public ConflictDialog(Dictionary<int, List<int>> positionsFound, string currentInstructionPath) : base()
        {
            InitializeComponent();
            _currentInstructionPath = currentInstructionPath;
            _inputData = positionsFound;

            int longestConflictList = 0;
            int columnIndex = 0;

            foreach (var findFileNumber in positionsFound.Keys)
            {
                if (positionsFound[findFileNumber].Count > longestConflictList)
                    longestConflictList = positionsFound[findFileNumber].Count;
                //if Value.Count is > 1 that means we got a coflict
                if (positionsFound[findFileNumber].Count <= 1)
                    continue;

                CheckBoxGrid.ColumnDefinitions.Add(new ColumnDefinition());
                //creating header in conflict column
                var label = new Label();
                string formatedHeader = findFileNumber.ToString();
                while (formatedHeader.Length < 3)
                    formatedHeader = "0" + formatedHeader;
                formatedHeader = "find" + formatedHeader;
                label.Content = formatedHeader;

                //seting header in right collumn
                Grid.SetColumn(label, columnIndex);
                Grid.SetRow(label, 0);
                CheckBoxGrid.Children.Add(label);
                columnIndex++;
            }

            int i = 0;
            foreach (var position in positionsFound)
            {
                if (position.Value.Count > 1)
                {
                    Grid internalGrid = new Grid
                    {
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    for (int rowsCount = 0; rowsCount < position.Value.Count; rowsCount++)
                    {
                        internalGrid.RowDefinitions.Add(new RowDefinition());
                    }
                    int j = 0;
                    var checkBoxes = new List<CheckBox>();
                    foreach (var index in position.Value)
                    {
                        var checkBox = new CheckBox
                        {
                            Content = index.ToString(),
                            IsChecked = true,
                            Height = 30
                        };
                        checkBox.Click += CheckBox_Click;

                        internalGrid.Children.Add(checkBox);
                        Grid.SetColumn(checkBox, 0);
                        Grid.SetRow(checkBox, j);
                        checkBoxes.Add(checkBox);
                        j++;
                    }
                    _internalGrids.Add(i, internalGrid);

                    ScrollViewer internalScroll = new ScrollViewer
                    {
                        Content = internalGrid,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Visible
                    };
                    CheckBoxGrid.Children.Add(internalScroll);
                    Grid.SetColumn(internalScroll, i);
                    Grid.SetRow(internalScroll, 1);
                    _checkBoxes.Add(position.Key, checkBoxes);
                    i++;
                }
            }
            LoadSettings();
            base.SizeChanged += ConflictDialog_SizeChanged;
        }

        private Dictionary<int, List<int>> Solve()
        {
            var solvedConficts = new Dictionary<int, List<int>>();
            foreach (var column in _checkBoxes)
            {
                List<int> indexes = new List<int>();
                foreach (var checkbox in column.Value)
                {
                    bool isChecked = false;
                    if (checkbox.IsChecked == null)
                        isChecked = false;
                    else
                        isChecked = (bool)checkbox.IsChecked;
                    if (isChecked)
                        indexes.Add(int.Parse(checkbox.Content.ToString()));
                }
                if (indexes.Count > 0)
                    solvedConficts.Add(column.Key, indexes);
            }
            foreach (var postion in _inputData)
                if (postion.Value.Count == 1)
                    solvedConficts.Add(postion.Key, postion.Value);
            return solvedConficts;
        }

        public void Submit(object sender, RoutedEventArgs e)
        {
            SolvedConflicts = Solve();
            Close();
        }

        public void OpenFindTxt(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(_currentInstructionPath + Path.DirectorySeparatorChar + "find.txt");
            }
            catch (Exception)
            {
                MessageBox.Show("Nie znaleziono pliku find.txt");
            }
        }

        private void LoadSettings()
        {
            base.Left = Settings.Default.ConflictDialogX;
            base.Top = Settings.Default.ConflictDialogY;
            base.Height = Settings.Default.ConflictDialogHeight;
            base.Width = Settings.Default.ConflictDialogWidth;
        }

        private void ConflictDialog_LocationChanged(object sender, EventArgs e)
        {
            Settings.Default.ConflictDialogX = base.Left;
            Settings.Default.ConflictDialogY = base.Top;
            Settings.Default.Save();
        }

        private void ConflictDialog_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Settings.Default.ConflictDialogHeight = base.Height;
            Settings.Default.ConflictDialogWidth = base.Width;
            Settings.Default.Save();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var clickedCheckBox = (CheckBox)sender;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (_lastClickedCheckBoxWithShift == null)
                    _lastClickedCheckBoxWithShift = clickedCheckBox;
                else
                {
                    int clickedColumn = Grid.GetColumn((ScrollViewer)((Grid)clickedCheckBox.Parent).Parent);
                    int lastColumn = Grid.GetColumn((ScrollViewer)((Grid)_lastClickedCheckBoxWithShift.Parent).Parent);

                    if (clickedColumn != lastColumn || _lastClickedCheckBoxWithShift == clickedCheckBox)
                    {
                        _lastClickedCheckBoxWithShift = null;
                        return;
                    }

                    int topRow;
                    int bottomRow;
                    if (Grid.GetRow(clickedCheckBox) > Grid.GetRow(_lastClickedCheckBoxWithShift))
                    {
                        topRow = Grid.GetRow(_lastClickedCheckBoxWithShift);
                        bottomRow = Grid.GetRow(clickedCheckBox);
                    }
                    else
                    {
                        topRow = Grid.GetRow(clickedCheckBox);
                        bottomRow = Grid.GetRow(_lastClickedCheckBoxWithShift);
                    }
                    foreach (CheckBox child in _internalGrids[clickedColumn].Children)
                    {
                        if (Grid.GetRow(child) >= topRow && Grid.GetRow(child) <= bottomRow)
                        {
                            if ((bool)_lastClickedCheckBoxWithShift.IsChecked)
                                child.IsChecked = true;
                            else
                                child.IsChecked = false;
                        }
                    }
                    _lastClickedCheckBoxWithShift = null;
                }
            }
            else
                _lastClickedCheckBoxWithShift = null;
        }
    }
}
