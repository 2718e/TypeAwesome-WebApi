import * as _ from 'lodash';
import * as $ from 'jquery';

module MyApi {

export interface IMethodInfo<TBodyParam, TQueryStringParams, TReturn> {
    url: string;
}

export function BuildQueryString<TQueryStringParams>(queryParams: TQueryStringParams): string {
    var result = "";
    if (_.keys(queryParams).length > 0){
    var result = "/?" + _(queryParams).keys().map(key => "" + key + "=" + queryParams[key]).reduce((s: string, acc: string) => s + "&" + acc);
    }
    return result;
}

export function CallMethodNoBodyParam<TReturn, TQueryStringParams>(methodInfo: IMethodInfo<void, TQueryStringParams, TReturn>, queryParams: TQueryStringParams): PromiseLike<TReturn> {
    var urlWithQuery = methodInfo.url + BuildQueryString(queryParams);
    var result = $.ajax({
        url: urlWithQuery
    });;
    return result
}

export function CallMethodWithBodyParam<TBodyParam, TQueryStringParams, TReturn>
    (methodInfo: IMethodInfo<TBodyParam, TQueryStringParams, TReturn>, parameter: TBodyParam, queryParams: TQueryStringParams): PromiseLike<TReturn> {
    var urlWithQuery = methodInfo.url + BuildQueryString(queryParams);
    var result = $.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        url: urlWithQuery,
        data: JSON.stringify(parameter)
    });
    return result;
}

export interface ISamplemodel2 {
  Things : ISampleModel[];
  CustomerName : string;
  CostCents : number;
}

export interface ISampleModel {
  Name : string;
  Id : number;
  Amount : number;
}

export enum EPet {
    Cat = 0,
    Dog = 1,
    Bird = 2,
    Horse = 3
}

export interface SampleDuplicateModelQueryParams {
  nTimes : number;
}

export interface SampleMultiply3NumbersQueryParams {
  n1 : number;
  n2 : number;
  n3 : number;
}

export var SampleSampleGetMethodInfo : IMethodInfo<void,{},ISamplemodel2> = {
    url : "/api/Sample/SampleGet"
}

export var SampleDuplicateModelMethodInfo : IMethodInfo<ISampleModel,SampleDuplicateModelQueryParams,ISamplemodel2> = {
    url : "/api/Sample/DuplicateModel"
}

export var SampleMultiply3NumbersMethodInfo : IMethodInfo<void,SampleMultiply3NumbersQueryParams,number> = {
    url : "/api/Sample/Multiply3Numbers"
}

export var SampleGetPetMethodInfo : IMethodInfo<void,{},EPet> = {
    url : "/api/Sample/GetPet"
}

export var SampleApiDescriptionMethodInfo : IMethodInfo<void,{},string> = {
    url : "/api/Sample/ApiDescription"
}

export function SampleSampleGet() : PromiseLike<ISamplemodel2>{
var queryParams = {};return CallMethodNoBodyParam(SampleSampleGetMethodInfo, queryParams) 
}

export function SampleMultiply3Numbers(inn1 : number, inn2 : number, inn3 : number) : PromiseLike<number>{
var queryParams = {n1 : inn1, n2 : inn2, n3 : inn3};return CallMethodNoBodyParam(SampleMultiply3NumbersMethodInfo, queryParams) 
}

export function SampleGetPet() : PromiseLike<EPet>{
var queryParams = {};return CallMethodNoBodyParam(SampleGetPetMethodInfo, queryParams) 
}

export function SampleApiDescription() : PromiseLike<string>{
var queryParams = {};return CallMethodNoBodyParam(SampleApiDescriptionMethodInfo, queryParams) 
}

export function SampleDuplicateModel(innTimes : number, toClone : ISampleModel) : PromiseLike<ISamplemodel2>{
var queryParams = {nTimes : innTimes};return CallMethodWithBodyParam(SampleDuplicateModelMethodInfo, toClone, queryParams) 
}

}
export = MyApi;