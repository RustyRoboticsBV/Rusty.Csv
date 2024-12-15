#if GODOT
using Godot;
using Godot.Collections;

namespace Rusty.Csv
{
    /// <summary>
    /// A class that can load a CSV table from a file and return it as a resource of some type.
    /// </summary>
    public abstract partial class CsvImporter<ResourceT> : Node where ResourceT : Resource
    {
        /* Public methods. */
        /// <summary>
        /// Load a resource from a file.
        /// </summary>
        public ResourceT Import(string filePath, Dictionary options)
        {
            CsvTable table = CsvTable.Load(filePath);
            return Convert(table, options);
        }

        /// <summary>
        /// Load a resource from a file, using some importer.
        /// </summary>
        public static ResourceT Import<ImporterT>(string filePath, Dictionary options)
            where ImporterT : CsvImporter<ResourceT>, new()
        {
            ImporterT importer = new ImporterT();
            return importer.Import(filePath, options);
        }

        /* Protected methods. */
        /// <summary>
        /// Convert a loaded CSV table into a resource.
        /// </summary>
        protected abstract ResourceT Convert(CsvTable table, Dictionary options);
    }
}
#endif