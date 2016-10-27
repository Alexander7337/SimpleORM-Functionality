namespace SimpleORM.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class EntityAttribute : Attribute
    {
        public string TableName { get; set; }
    }
}
