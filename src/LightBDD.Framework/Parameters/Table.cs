﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LightBDD.Core.Execution;
using LightBDD.Core.Formatting.Values;
using LightBDD.Core.Results.Parameters;
using LightBDD.Core.Results.Parameters.Tabular;
using LightBDD.Framework.Formatting.Values;
using LightBDD.Framework.Results.Implementation;

namespace LightBDD.Framework.Parameters
{
    //TODO: consider moving to root
    //TODO: consider introducing IComplexParameter (that won't necessarily is verifiable)
    public class Table<TRow> : IComplexParameter, ISelfFormattable, IReadOnlyList<TRow>
    {
        private readonly IReadOnlyList<TRow> _rows;
        private readonly TableColumn[] _columns;
        private IValueFormattingService _formattingService = ValueFormattingServices.Current;

        public Table(IReadOnlyList<TRow> rows, IEnumerable<TableColumn> columns)
        {
            _rows = rows;
            _columns = columns.ToArray();
        }

        void IComplexParameter.SetValueFormattingService(IValueFormattingService formattingService)
        {
            _formattingService = formattingService;
        }

        IParameterDetails IComplexParameter.Details => new TabularParameterDetails(GetColumns(), GetRows());

        private IEnumerable<ITabularParameterRow> GetRows()
        {
            return _rows.Select(GetRow);
        }

        private ITabularParameterRow GetRow(TRow row, int index)
        {
            return new TabularParameterRow(index, _columns.Select(x => _formattingService.FormatValue(x.GetValue(row))));
        }

        private IEnumerable<ITabularParameterColumn> GetColumns()
        {
            return _columns.Select(x => new TabularParameterColumn(x.Name, false));
        }

        /// <summary>
        /// Returns inline representation of table
        /// </summary>
        public string Format(IValueFormattingService formattingService)
        {
            return "<table>";
        }

        public int Count => _rows.Count;

        public IEnumerator<TRow> GetEnumerator()
        {
            return _rows.AsEnumerable().GetEnumerator();
        }

        public TRow this[int index] => _rows[index];
        public IReadOnlyList<TableColumn> Columns => _columns;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}