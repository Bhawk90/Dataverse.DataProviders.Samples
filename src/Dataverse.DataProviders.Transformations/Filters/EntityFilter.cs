using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Data.Expressions;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using System.Linq;

namespace Gaborg.Dataverse.DataProviders.Transformations.Filters
{
    public class EntityFilter<T> : IEntityFilter<T> where T : Entity
    {
        /// <summary>
        /// Returns a list of <typeparamref name="T" /> elements filtered by the <paramref name="queryExpression"/>.
        /// </summary>
        /// <param name="entities">A list of early bound entities the filtering will be applied to.</param>
        /// <param name="queryExpression">QueryExpression instance provided by execution context.</param>
        /// <returns>Items of type <typeparamref name="T"/> filtered by <paramref name="queryExpression"/>.</returns>
        public IList<T> FilterBy(IList<T> entities, QueryExpression queryExpression)
        {
            // FilterExpressionTransform throws an exception if no criteria has been defined
            if (queryExpression.Criteria.Conditions.Count > 0 || queryExpression.Criteria.Filters.Count > 0)
            {
                // FilterExpressionTransform transform the filters from the specified queryExpression
                // to LINQ expressions that can be evaluated on collections that support querying.
                var transformedExpression = new FilterExpressionTransform<T>().Transform(queryExpression.Criteria);
                var compiledQuery = transformedExpression.Compile();

                return entities.Where(compiledQuery).ToList();
            }

            return entities;
        }
    }
}
