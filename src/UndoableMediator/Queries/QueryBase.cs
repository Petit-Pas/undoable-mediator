﻿using UndoableMediator.Mediators;

namespace UndoableMediator.Queries;

public abstract class QueryBase<T> : IQuery
{
    public QueryResponse<T>? ExecuteBy(IUndoableMediator mediator)
    {
        return mediator.Execute<T>(this) as QueryResponse<T>;
    }
}
