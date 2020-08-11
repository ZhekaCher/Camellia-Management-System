﻿using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using CamelliaManagementSystem.JsonObjects;
using CamelliaManagementSystem.JsonObjects.RequestObjects;
using CamelliaManagementSystem.JsonObjects.ResponseObjects;

//TODO(REFACTOR)
namespace CamelliaManagementSystem.Requests
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 15:49:33
    /// @version 1.0
    /// <summary>
    /// Parent Request Class
    /// </summary>
    public abstract class CamelliaRequest
    {
        internal readonly CamelliaClient CamelliaClient;

        public CamelliaRequest(CamelliaClient camelliaClient)
        {
            CamelliaClient = camelliaClient;
        }

        protected abstract string RequestLink();
        protected abstract BiinType TypeOfBiin();

        private ReadinessStatus GetReadinessStatus(string requestNumber)
        {
            try
            {
                var res = CamelliaClient.HttpClient
                    .GetStringAsync($"{RequestLink()}/rest/request-states/{requestNumber}")
                    .GetAwaiter()
                    .GetResult();
                var readinessStatus = JsonSerializer.Deserialize<ReadinessStatus>(res);
                return readinessStatus;
            }
            catch (Exception)
            {
                throw new InvalidDataException("It seems that camellia rejected request");
            }
            
        }


        protected ReadinessStatus WaitResult(string requestNumber, int delay = 1000, int timeout = 60000)
        {
            delay = delay < 500 ? 1000 : delay;
            var wait = timeout / delay;
            Thread.Sleep(delay * 2);
            var readinessStatus = GetReadinessStatus(requestNumber);

            while (readinessStatus.status.Equals("IN_PROCESSING"))
            {
                if (wait-- <= 0)
                    throw new InvalidDataException($"Timeout '{timeout}' exceeded");
                // if (wait < timeout / delay /2 && IsDenied(requestNumber))
                // {
                    // throw new InvalidDataException("Request denied");
                // }
                Thread.Sleep(delay);
                readinessStatus = GetReadinessStatus(requestNumber);
            }

            if (readinessStatus.status.Equals("APPROVED"))
                return readinessStatus;

            throw new InvalidDataException($"Readiness status equals {readinessStatus.status}");
        }


        protected bool IsDenied(string requestNumber)
        {
            var response = CamelliaClient.HttpClient.GetStringAsync($"https://egov.kz/services/P30.03/rest/like/{requestNumber}/{CamelliaClient.UserInformation.uin}")
                .GetAwaiter()
                .GetResult();
            return response.Contains("DENIED");
        }
        protected string SendPdfRequest(string signedToken, string solvedCaptcha = null)
        {
            string requestUri = solvedCaptcha == null
                ? $"{RequestLink()}rest/app/send-eds"
                : $"{RequestLink()}rest/app/send-eds?captchaCode={solvedCaptcha}";

            using (var request = new HttpRequestMessage(new HttpMethod("POST"),
                requestUri))
            {
                request.Headers.Add("Connection", "keep-alive");
                request.Headers.Add("Accept", "application/json, text/plain, */*");
                request.Headers.Add("Cache-Control", "max-age=0");
                request.Headers.Add("Origin", "https://egov.kz");
                request.Headers.Add("User-Agent", "Mozilla5.0 Windows NT 10.0");
                request.Headers.Add("Sec-Fetch-Site", "same-origin");
                request.Headers.Add("Sec-Fetch-Mode", "cors");
                request.Headers.Add("Referer", $"{RequestLink()}");
                request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                request.Headers.Add("Cookie", "sign.certType=file;sign.file=null");

                var json = JsonSerializer.Serialize(new XMLToken(signedToken));

                request.Content =
                    new StringContent(json, Encoding.UTF8, "application/json");
                var response = CamelliaClient.HttpClient.SendAsync(request).GetAwaiter().GetResult();
                return response.Content.ReadAsStringAsync().Result.Replace("{\"requestNumber\":\"", "")
                    .Replace("\"}", "");
            }
        }

        protected string GetToken(string biin)
        {
            using var request = new HttpRequestMessage(new HttpMethod("POST"),
                $"{RequestLink()}/rest/app/xml");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Cache-Control", "max-age=0");
            request.Headers.Add("Origin", "https://idp.egov.kz");
            request.Headers.Add("User-Agent", "Mozilla5.0 Windows NT 10.0");
            request.Headers.Add("Sec-Fetch-Site", "same-origin");
            request.Headers.Add("Sec-Fetch-Mode", "navigate");
            request.Headers.Add("Referer", "https://idp.egov.kz/idp/sign-in");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");

            string json;
                if(TypeOfBiin() == BiinType.BIN)
                    json = JsonSerializer.Serialize(new BinDeclarant(biin, CamelliaClient.UserInformation.uin));
                else
                    json = JsonSerializer.Serialize(new IinDeclarant(biin, CamelliaClient.UserInformation.uin));
                

            request.Content =
                new StringContent(json, Encoding.UTF8, "application/json");
            var response = CamelliaClient.HttpClient.SendAsync(request).GetAwaiter().GetResult();
            return response.Content.ReadAsStringAsync().Result;
        }
        
        protected string GetToken(string biin, string stringDate)
        {
            using var request = new HttpRequestMessage(new HttpMethod("POST"),
                $"{RequestLink()}/rest/app/xml");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Cache-Control", "max-age=0");
            request.Headers.Add("Origin", "https://idp.egov.kz");
            request.Headers.Add("User-Agent", "Mozilla5.0 Windows NT 10.0");
            request.Headers.Add("Sec-Fetch-Site", "same-origin");
            request.Headers.Add("Sec-Fetch-Mode", "navigate");
            request.Headers.Add("Referer", "https://idp.egov.kz/idp/sign-in");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");

            string json;
            if(TypeOfBiin() == BiinType.BIN)
                json = JsonSerializer.Serialize(new BinDateDeclarant(biin, stringDate, CamelliaClient.UserInformation.uin));
            else
                json = JsonSerializer.Serialize(new BinDateDeclarant(biin, stringDate, CamelliaClient.UserInformation.uin));
                

            request.Content =
                new StringContent(json, Encoding.UTF8, "application/json");
            var response = CamelliaClient.HttpClient.SendAsync(request).GetAwaiter().GetResult();
            return response.Content.ReadAsStringAsync().Result;
        }
    }

    public enum BiinType
    {
        BIN,
        IIN
    }
}