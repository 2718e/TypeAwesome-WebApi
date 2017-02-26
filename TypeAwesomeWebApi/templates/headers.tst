export interface IMethodInfo<TBodyParam, TQueryStringParams, TReturn> {
    url: string;
}

export function BuildQueryString<TQueryStringParams>(queryParams: TQueryStringParams): string {
    let result = "";
    if (_.keys(queryParams).length > 0) {
        let result = "/?" + _(queryParams).keys().map(key => "" + key + "=" + queryParams[key]).reduce((s: string, acc: string) => s + "&" + acc);
    }
    return result;
}

export function CallMethodNoBodyParam<TReturn, TQueryStringParams>(methodInfo: IMethodInfo<void, TQueryStringParams, TReturn>, queryParams: TQueryStringParams): PromiseLike<TReturn> {
    let urlWithQuery = methodInfo.url + BuildQueryString(queryParams);
    let result = $.ajax({
        url: urlWithQuery
    });
    return result;
}

export function CallMethodWithBodyParam<TBodyParam, TQueryStringParams, TReturn>
    (methodInfo: IMethodInfo<TBodyParam, TQueryStringParams, TReturn>, parameter: TBodyParam, queryParams: TQueryStringParams): PromiseLike<TReturn> {
    let urlWithQuery = methodInfo.url + BuildQueryString(queryParams);
    let result = $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        url: urlWithQuery,
        data: JSON.stringify(parameter)
    });
    return result;
}

