using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaborg.Dataverse.DataProviders.Transformations.Filters
{
    public interface IEntityFilter<T> where T : Entity
    {
        /// <summary>
        /// Returns a list of <typeparamref name="T" /> elements filtered by the <paramref name="queryExpression"/>.
        /// </summary>
        /// <param name="entities">A list of early bound entities the filtering will be applied to.</param>
        /// <param name="queryExpression">QueryExpression instance provided by execution context.</param>
        /// <returns>Items of type <typeparamref name="T"/> filtered by <paramref name="queryExpression"/>.</returns>
        IList<T> FilterBy(IList<T> entities, QueryExpression queryExpression);
    }
}
