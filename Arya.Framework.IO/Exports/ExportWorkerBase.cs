﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Arya.Framework.Common;
using Arya.Framework.Common.ComponentModel;
using Arya.Framework.Common.Extensions;
using Arya.Framework.Data.AryaDb;
using Arya.Framework.Utility;

namespace Arya.Framework.IO.Exports
{

    #region Enumerations

    public enum SortOrder
    {
        [DisplayTextAndValue("Order by Attribute Names only", null)]
        OrderbyAttributeNameOnly,
        [DisplayTextAndValue("Order by Disp.-Nav.-Attributes", null)]
        OrderbyDisplayNavigation,
        [DisplayTextAndValue("Order by Nav.-Disp.-Attributes", null)]
        OrderbyNavigationDisplay
    }

    #endregion Enumerations

    [TypeConverter(typeof(PropertySorter)), DefaultProperty("Taxonomies"), Serializable]
    public abstract class ExportWorkerBase : WorkerBase, ISerializable, IComponent
    {
        #region Fields

        protected readonly List<DataTable> ExportDataTables = new List<DataTable>();

        private AryaDbDataContext _currentDb;

        #endregion Fields

        #region Constructors

        [Obsolete("This was required for Arya Legacy. Not necessary for Web Version.")]
        protected ExportWorkerBase(string argumentFilePath, PropertyGrid ownerPropertyGrid, Guid userId, Guid projectId)
            : base(false)
        {
            Arguments.UserId = userId;
            Arguments.ProjectId = projectId;
            State = WorkerState.Working;

            ////Hookup Refresh event with Collection Changes
            //if (ownerPropertyGrid != null)
            //{
            //    Taxonomies.ItemChanged += (sender, args) =>
            //                                  {
            //                                      ownerPropertyGrid.Refresh();
            //                                      autoGeneratedFileName = MakeValidFileName();
            //                                  };

            //    PropertyChanged += (sender, args) => ownerPropertyGrid.Refresh();
            //}
        }

        protected ExportWorkerBase(string argumentDirectoryPath, Type argumentsType)
            : base(argumentDirectoryPath, argumentsType)
        {
        }

        protected ExportWorkerBase(string argumentDirectoryPath)
            : base(argumentDirectoryPath, typeof(ExportArgs))
        {
        }

        [Obsolete("This was required for Arya Legacy. Not necessary for Web Version.")]
        protected ExportWorkerBase(SerializationInfo info, StreamingContext ctxt)
            : base(Environment.CurrentDirectory, null)
        {
            ((ExportArgs)Arguments).FieldDelimiter = (Delimiter)info.GetValue("FieldDelimiter", typeof(Delimiter));
        }

        #endregion Constructors

        #region Enumerations

        public enum SaveFileType
        {
            [DisplayTextAndValue("Text", "txt")]
            Text,
            [DisplayTextAndValue("Excel", "xlsx")]
            Excel,
            [DisplayTextAndValue("XML", "xml")]
            Xml
        }

        public enum SourceType
        {
            [DisplayTextAndValue("Taxonomy", "Taxonomy")]
            Taxonomy,
            [DisplayTextAndValue("Sku List", "SkuList")]
            SkuList
        }

        #endregion Enumerations

        #region Events

        public event EventHandler Disposed;

        #endregion Events

        #region Properties

        [Browsable(false)]
        protected AryaDbDataContext CurrentDb
        {
            get { return _currentDb ?? (_currentDb = new AryaDbDataContext(Arguments.ProjectId, Arguments.UserId)); }
        }

        [Browsable(false)]
        public ISite Site
        {
            get { return new ExportDesignerVerbSite(this); }
            set { throw new NotImplementedException(); }
        }

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            // never called in this specific context with the PropertyGrid
            // but just reference the required Disposed event to avoid warnings
            if (Disposed != null)
                Disposed(this, EventArgs.Empty);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("FieldDelimiter", ((ExportArgs)Arguments).FieldDelimiter);
        }

        public static IEnumerable<ExportWorkerBase> GetExports(string name = null)
        {
            var exports =
                Assembly.GetExecutingAssembly().GetTypes().Where(p => p.IsSubclassOf(typeof(ExportWorkerBase)));

            if (!string.IsNullOrWhiteSpace(name))
                exports = exports.Where(exp => exp.FullName == name);

            return exports.Select(p => (ExportWorkerBase)Activator.CreateInstance(p, new object[] { string.Empty }));
        }

        public void ResetWorkerStatus()
        {
            StatusMessage = "Ready";
            CurrentProgress = 0;
            MaximumProgress = 0;
            State = WorkerState.Ready;
        }

        public override void Run()
        {
            State = WorkerState.Working;

            FetchExportData();

            SaveExportData();

            State = WorkerState.Complete;
        }

