﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace LoggingPerformance.Octopus.Persistance
{
    public abstract class DocumentMap<TDocument> : DocumentMap
    {
        protected DocumentMap()
        {
            InitializeDefault(typeof(TDocument));
        }

        protected ColumnMapping Column<T>(Expression<Func<TDocument, T>> property)
        {
            var column = new ColumnMapping(GetPropertyInfo(property));
            IndexedColumns.Add(column);
            return column;
        }

        protected ColumnMapping Column<T>(Expression<Func<TDocument, T>> property, Action<ColumnMapping> configure)
        {
            var column = Column(property);
            configure(column);
            return column;
        }

        protected ColumnMapping VirtualColumn<TProperty>(string name, DbType databaseType, Func<TDocument, TProperty> reader, Action<TDocument, TProperty> writer = null, int? maxLength = null, bool nullable = false)
        {
            var column = new ColumnMapping(name, databaseType, new DelegateReaderWriter<TDocument, TProperty>(reader, writer));
            IndexedColumns.Add(column);
            if (maxLength != null)
            {
                column.MaxLength = maxLength.Value;
            }
            column.IsNullable = nullable;
            return column;
        }

        static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda));

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda));

            return propInfo;
        }

        protected UniqueRule Unique(string constraintName, string columnName, string errorMessage)
        {
            var unique = new UniqueRule(constraintName, columnName) { Message = errorMessage };
            UniqueConstraints.Add(unique);
            return unique;
        }

        protected UniqueRule Unique(string constraintName, string[] columnNames, string errorMessage)
        {
            var unique = new UniqueRule(constraintName, columnNames) { Message = errorMessage };
            UniqueConstraints.Add(unique);
            return unique;
        }
    }

    public abstract class DocumentMap
    {
        protected DocumentMap()
        {
            IndexedColumns = new List<ColumnMapping>();
            UniqueConstraints = new List<UniqueRule>();
        }

        public string TableName { get; protected set; }
        public string IdPrefix { get; protected set; }
        public bool IsProjection { get; protected set; }
        public Type Type { get; protected set; }
        public ColumnMapping IdColumn { get; private set; }
        public List<ColumnMapping> IndexedColumns { get; private set; }
        public List<UniqueRule> UniqueConstraints { get; private set; }

        protected void InitializeDefault(Type type)
        {
            Type = type;
            TableName = type.Name;
            IdPrefix = TableName + "s";

            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                if (property.Name == "Id")
                {
                    IdColumn = new ColumnMapping(property);
                }
            }
        }
    }
}