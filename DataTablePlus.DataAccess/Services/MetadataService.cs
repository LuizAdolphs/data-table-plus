﻿/*******************************************************************************
 * You may amend and distribute as you like, but don't remove this header!
 * 
 * See https://github.com/ffernandolima/data-table-plus for details.
 *
 * Copyright (C) 2018 Fernando Luiz de Lima
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
 * See the GNU Lesser General Public License for more details.
 *
 * The GNU Lesser General Public License can be viewed at http://www.opensource.org/licenses/lgpl-license.php
 * If you unfamiliar with this license or have questions about it, here is a FAQ: http://www.gnu.org/licenses/gpl-faq.html
 *
 * All code and executables are provided "as is" with no warranty either express or implied. 
 * The author accepts no liability for any damage or loss of business that this product may cause.
 * 
 *******************************************************************************/

using DataTablePlus.Common;
using DataTablePlus.DataAccess.Extensions;
using DataTablePlus.DataAccess.Resources;
using DataTablePlus.DataAccessContracts.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DataTablePlus.DataAccess.Services
{
	/// <summary>
	/// Service that should be used to get some metadata
	/// </summary>
	public class MetadataService : ServiceBase, IMetadataService
	{
		/// <summary>
		/// Gets the table name from the mapped entity on EF
		/// </summary>
		/// <typeparam name="T">Type of the mapped entity</typeparam>
		/// <returns>Table name or null</returns>
		public string GetTableName<T>() where T : class
		{
			return this.GetTableName(typeof(T));
		}

		/// <summary>
		/// Gets the table name from the mapped entity on EF
		/// </summary>
		/// <param name="type">Type of the mapped entity</param>
		/// <returns>Table name or null</returns>
		public string GetTableName(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type), $"{nameof(type)} {CommonResources.App_CannotBeNull}");
			}

			return this.DbContext.GetTableName(type);
		}

		/// <summary>
		/// Gets a mapping between the model properties and the mapped column names
		/// </summary>
		/// <typeparam name="T">Type of the mapped entity</typeparam>
		/// <returns>Mapping or null</returns>
		public IDictionary<PropertyInfo, string> GetMappings<T>() where T : class
		{
			return this.GetMappings(typeof(T));
		}

		/// <summary>
		/// Gets a mapping between the model properties and the mapped column names
		/// </summary>
		/// <param name="type">Type of the mapped entity</param>
		/// <returns>Mapping or null</returns>
		public IDictionary<PropertyInfo, string> GetMappings(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type), $"{nameof(type)} {CommonResources.App_CannotBeNull}");
			}

			return this.DbContext.GetMappings(type);
		}

		/// <summary>
		/// Gets the entity keys
		/// </summary>
		/// <typeparam name="T">Type of the mapped entity</typeparam>
		/// <returns>A list that contains the entity keys</returns>
		public IList<string> GetKeyNames<T>() where T : class
		{
			return this.GetKeyNames(typeof(T));
		}

		/// <summary>
		/// Gets the entity keys
		/// </summary>
		/// <param name="type">Type of the mapped entity</param>
		/// <returns>A list that contains the entity keys</returns>
		public IList<string> GetKeyNames(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type), $"{nameof(type)} {CommonResources.App_CannotBeNull}");
			}

			return this.DbContext.GetKeyNames(type);
		}

		/// <summary>
		/// Gets the database table schema
		/// </summary>
		/// <param name="tableName">The name of the database table</param>
		/// <returns>A datatable that represents a mirror of the database table schema</returns>
		public DataTable GetTableSchema(string tableName)
		{
			if (string.IsNullOrWhiteSpace(tableName))
			{
				throw new ArgumentException($"{nameof(tableName)} {CommonResources.App_CannotBeNullOrWhiteSpace}", nameof(tableName));
			}

			DataTable dataTable;

			try
			{
				using (var command = this.SqlConnection.CreateCommand())
				{
					if (this.SqlConnection.State != ConnectionState.Open)
					{
						this.SqlConnection.Open();
					}

					command.CommandText = string.Format(DataResources.MetadataService_GetSchemaTable, tableName);
					command.CommandType = CommandType.Text;

					using (var reader = command.ExecuteReader())
					{
						dataTable = new DataTable
						{
							TableName = tableName
						};

						dataTable.BeginLoadData();
						dataTable.Load(reader);
						dataTable.EndLoadData();
					}

					if (this.SqlConnection.State != ConnectionState.Closed)
					{
						this.SqlConnection.Close();
					}
				}
			}
			catch
			{
				throw;
			}

			return dataTable;
		}

		#region IDisposable Members

		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (!this._disposed)
			{
				if (disposing)
				{
					base.Dispose(true);
				}
			}

			this._disposed = true;
		}

		#endregion
	}
}
