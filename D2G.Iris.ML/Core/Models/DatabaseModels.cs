namespace D2G.Iris.ML.Core.Models
{
    public class TableInfo
    {
        public string SchemaName { get; set; } = "";
        public string TableName { get; set; } = "";
        public string TableType { get; set; } = "";
        public string FullName { get; set; } = "";
        public long? RowCount { get; set; }
    }

    public class ColumnInfo
    {
        public string ColumnName { get; set; } = "";
        public string DataType { get; set; } = "";
        public bool IsNullable { get; set; }
        public int? MaxLength { get; set; }
        public byte? Precision { get; set; }
        public int? Scale { get; set; }
        public int OrdinalPosition { get; set; }

        public string DisplayType
        {
            get
            {
                var type = DataType.ToUpper();

                if (MaxLength.HasValue && MaxLength > 0)
                    return $"{type}({MaxLength})";

                if (Precision.HasValue && Scale.HasValue)
                    return $"{type}({Precision},{Scale})";

                if (Precision.HasValue)
                    return $"{type}({Precision})";

                return type;
            }
        }
    }
}