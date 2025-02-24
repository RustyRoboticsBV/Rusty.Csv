# Rusty.CSV
An engine-agnostic CSV parser, implemented in C#. It can be used in both the Godot and Unity game engines, as well as in plain C#.

## Usage

### Loading a CSV file
Here's how you'd open a CSV file and retrieve a cell's contents:

    CSVTable table = new CSVTable("path/to/file.csv");      // Load the CSV file.
    string cell = table[0, 1];                              // Retrieve the cell at column 0, row 1.

The `CSVTable` class is essentially a 2D array of string objects. Some further conversions may be necessary before the data can be used in-game.

### Godot resource loading
For the Godot game engine, you can create custom resource loaders that use the `CSVTable` class. Here's an example of creating a simple resource loader for a custom 'ItemDatabase' resource:

Item datatabe resource script:

    using Godot;

    [GlobalClass]
    public partial class ItemDatabase : Resource
    {
        [Export] public Item[] Items { get; set; }
    }

Item resource script:

    using Godot;

    [GlobalClass]
    public partial class Item : Resource
    {
        [Export] public string Name { get; set; }
        [Export] public int Value { get; set; }
    }

Resource loader script:

    Using Godot;
    using Rusty.CSV;
    
    public partial class ItemLoader : ResourceImporter<Item>
    {
        protected override Item Convert(CSVTable table, Dictionary importOptions)
        {
            Item[] items = new Item[table.Height];
            for (int i = 0; i < table.Height; i++)
            {
                items[i] = new Item()
                {
                    Name = table["Name", i];
                    Value = int.Parse(table["Value", i]);
                };
            }
            return new ItemDatabase()
            {
                Items = items;
            };
        }
    }
