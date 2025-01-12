using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Forms; // for Keys enum

namespace FF8AngeloSearch;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const double timerTick = 0.1;

    private bool _hookEnabled = false;
    private int _manualKeyPressCount = 0;
    private int _gameValue = 0;
    private DispatcherTimer _timer;

    private DateTime _13Start;

    private List<Row> _rows = new List<Row>();
    private List<Item> _items = new List<Item>();

    private List<Row> _possibleRows = new List<Row>();

    private Row _currentRow;

    private Keys _manualKey = Keys.R;
    private Keys _resetKey = Keys.J;

    public MainWindow()
    {
        InitializeComponent();

        KeyboardHook.KeyPressed += OnKeyPressed;

        // Load options from XML file
        var optionsPath = "Options.xml";
        if (File.Exists(optionsPath))
        {
            var doc = new System.Xml.XmlDocument();
            doc.Load(optionsPath);

            var incrementKeyNode = doc.SelectSingleNode("/Options/IncrementKey");
            var resetKeyNode = doc.SelectSingleNode("/Options/ResetKey");

            if (incrementKeyNode != null)
            {
            _manualKey = (Keys)Enum.Parse(typeof(Keys), incrementKeyNode.InnerText);
            }

            if (resetKeyNode != null)
            {
            _resetKey = (Keys)Enum.Parse(typeof(Keys), resetKeyNode.InnerText);
            }
        }

        _rows.Add(new Row(1, 4,  "00:00:53"));
        _rows.Add(new Row(2, 32, "00:07:07"));
        _rows.Add(new Row(3, 86, "00:19:07"));
        _rows.Add(new Row(4, 32, "00:07:07"));
        _rows.Add(new Row(5, 19, "00:04:13"));
        _rows.Add(new Row(6, 41, "00:09:07"));
        _rows.Add(new Row(7, 1,  "00:00:13"));
        _rows.Add(new Row(8, 18, "00:04:00"));
        _rows.Add(new Row(9, 5,  "00:01:07"));

        _items = ParseItemsFromText(_rows, File.ReadAllText("items.txt"));
        ItemListBox.ItemsSource = _items.Select(i => i.Name).Distinct().OrderBy(name => name).ToList();

        _possibleRows.AddRange(_rows);

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(timerTick);
        _timer.Tick += OnTimerTick;
    }

    private void OnKeyPressed(Keys key)
    {
        if (key == _manualKey)
        {
            OnManualKeyPressed();
        }
        else if (key == _resetKey)
        {
            OnResetKeyPressed();
        }
    }

    private void OnManualKeyPressed()
    {
        Dispatcher.Invoke(() =>
        {
            _manualKeyPressCount++;
            RButtonPressLabel.Content = $"Manual Increment: {_manualKeyPressCount}";
        });
    }

    private void OnResetKeyPressed()
    {
        Dispatcher.Invoke(() =>
        {
            _gameValue = 0;
            _manualKeyPressCount = 0;
            JButtonPressLabel.Content = $"Game Value: {_gameValue}";
            RButtonPressLabel.Content = $"Manual Increment: {_manualKeyPressCount}";
            _timer.Stop();
            _13Start = DateTime.Now;
            _timer.Start();
        });
    }

    private void OnTimerTick(object sender, EventArgs e)
    {
        TimeSpan _13diff = DateTime.Now - _13Start;
        if (_13diff.TotalMilliseconds >= 13300)
        {
            _gameValue++;
            _13Start = DateTime.Now;
        }
        TickProgressBar.Value = _13diff.TotalMilliseconds;
        JButtonPressLabel.Content = $"Game Value: {_gameValue + _manualKeyPressCount}";
        TimeElapsedLabel.Content = $"Time Elapsed: {_13diff.Seconds}.{_13diff.Milliseconds:D3} seconds";
    }

    private void ToggleHookButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_hookEnabled)
        {
            // Install the hook
            KeyboardHook.InstallHook();
            _hookEnabled = true;
            ToggleHookButton.Content = "Disable Hook";
        }
        else
        {
            // Uninstall the hook
            KeyboardHook.UninstallHook();
            _hookEnabled = false;
            ToggleHookButton.Content = "Enable Hook";
        }
    }

    // When the window closes, always make sure to uninstall the hook
    protected override void OnClosed(EventArgs e)
    {
        if (_hookEnabled)
        {
            KeyboardHook.UninstallHook();
        }
        // Unsubscribe from event to avoid potential memory leaks
        KeyboardHook.KeyPressed -= OnKeyPressed;
        
        base.OnClosed(e);
    }

    private List<Item> ParseItemsFromText(List<Row> rows, string text)
    {
        var items = new List<Item>();
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var header = lines[1].Split('|').Select(c => c.Trim()).ToArray();

        for (int i = 3; i < lines.Length; i++) // Skip header lines
        {
            var columns = lines[i].Split('|').Select(c => c.Trim()).ToArray();
            if (columns.Length < 7) continue;

            var rowNumber = int.Parse(columns[1]);
            var row = rows.FirstOrDefault(r => r.Number == rowNumber);
            if (row == null) continue;

            for (int j = 2; j <= 7; j++)
            {
                var itemName = columns[j].Split('(').First().Trim();
                var dropChance = header[j].Split('(').Last().TrimEnd(')');

                var existingItem = items.FirstOrDefault(i => i.Name == itemName);
                if (existingItem != null)
                {
                    existingItem.Rows.Add(row);
                    row.Items.Add(existingItem);
                }
                else
                {
                    var item = new Item
                    {
                        Name = itemName,
                        DropChance = dropChance,
                        Rows = new List<Row> { row }
                    };
                    row.Items.Add(item);
                    items.Add(item);
                }
            }
        }

        return items;
    }

    private void ConfirmSelectionButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedItem = ItemListBox.SelectedItem as string;
        if (selectedItem == null) return;

        var selectedItems = _items.Where(i => i.Name == selectedItem).ToList();
        if (selectedItems == null) return;

        var _possibleClicks = selectedItems.Select(item => "[ " + string.Join(" | ", item.Rows.OrderBy(r=> r.Clicks).Select(r => $"{r.Clicks}")) + " ]");

        _possibleRows = _possibleRows.Where(r => selectedItems.Any(i => r.Items.Contains(i))).ToList();

        if (_possibleRows.Count == 1)
        {
            _currentRow = _possibleRows.First();
            _possibleClicks = selectedItems.Select(item => "[ " + string.Join(" | ", item.Rows.Where(r => r.Number == _currentRow.Number).OrderBy(r=> r.Clicks).Select(r => $"{r.Clicks}")) + " ]");

            _possibleRows = _rows.Where(r => r.Number == ((_currentRow.Number == 9 ? 0 : _currentRow.Number) + 1)).ToList();
        }
        else
        {
            var _newRows = selectedItems.SelectMany(i => i.Rows).Distinct().ToList();
            _possibleRows = _newRows.Select(r => new { Number = r.Number == 9 ? 1 : r.Number + 1 })
                                         .Distinct()
                                         .SelectMany(n => _rows.Where(r => r.Number == n.Number))
                                         .ToList();
        }

        ItemListBox.ItemsSource = _possibleRows.SelectMany(r => r.Items)
                                               .Distinct()
                                               .OrderBy(i => i.Name)
                                               .Select(item => $"{item.Name}")
                                               .ToList();

        PossibleClicksLabel.Content = $"Possible Clicks: {string.Join(", ", _possibleClicks)}";
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        _possibleRows.Clear();
        _possibleRows.AddRange(_rows);

        _currentRow = null;

        ItemListBox.ItemsSource = _items.Select(i => i.Name).Distinct().OrderBy(name => name).ToList();
        PossibleClicksLabel.Content = "Possible Clicks: ";
    }
}

public class Item
{
    public string Name { get; set; }
    public List<Row> Rows { get; set; }
    public string DropChance { get; set; }
}

public class Row
{
    public int Number { get; set; }
    public int Clicks { get; set; }
    public TimeSpan Duration { get; set; }
    public List<Item> Items { get; set; }

    public Row(int number, int clicks, string duration)
    {
        Number = number;
        Clicks = clicks;
        Duration = TimeSpan.Parse(duration);
        Items = new List<Item>();
    }
}