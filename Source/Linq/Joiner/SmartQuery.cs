using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bars2Db.Linq.Joiner.Interfaces;

namespace Bars2Db.Linq.Joiner
{
    public class SmartQuery<TEntity> : ISmartQuery<TEntity>

    {
        /// <summary>
        ///     Gets the type of the element(s) that are returned when the expression tree associated with this instance of
        ///     <see cref="T:System.Linq.IQueryable" /> is executed.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Type" /> that represents the type of the element(s) that are returned when the
        ///     expression tree associated with this object is executed.
        /// </returns>
        public Type ElementType
        {
            get { return typeof(TEntity); }
        }

        /// <summary>Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable" />.</summary>
        /// <returns>The <see cref="T:System.Linq.Expressions.Expression" /> that is associated with this instance of
        ///     <see cref="T:System.Linq.IQueryable" />.</returns>
        public Expression Expression { get; set; }

        /// <summary>Gets the query provider that is associated with this data source.</summary>
        /// <returns>The <see cref="T:System.Linq.IQueryProvider" /> that is associated with this data source.</returns>
        public IQueryProvider Provider { get; private set; }

        public SmartQuery(IQueryProvider queryProvider)
        {
            Provider = queryProvider;
            Expression = Expression.Constant(this);
        }

        public SmartQuery(IQueryProvider provider, Expression expression)
        {
            Provider = provider;
            Expression = expression;
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<TEntity> GetEnumerator()
        {
            var smartJoiner = ((ISmartJoiner)Provider);

            return smartJoiner.Execute(this).GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}