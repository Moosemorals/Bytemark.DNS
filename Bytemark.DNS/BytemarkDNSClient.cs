using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Bytemark.DNS.Models;

using Newtonsoft.Json;

namespace Bytemark.DNS {
    public class BytemarkDNSClient {
        private readonly string BaseURL = @"https://dns.bytemark.co.uk/api/v1/";
        private readonly HttpClient _http;
        private readonly AuthParameters _authParams;
        private AuthToken? _authToken;

        public BytemarkDNSClient(HttpClient httpClient, string username, string password) {
            _http = httpClient;
            _authParams = new AuthParameters(username, password);
        }

        private async Task<Result<string>> CreateAuthSession() {
            string json = JsonConvert.SerializeObject(_authParams);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://auth.bytemark.co.uk/session") {
                Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                    )
            };
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

            HttpResponseMessage response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode) {
                _authToken = new AuthToken(await response.Content.ReadAsStringAsync(), DateTimeOffset.UtcNow);
                return new Result<string>((int)response.StatusCode, _authToken.Token, null);
            } else {
                return new Result<string>((int)response.StatusCode, null, "Authentication Failed");
            }
        }

        private async Task<Result<string>> RefreshAuthSession() {
            if (_authToken == null) {
                throw new Exception("_authToken hasn't been set");
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://auth.bytemark.co.uk/session/" + _authToken.Token);

            HttpResponseMessage response = await _http.SendAsync(request);

            if (response.IsSuccessStatusCode) {
                _authToken.LastUpdated = DateTimeOffset.UtcNow;
                return new Result<string>((int)response.StatusCode, _authToken.Token, null);
            } else {
                return new Result<string>((int)response.StatusCode, null, "Authentication session not found");
            }
        }

        private async Task<Result<string>> GetAuthToken() => _authToken != null && _authToken.IsFresh
            ? await RefreshAuthSession()
            : await CreateAuthSession();

        private async Task<Result<T>> SendRequest<T>(HttpRequestMessage request) where T : class {
            try {

                request.Headers.Authorization = await GetAuthenticationHeader();

                HttpResponseMessage response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) {
                    string json = await response.Content.ReadAsStringAsync();
                    return new Result<T>((int)response.StatusCode, JsonConvert.DeserializeObject<T>(json), null);
                } else {
                    string body = await response.Content.ReadAsStringAsync();
                    try {
                        ErrorResponse e = JsonConvert.DeserializeObject<ErrorResponse>(body);
                        return new Result<T>((int)response.StatusCode, null, e.Error ?? "Can't parse error response from remote server");
                    } catch {
                        return new Result<T>((int)response.StatusCode, null, "Can't parse json from remote server: " + body);
                    }
                }
            } catch (Exception ex) {
                return new Result<T>(500, null, ex.Message); 
            } 
        }

        private async Task<Result<string>> SendRequest(HttpRequestMessage request) {
            try {
                request.Headers.Authorization = await GetAuthenticationHeader();
                HttpResponseMessage response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) {
                    return new Result<string>((int)response.StatusCode, "Success", null);
                } else {
                    ErrorResponse e = JsonConvert.DeserializeObject<ErrorResponse>(await response.Content.ReadAsStringAsync());
                    return new Result<string>((int)response.StatusCode, null, e.Error ?? "Can't parse error respones from remote server");
                }
            } catch (Exception ex) {
                return new Result<string> (500, null, ex.Message);
            } 
        }

        private async Task<AuthenticationHeaderValue> GetAuthenticationHeader() {
            Result<string> token = await GetAuthToken();

            if (token.IsSuccess) {
                return new AuthenticationHeaderValue("Bearer", token.Payload);
            } else {
                throw new BytemarkDNSException(token.Error ?? "Unkown error");
            }
        }

        private string BuildQueryString(params string[] param) => BuildQueryString(new List<string>(param));

        private string BuildQueryString(IList<string> param) {
            if (param == null || param.Count == 0) {
                return "";
            }
            if (param.Count % 2 != 0) {
                throw new ArgumentException("Parameters must come in pairs");
            }

            string result = "";

            for (int i = 0; i < param.Count; i += 2) {

                if (i > 0) {
                    result += "&";
                }

                result += HttpUtility.UrlEncode(param[i]);

                string value = param[i + 1];
                if (!string.IsNullOrEmpty(value)) {
                    result += "=";
                    result += HttpUtility.UrlEncode(value);
                }
            }

            return result;
        }


        /// <summary>
        /// List all known domains. Note, overview parameter doesn't seem to work.
        /// </summary>
        /// <param name="AccountID">Only show domains for this account (optional)</param>
        /// <param name="Overview">Also fetch all records associated with each domain (optional, default = false)</param>
        /// <returns>List of Domains</returns>
        public async Task<Result<IEnumerable<Domain>>> ListDomainsAsync(int? AccountID = null, bool Overview = false) {
            List<string> param = new List<string>();
            if (AccountID != null) {
                param.Add("account_id");
                param.Add(AccountID.Value.ToString());
            }
            if (Overview) {
                param.Add("view");
                param.Add("overview");
            }

            string target = BaseURL + "domains";
            if (param.Count > 0) {
                target += "?" + BuildQueryString(param);
            }



            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, target);

            return await SendRequest<IEnumerable<Domain>>(request);
        }

        /// <summary>
        /// Create a domain
        /// </summary>
        /// <param name="AccountID"></param>
        /// <param name="Name">This must not end with a dot (.).</param>
        /// <returns>Details of newly created Domain</returns>
        public async Task<Result<Domain>> CreateDomainAsync(int AccountID, string Name) {
            string json = JsonConvert.SerializeObject(new Domain(null, Name, AccountID, null));

            HttpRequestMessage request = new(HttpMethod.Post, BaseURL + "domains") {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };

            return await SendRequest<Domain>(request);
        }

        /// <summary>
        /// Get details of a domain
        /// </summary>
        /// <param name="DomainID">ID of domain</param>
        /// <param name="overview">Fetch all reacords associated with the domain. (optional default = false)</param>
        /// <returns></returns>
        public async Task<Result<Domain>> FetchDomainAsync(int DomainID, bool overview = false) {
            string target = string.Format("{0}{1}/{2}", BaseURL, "domains", DomainID);
            if (overview) {
                target += "?overview=true";
            }

            return await SendRequest<Domain>(new HttpRequestMessage(HttpMethod.Get, target));
        }

        /// <summary>
        /// Update a domain
        /// </summary>
        /// <param name="DomainID">ID of domain/param>
        /// <param name="AccountID"></param>
        /// <param name="Name">This must not end in a dot (.).</param>
        /// <returns></returns>
        public Result<Domain> UpdateDomain(int DomainID, int AccountID, string Name) => throw new NotImplementedException("This is not implemented yet, as there are no changable attributes on a domain.");

        /// <summary>
        /// Delete a domain
        /// </summary>
        /// <param name="DomainID">ID of domain</param>
        public async Task<Result<string>> DeleteDomainAsync(int DomainID) {
            string target = string.Format("{0}{1}/{2}", BaseURL, "domains", DomainID);

            return await SendRequest(new HttpRequestMessage(HttpMethod.Delete, target));
        }

        /// <summary>
        /// Populate the domain's records to a state where they work, i.e.
        /// adding SOA and NS records appropriately.

        /// This will return 409 if any SOA or NS records already exist for the
        /// Domain.

        /// </summary>
        /// <param name="DomainID">ID of domain</param>
        public async Task<Result<string>> AutoconfigureDomainAsync(int DomainID) {
            string target = string.Format("{0}{1}/{2}/autoconf", BaseURL, "domains", DomainID);

            return await SendRequest(new HttpRequestMessage(HttpMethod.Post, target));
        }

        /// <summary>
        /// List domain records
        /// </summary>
        /// <param name="DomainID"></param>
        /// <param name="AccountID"></param>
        /// <returns></returns>
        public async Task<Result<IEnumerable<Record>>> ListRecordsAsync(int DomainID, int AccountID) {
            string target = string.Format("{0}{1}", BaseURL, "records");

            target += "?" + BuildQueryString(
                "domain_id", DomainID.ToString(),
                "account_id", AccountID.ToString()
                );

            return await SendRequest<IEnumerable<Record>>(new HttpRequestMessage(HttpMethod.Get, target));
        }

        /// <summary>
        /// Create a domain record
        /// </summary>
        /// <param name="Record">A Record with all the details filled in</param>
        /// <returns></returns>
        public async Task<Result<Record>> CreateRecordAsync(Record Record) {
            string target = string.Format("{0}{1}", BaseURL, "records");

            string json = JsonConvert.SerializeObject(Record);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, target) {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };

            return await SendRequest<Record>(request);
        }

        /// <summary>
        /// View a domain record
        /// </summary>
        /// <param name="RecordID"></param>
        /// <returns></returns>
        public async Task<Result<Record>> ViewRecordAsync(int RecordID) {
            string target = string.Format("{0}{1}/{2}", BaseURL, "records", RecordID);

            return await SendRequest<Record>(new HttpRequestMessage(HttpMethod.Get, target));
        }

        /// <summary>
        /// Update a record
        /// </summary>
        /// <param name="RecordID"></param>
        /// <param name="Record">A record with the changes you want to show</param>
        /// <returns></returns>
        public async Task<Result<Record>> UpdateRecordAsync(int RecordID, Record Record) {
            string target = string.Format("{0}{1}/{2}", BaseURL, "records", RecordID);

            string json = JsonConvert.SerializeObject(Record);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, target) {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };

            return await SendRequest<Record>(request);

        }

        /// <summary>
        /// Delete a domain record
        /// </summary>
        /// <param name="RecordID"></param>
        public async Task<Result<string>> DeleteRecordAsync(int RecordID) {
            string target = string.Format("{0}{1}/{2}", BaseURL, "records", RecordID);

            return await SendRequest(new HttpRequestMessage(HttpMethod.Delete, target));
        }
    }
}
