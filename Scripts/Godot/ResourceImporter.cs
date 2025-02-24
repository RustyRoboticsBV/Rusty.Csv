#if GODOT
using Godot;
using Godot.Collections;
using System;

namespace Rusty.Csv
{
    /// <summary>
    /// A base class for resource importers that can load a CSV table from a file and convert it to the desired resource type.
    /// </summary>
    public abstract partial class CsvResourceImporter<ResourceT> : Node where ResourceT : Resource
    {
        /* Public methods. */
        /// <summary>
        /// Load a resource from a file.
        /// </summary>
        public ResourceT Import(string filePath, Dictionary importOptions = null)
        {
            try
            {
                filePath = ProjectSettings.GlobalizePath(filePath);
                CsvTable table = CsvTable.Load(filePath);
                return Convert(table, importOptions);
            }
            catch (Exception ex)
            {
                GD.Print(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Load a resource from a file, using some importer.
        /// </summary>
        public static ResourceT Import<ImporterT>(string filePath, Dictionary importOptions = null)
            where ImporterT : CsvResourceImporter<ResourceT>, new()
        {
            ImporterT importer = new ImporterT();
            return importer.Import(filePath, importOptions);
        }

        /* Protected methods. */
        /// <summary>
        /// Convert a loaded CSV table into a resource.
        /// </summary>
        protected abstract ResourceT Convert(CsvTable table, Dictionary importOptions);
    }
}
#endif