using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSSQLQueue
{
    public class SqlTable
    {
        public enum SqlColumnDataType
        {
            nvarchar = 0x11,
            n_integer,
            n_decimal,
            bit,
            datetime
        }
        public class SqlColumn
        {
            public string Name { get; set; }
            public SqlColumnDataType DataType { get; set; }
            public bool UniqueId { get; set; }

            public SqlColumn(string name, TaskQueue.TItemValue_Type vt, bool unique = false)
            {
                switch (vt)
                {
                    case TaskQueue.TItemValue_Type.text:
                        DataType = SqlColumnDataType.nvarchar;
                        break;
                    case TaskQueue.TItemValue_Type.num_int:
                        DataType = SqlColumnDataType.n_integer;
                        break;
                    case TaskQueue.TItemValue_Type.num_double:
                        DataType = SqlColumnDataType.n_decimal;
                        break;
                    case TaskQueue.TItemValue_Type.boolean:
                        DataType = SqlColumnDataType.bit;
                        break;
                    case TaskQueue.TItemValue_Type.datetime:
                        DataType = SqlColumnDataType.datetime;
                        break;
                    default:
                        break;
                }
                Name = name;
                UniqueId = unique;
            }

        }

        public string TableName { get; set; }
        public List<SqlColumn> Columns { get; set; }

        public SqlTable(TaskQueue.RepresentedModel model, string tableName)
        {
            TableName = tableName;

            Columns = new List<SqlColumn>();
            Columns.Add(new SqlColumn(TableName + "_ID", TaskQueue.TItemValue_Type.num_int, true));
            foreach (var clm in model.schema.ALL())
            {
                //Columns.Add(new SqlColumn(clm.Value1, clm.Value2));
            }
        }
    }
}