        public virtual List<string> ValidateInput()
        {
            var errors = new List<string>(2);

            var exportArgs = (ExportArgs)Arguments;
            //if (string.IsNullOrWhiteSpace(exportArgs.ExportFileName))
            //    errors.Add("Export file name not selected.");

            if (!exportArgs.SourceSelected)
                errors.Add("Taxonomies must be selected for export.");

            return errors;
        }

        internal int GetMaxTaxonomyLength(AryaDbDataContext dc)
        {
            var maxLength = 0;
            //maxLength= var maxLength = 0;

            var exportTaxonomies =
                dc.TaxonomyInfos.Where(p => ((ExportArgs)Arguments).TaxonomyIds.Contains(p.ID)).ToList();
            var allChildren = exportTaxonomies.SelectMany(p => p.AllChildren).Distinct().ToList();
            var allLeafChildren = exportTaxonomies.SelectMany(p => p.AllLeafChildren).Distinct().ToList();

            var nodes = allChildren.Count > allLeafChildren.Count ? allChildren : allLeafChildren;

            if (nodes.Count > 0)
                maxLength = nodes.Select(ti => ti.GetNodeDepth()).Max();
            //if (allChildren.Count == 0 && allLeafChildren.Count != 0)
            //{
            //    maxLength =
            //        allLeafChildren.Select(
            //            child => child.ToString().Split(new[] {TaxonomyInfo.DELIMITER}, StringSplitOptions.None).Length)
            //            .Max();
            //}
            //else if (allLeafChildren.Count == 0 && allChildren.Count != 0)
            //{
            //    maxLength =
            //        allChildren.Select(
            //            child => child.ToString().Split(new[] {TaxonomyInfo.DELIMITER}, StringSplitOptions.None).Length)
            //            .Max();
            //}
            //else if (allLeafChildren.Count != 0 && allChildren.Count != 0)
            //{
            //    if (allLeafChildren.Count >= allChildren.Count)
            //    {
            //        maxLength =
            //            allLeafChildren.Select(
            //                child =>
            //                    child.ToString().Split(new[] {TaxonomyInfo.DELIMITER}, StringSplitOptions.None).Length)
            //                .Max();
            //    }
            //    else
            //    {
            //        maxLength =
            //            allChildren.Select(
            //                child =>
            //                    child.ToString().Split(new[] {TaxonomyInfo.DELIMITER}, StringSplitOptions.None).Length)
            //                .Max();
            //    }
            //}
            else
            {
                StatusMessage = "There was no data to export.";
                MaximumProgress = 1;
                CurrentProgress = 1;
                State = WorkerState.Ready;
            }
            if (((ExportArgs)Arguments).IgnoreT1Taxonomy && maxLength > 1)
                maxLength--;
            return maxLength;
        }

        protected static decimal GetRank(SchemaData schemaData, SortOrder orderAttributesBy)
        {
            if (schemaData == null)
                return Int32.MaxValue;

            if (orderAttributesBy == SortOrder.OrderbyNavigationDisplay)
            {
                return schemaData.NavigationOrder > 0
                    ? schemaData.NavigationOrder
                    : schemaData.DisplayOrder > 0 ? schemaData.DisplayOrder + 1000 : Int32.MaxValue;
            }

            if (orderAttributesBy == SortOrder.OrderbyDisplayNavigation)
            {
                return schemaData.DisplayOrder > 0
                    ? schemaData.DisplayOrder
                    : schemaData.NavigationOrder > 0 ? schemaData.NavigationOrder + 1000 : Int32.MaxValue;
            }

            return 0;
        }

        protected abstract void FetchExportData();

        protected virtual void SaveExportData()
        {
            foreach (var table in ExportDataTables)
            {
                var exportArgs = (ExportArgs)Arguments;
                var filePath = Path.Combine(ArgumentDirectoryPath,
                    string.Format("{0}_{1}.{2}", exportArgs.BaseFilename, table.TableName,
                        exportArgs.ExportFileType.GetValue()));

                switch (exportArgs.ExportFileType)
                {
                    case SaveFileType.Text:
                        table.SaveTextFile(filePath, exportArgs.FieldDelimiter.GetValue().ToString());
                        break;
                    case SaveFileType.Excel:
                        try
                        {
                            table.SaveExcelFile(filePath);
                        }
                        catch (Exception)
                        {
                            table.SaveTextFile(filePath, exportArgs.FieldDelimiter.GetValue().ToString());
                        }
                        break;
                    case SaveFileType.Xml:
                        try
                        {
                            foreach (DataColumn dataColumn in table.Columns)
                                dataColumn.ColumnName = dataColumn.ColumnName.Replace(' ', '_');
                            table.SaveXmlFile(filePath);
                        }
                        catch (Exception)
                        {
                            table.SaveTextFile(filePath, exportArgs.FieldDelimiter.GetValue().ToString());
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion Methods
    }
}