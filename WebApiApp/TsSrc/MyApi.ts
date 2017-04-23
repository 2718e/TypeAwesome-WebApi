import * as _ from "lodash";
import * as $ from "jquery";

module ApiExport {

export interface IMethodInfo<TBodyParam, TQueryStringParams, TReturn> {
    url: string;
	httpMethod: string;
}

export function BuildQueryString<TQueryStringParams>(queryParams: TQueryStringParams): string {
    let result = "";
    if (_.keys(queryParams).length > 0) {
        result = "/?" + _(queryParams).keys().map(key => "" + key + "=" + queryParams[key]).reduce((s: string, acc: string) => s + "&" + acc);
    }
    return result;
}

export function CallMethodNoBodyParam<TReturn, TQueryStringParams>(methodInfo: IMethodInfo<void, TQueryStringParams, TReturn>, queryParams: TQueryStringParams): PromiseLike<TReturn> {
    let urlWithQuery = methodInfo.url + BuildQueryString(queryParams);
    let result = $.ajax({
		type: methodInfo.httpMethod,
        url: urlWithQuery
    });
    return result;
}

export function CallMethodWithBodyParam<TBodyParam, TQueryStringParams, TReturn>
    (methodInfo: IMethodInfo<TBodyParam, TQueryStringParams, TReturn>, parameter: TBodyParam, queryParams: TQueryStringParams): PromiseLike<TReturn> {
    let urlWithQuery = methodInfo.url + BuildQueryString(queryParams);
    let result = $.ajax({
        type: methodInfo.httpMethod,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        url: urlWithQuery,
        data: JSON.stringify(parameter)
    });
    return result;
}

export interface ISamplemodel2 {
  Things?: ISampleModel[];
  CustomerName?: string;
  CostCents?: number;
}

export interface ISampleModel {
  Name?: string;
  Id?: number;
  Amount?: number;
}

export enum EPet {
    Cat = 0,
    Dog = 1,
    Bird = 2,
    Horse = 3
}

export interface ISimpleTypesModel {
  Timestamp?: string;
  Id?: string;
  Duration?: string;
}

export interface SampleDeleteEverythingQueryParams {
  really: boolean;
}

export interface SampleChangeDataQueryParams {
  key: string;
  value: number;
}

export interface SampleDuplicateModelQueryParams {
  nTimes: number;
}

export interface SampleMultiply3NumbersQueryParams {
  n1: number;
  n2: number;
  n3: number;
}

export const SampleSampleGetMethodInfo: IMethodInfo<void, {}, ISamplemodel2> = {
    url: "/api/Sample/SampleGet",
    httpMethod: "GET"
};

export const SampleDeleteEverythingMethodInfo: IMethodInfo<void, SampleDeleteEverythingQueryParams, any> = {
    url: "/api/Sample/DeleteEverything",
    httpMethod: "DELETE"
};

export const SampleChangeDataMethodInfo: IMethodInfo<void, SampleChangeDataQueryParams, number> = {
    url: "/api/Sample/ChangeData",
    httpMethod: "PUT"
};

export const SampleDuplicateModelMethodInfo: IMethodInfo<ISampleModel, SampleDuplicateModelQueryParams, ISamplemodel2> = {
    url: "/api/Sample/DuplicateModel",
    httpMethod: "POST"
};

export const SampleMultiply3NumbersMethodInfo: IMethodInfo<void, SampleMultiply3NumbersQueryParams, number> = {
    url: "/api/Sample/Multiply3Numbers",
    httpMethod: "GET"
};

export const SampleGetPetMethodInfo: IMethodInfo<void, {}, EPet> = {
    url: "/api/Sample/GetPet",
    httpMethod: "GET"
};

export const SampleApiDescriptionMethodInfo: IMethodInfo<void, {}, string> = {
    url: "/api/Sample/ApiDescription",
    httpMethod: "GET"
};

export const SampleGetIdentifiedTimeMethodInfo: IMethodInfo<void, {}, ISimpleTypesModel> = {
    url: "/api/Sample/GetIdentifiedTime",
    httpMethod: "GET"
};

export function SampleSampleGet(): PromiseLike<ISamplemodel2> {
    let queryParams = {};
    return CallMethodNoBodyParam(SampleSampleGetMethodInfo, queryParams);
}

export function SampleDeleteEverything(inreally: boolean): PromiseLike<any> {
    let queryParams = {really : inreally};
    return CallMethodNoBodyParam(SampleDeleteEverythingMethodInfo, queryParams);
}

export function SampleChangeData(inkey: string, invalue: number): PromiseLike<number> {
    let queryParams = {key : inkey, value : invalue};
    return CallMethodNoBodyParam(SampleChangeDataMethodInfo, queryParams);
}

export function SampleMultiply3Numbers(inn1: number, inn2: number, inn3: number): PromiseLike<number> {
    let queryParams = {n1 : inn1, n2 : inn2, n3 : inn3};
    return CallMethodNoBodyParam(SampleMultiply3NumbersMethodInfo, queryParams);
}

export function SampleGetPet(): PromiseLike<EPet> {
    let queryParams = {};
    return CallMethodNoBodyParam(SampleGetPetMethodInfo, queryParams);
}

export function SampleApiDescription(): PromiseLike<string> {
    let queryParams = {};
    return CallMethodNoBodyParam(SampleApiDescriptionMethodInfo, queryParams);
}

export function SampleGetIdentifiedTime(): PromiseLike<ISimpleTypesModel> {
    let queryParams = {};
    return CallMethodNoBodyParam(SampleGetIdentifiedTimeMethodInfo, queryParams);
}

export function SampleDuplicateModel(innTimes: number, toClone: ISampleModel): PromiseLike<ISamplemodel2> {
    let queryParams = {nTimes : innTimes};
    return CallMethodWithBodyParam(SampleDuplicateModelMethodInfo, toClone, queryParams);
}

}
export = ApiExport;