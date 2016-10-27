namespace SimpleORM.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Field)]
    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; set; }
    }
}
