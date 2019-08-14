using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace HexChanger
{
    /// <summary>
    /// Interaction logic for ConflictDialog.xaml
    /// </summary>
    public partial class ConflictDialog : Window
    {
        private Dictionary<int, List<CheckBox>> _checkBoxes;
        public Dictionary<int, List<int>> SolvedConflicts { get; set; }
        private Dictionary<int, List<int>> _inputData;

        public ConflictDialog(Dictionary<int, List<int>> positionsFound) : base()
        {
            InitializeComponent();
            _inputData = positionsFound;
            int longestLength = 0;
            //header
            CheckGrid.RowDefinitions.Add(new RowDefinition());
            int c = 0;
            foreach (var position in positionsFound)
            {
                if (position.Value.Count > 1)
                {
                    CheckGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    var label = new Label
                    {
                        Content = position.Key.ToString()
                    };
                    Grid.SetColumn(label, c);
                    Grid.SetRow(label, 0);
                    CheckGrid.Children.Add(label);
                    c++;
                }
                if (position.Value.Count > longestLength)
                {
                    longestLength = position.Value.Count;
                }
            }

            for (int k = 0; k < longestLength; k++)
            {
                CheckGrid.RowDefinitions.Add(new RowDefinition());
            }

            _checkBoxes = new Dictionary<int, List<CheckBox>>();
            int i = 0;
            int j = 1;
            foreach (var position in positionsFound)
            {
                if (position.Value.Count > 1)
                {
                    j = 1;
                    var checkBoxes = new List<CheckBox>();
                    foreach (var index in position.Value)
                    {
                        var checkBox = new CheckBox
                        {
                            Content = index.ToString(),
                            Height = 30
                        };

                        Grid.SetColumn(checkBox, i);
                        Grid.SetRow(checkBox, j);
                        CheckGrid.Children.Add(checkBox);
                        checkBoxes.Add(checkBox);
                        j++;
                    }
                    _checkBoxes.Add(position.Key, checkBoxes);
                    i++;
                }
            }
            CheckGrid.Height = CheckGrid.RowDefinitions.Count * 30;
            CheckGrid.Width = CheckGrid.ColumnDefinitions.Count * 100;
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
                    {
                        isChecked = false;
                    }
                    else
                    {
                        isChecked = (bool)checkbox.IsChecked;
                    }
                    if (isChecked)
                    {
                        indexes.Add(int.Parse(checkbox.Content.ToString()));
                    }
                }
                if (indexes.Count > 0)
                {
                    solvedConficts.Add(column.Key, indexes);
                }
            }
            foreach (var postion in _inputData)
            {
                if (postion.Value.Count == 1)
                {
                    solvedConficts.Add(postion.Key, postion.Value);
                }
            }
            return solvedConficts;
        }

        public void Submit(object sender, RoutedEventArgs e)
        {
            SolvedConflicts = Solve();
            Close();
        }
    }
}
