﻿export interface IMethodInfo<TBodyParam, TQueryStringParams, TReturn> {
    url: string;
}

export function BuildQueryString<TQueryStringParams>(queryParams: TQueryStringParams): string {
    var result = _(queryParams).keys().map(key => "" + key + "=" + queryParams[key]).reduce((s: string, acc: string) => s + "&" + acc);
    return result;
}

export function CallMethodNoBodyParam<TReturn, TQueryStringParams>(methodInfo: IMethodInfo<void, TQueryStringParams, TReturn>, queryParams: TQueryStringParams): PromiseLike<TReturn> {
    var urlWithQuery = methodInfo.url + "/?" + BuildQueryString(queryParams);
    var result = $.ajax({
        url: urlWithQuery
    });;
    return result
}

export function CallMethodWithBodyParam<TBodyParam, TQueryStringParams, TReturn>
    (methodInfo: IMethodInfo<TBodyParam, TQueryStringParams, TReturn>, parameter: TBodyParam, queryParams: TQueryStringParams): PromiseLike<TReturn> {
    var urlWithQuery = methodInfo.url + "/?" + BuildQueryString(queryParams);
    var result = $.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        url: urlWithQuery,
        data: JSON.stringify(parameter)
    });
    return result;
}