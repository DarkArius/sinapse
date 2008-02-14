/***************************************************************************
 *   Sinapse Neural Network Tool         http://code.google.com/p/sinapse/ *
 *  ---------------------------------------------------------------------- *
 *   Copyright (C) 2006-2008 Cesar Roberto de Souza <cesarsouza@gmail.com> *
 *                                                                         *
 *                                                                         *
 *   This program is free software; you can redistribute it and/or modify  *
 *   it under the terms of the GNU General Public License as published by  *
 *   the Free Software Foundation; either version 3 of the License, or     *
 *   (at your option) any later version.                                   *
 *                                                                         *
 *   This program is distributed in the hope that it will be useful,       *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 *   GNU General Public License for more details.                          *
 *                                                                         *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Data;
using System.IO;

using AForge;

using Sinapse.Data.Structures;


namespace Sinapse.Data
{

    public enum NetworkSet : ushort { Training = 0, Validation = 1, Testing = 2 };


    [Serializable]
    internal sealed class NetworkDatabase
    {


        #region Const definitions
        public const string ColumnRoleId = "@_informationRoleId";
        public const string ColumnTrainingLayerId = "@_trainingLayerId";
        public const string ColumnComputedPrefix = "@_computed";
        #endregion


        //----------------------------------------

        private NetworkSchema m_networkSchema;
        private DataTable m_dataTable;

        [NonSerialized]
        internal FileSystemEventHandler DatabaseSaved;

        [NonSerialized]
        internal EventHandler DatabaseChanged;

        private string m_lastSavePath;


        //----------------------------------------


        #region Constructors
        public NetworkDatabase(NetworkSchema schema, DataTable dataTable)
        {
            this.m_lastSavePath = String.Empty;

            this.m_networkSchema = schema;
            this.m_dataTable = dataTable;

            this.createColumns(dataTable, schema);

            this.m_networkSchema.DataCategories.AutodetectCaptions(dataTable);
            this.m_networkSchema.DataRanges.AutodetectRanges(dataTable);
        }

        public NetworkDatabase(NetworkSchema schema)
        {
            this.m_lastSavePath = String.Empty;

            this.m_networkSchema = schema;
            this.m_dataTable = new DataTable();

            this.createColumns(m_dataTable, schema);

        }
        #endregion


        //----------------------------------------


        #region Properties
        internal NetworkSchema Schema
        {
            get { return this.m_networkSchema; }
        }

        internal DataTable DataTable
        {
            get { return this.m_dataTable; }
        }

        /*
        internal DataView TrainingSet
        {
            get { return; }
        }

        internal DataView TestingSet
        {
            get { return;
        }

        internal DataView ValidationSet
        {
            get { return; }
        }   */

        internal int TrainingLayerCount
        {
            get
            {
                return this.m_dataTable.DefaultView.ToTable(true, ColumnTrainingLayerId).Rows.Count;               
            }
        }

        internal string LastSavePath
        {
            get { return this.m_lastSavePath; }
        }

        internal bool IsSaved
        {
            get { return (this.m_lastSavePath != null && this.m_lastSavePath.Length > 0); }
        }
        #endregion


        //----------------------------------------


        #region Public Methods

        internal void ImportData(DataTable dataTable, NetworkSet networkSet, int trainingSet)
        {
            this.createColumns(dataTable, m_networkSchema, networkSet, trainingSet);
            this.m_dataTable.Merge(dataTable, false, MissingSchemaAction.Ignore);
        }


        /// <summary>
        /// Creates the desired set of input or output vectors based on the current data.
        /// </summary>
        /// <param name="set">The set of data.</param>
        internal TrainingVectors CreateVectors(NetworkSet set)
        {
            return this.CreateVectors(set, 0);
        }

        /// <summary>
        /// Creates the desired set of input or output vectors based on the current data.
        /// </summary>
        /// <param name="set">The set of data.</param>
        /// <param name="trainingSet">The training subset of the data.</param>
        /// <returns></returns>
        internal TrainingVectors CreateVectors(NetworkSet networkSet, ushort trainingLayer)
        {

            TrainingVectors vectors;
            string strQuery = String.Empty;

            switch (networkSet)
            {
                case NetworkSet.Testing:
                    strQuery = String.Format("[{0}] = {1}",
                        ColumnRoleId, (ushort)NetworkSet.Testing);
                    break;

                
                case NetworkSet.Training:
                    strQuery = String.Format("[{0}] = {1}",
                    ColumnRoleId, (ushort)NetworkSet.Training);

                    if (trainingLayer > 0)
                    {
                        strQuery = String.Format("{0} AND [{1}] = {2}",
                            strQuery, ColumnTrainingLayerId, trainingLayer);
                    }
                    break;

             
                case NetworkSet.Validation:
                    strQuery = String.Format("[{0}] = {1}",
                        ColumnRoleId, (ushort)NetworkSet.Validation);
                    break;
            }


            DataRow[] query = m_dataTable.Select(strQuery);

            vectors.Input = new double[query.Length][];
            vectors.Output = new double[query.Length][];

            for (int i = 0; i < query.Length; ++i)
            {
                vectors.Input[i] = this.NormalizeRow(query[i], m_networkSchema.InputColumns);
                vectors.Output[i] = this.NormalizeRow(query[i], m_networkSchema.OutputColumns);
            }

            return vectors;
        }

        /// <summary>
        /// Normalizes the given datarow so the data can be interpreted by a neural network. Use the RevertRow method to revert the row back to its original state
        /// </summary>
        /// <param name="sourceRow">The source datarow</param>
        /// <param name="columnList">The list of column names</param>
        /// <returns>Returns the normalized vector</returns>
        internal double[] NormalizeRow(DataRow sourceRow, string[] columnList)
        {
            double[] doubleData = new double[columnList.Length];

            for (int i = 0; i < columnList.Length; i++)
            {
                string columnName = columnList[i];

                DoubleRange range = this.m_networkSchema.DataRanges.GetRange(columnName);
                bool hasCaption = (Array.IndexOf(this.m_networkSchema.StringColumns, columnName) >= 0);

                double data;

                if (hasCaption)
                    data = this.m_networkSchema.DataCategories.GetID(columnName, (string)sourceRow[columnName]);
                else
                {
                    string strData = (string)sourceRow[columnName];
                    if (strData.Length > 0)
                        data = Double.Parse(strData);
                    else
                        data = 0;
                }

                doubleData[i] = (data - range.Min) / (range.Max - range.Min);
            }

            return doubleData;
        }

        /// <summary>
        /// Reverts any normalized output vector back to its original state.
        /// </summary>
        /// <param name="dataRow">The current data row</param>
        /// <param name="columnList">The list of column names used in the normalized vector</param>
        /// <param name="normalizedData">The normalized vector</param>
        internal void RevertRow(DataRow dataRow, string[] columnList, double[] normalizedData)
        {
            for (int i = 0; i < columnList.Length; i++)
            {
                string columnName = columnList[i];

                DoubleRange range = this.m_networkSchema.DataRanges.GetRange(columnName);
                bool hasCaption = (Array.IndexOf(this.m_networkSchema.StringColumns, columnName) >= 0);

                double data = normalizedData[i] * (range.Max - range.Min) + range.Min;

                if (hasCaption)
                    dataRow[columnName] = this.m_networkSchema.DataCategories.GetCaption(columnName, (int)Math.Round(data));
                else
                    dataRow[columnName] = data.ToString();
            }
        }
        #endregion


        //----------------------------------------


        #region Private Methods
        /// <summary>
        /// Creates the internal structure needed in the datatable, such as hidden columns and support data
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="schema"></param>
        private void createColumns(DataTable dataTable, NetworkSchema schema)
        {
            this.createColumns(dataTable, schema, NetworkSet.Training, 1);
        }

        /// <summary>
        /// Creates the internal structure needed in the datatable, such as hidden columns and support data
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="schema"></param>
        /// <param name="networkSet"></param>
        /// <param name="trainingSet"></param>
        private void createColumns(DataTable dataTable, NetworkSchema schema, NetworkSet networkSet, int trainingSet)
        {
            DataColumn col;

            if (!dataTable.Columns.Contains(ColumnRoleId))
            {
                col = new DataColumn(ColumnRoleId, typeof(ushort));
                col.AllowDBNull = false;
                col.DefaultValue = (ushort)networkSet;
                dataTable.Columns.Add(col);
            }

            if (!dataTable.Columns.Contains(ColumnTrainingLayerId))
            {
                col = new DataColumn(ColumnTrainingLayerId, typeof(int));
                col.AllowDBNull = false;
                col.DefaultValue = trainingSet;
                dataTable.Columns.Add(col);
            }

            foreach (String colName in schema.AllColumns)
            {
                if (!dataTable.Columns.Contains(colName))
                {
                    col = new DataColumn(colName, typeof(string));
                    col.AllowDBNull = false;
                    col.DefaultValue = String.Empty;
                    dataTable.Columns.Add(col);
                }
            }

            foreach (String colName in schema.OutputColumns)
            {
                if (!dataTable.Columns.Contains(ColumnComputedPrefix + colName))
                {
                    col = new DataColumn(ColumnComputedPrefix + colName, typeof(string));
                    col.AllowDBNull = false;
                    col.DefaultValue = String.Empty;
                    dataTable.Columns.Add(col);
                }
            }
        }
        #endregion


        //----------------------------------------


        #region Object Events
        private void OnDatabaseSaved(FileSystemEventArgs e)
        {
            if (this.DatabaseSaved != null)
                this.DatabaseSaved.Invoke(this, e);
        }

        private void OnDatabaseChanged()
        {
            if (this.DatabaseChanged != null)
                this.DatabaseChanged.Invoke(this, EventArgs.Empty);
        }
        #endregion


        //----------------------------------------


        #region Static Methods
        public static void Serialize(NetworkDatabase networkDatabase, string path)
        {
            FileStream fileStream = null;
            bool success = true;

            try
            {
                fileStream = new FileStream(path, FileMode.Create);

                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fileStream, networkDatabase);
            }
            catch (DirectoryNotFoundException e)
            {
                Debug.WriteLine("Directory not found during network serialization");
                success = false;
                throw e;
            }
            catch (SerializationException e)
            {
                Debug.WriteLine("Error occured during serialization");
                success = false;
                throw e;
            }
            catch (Exception e)
            {
                success = false;
                throw e;
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();

                if (success)
                    networkDatabase.m_lastSavePath = path;
            }
        }

        public static NetworkDatabase Deserialize(string path)
        {
            NetworkDatabase ndb = null;
            FileStream fileStream = null;
            bool success = true;

            try
            {
                fileStream = new FileStream(path, FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                ndb = (NetworkDatabase)bf.Deserialize(fileStream);
            }
            catch (FileNotFoundException e)
            {
                Debug.WriteLine("File not found during network deserialization");
                success = false;
                throw e;
            }
            catch (SerializationException e)
            {
                Debug.WriteLine("Error occured during deserialization");
                success = false;
                throw e;
            }
            catch (Exception e)
            {
                success = false;
                throw e;
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();

                if (success)
                {
                    ndb.m_lastSavePath = path;
                    ndb.OnDatabaseSaved(new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(path), Path.GetFileName(path)));
                }
            }

            return ndb;
        }

        #endregion


    }
}
