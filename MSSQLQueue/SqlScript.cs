using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSSQLQueue
{
    public class SqlScript
    {
        const string TemplateCreateTable = @"
CREATE TABLE dbo.Q_{0}
(
    {1}
) ON [PRIMARY] 
GO
";
        const string TemplateCreateColumn = @" {0} {1} {2} NULL{3}";
        public static string ForTableGen(SqlTable table)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                string type = "";
                switch (table.Columns[i].DataType)
                {
                    case SqlTable.SqlColumnDataType.varchar:
                        type = "varchar(MAX)";
                        break;
                    case SqlTable.SqlColumnDataType.n_integer:
                        type = "integer";
                        break;
                    case SqlTable.SqlColumnDataType.n_decimal:
                        type = "decimal";
                        break;
                    case SqlTable.SqlColumnDataType.bit:
                        type = "bit";
                        break;
                    case SqlTable.SqlColumnDataType.datetime:
                        type = "datetime";
                        break;
                    default:
                        break;
                }
                sb.AppendFormat(TemplateCreateColumn, table.Columns[i].Name, type, table.Columns[i].UniqueId ? "PRIMARY KEY IDENTITY(1,1) NOT" : "",
                    i + 1 == table.Columns.Count ? "" : ", ");
            }
            return string.Format(TemplateCreateTable, table.TableName, sb.ToString());
        }
    }
}
