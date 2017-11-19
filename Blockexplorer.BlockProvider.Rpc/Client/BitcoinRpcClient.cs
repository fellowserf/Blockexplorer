﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;

namespace Blockexplorer.BlockProvider.Rpc.Client
{
	public class UserOptions<T> : IOptions<T> where T : class, new()
	{
		public UserOptions()
		{
		}

		public T Value { get; set; }
	}
	public class RpcSettings
	{
		public string User { get; set; }
		public string Password { get; set; }
		public string Url { get; set; }
	}

	public class BitcoinRpcClient
	{
		readonly RpcSettings _settings;

		public BitcoinRpcClient(RpcSettings settings)
		{
			_settings = settings;
		}

		private async Task<string> InvokeMethod(string a_sMethod, params object[] a_params)
		{
			try
			{
				HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(_settings.Url);
				//HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Url);
				webRequest.Credentials = new NetworkCredential(_settings.User, _settings.Password);
				string username = _settings.User;
				string password = _settings.Password;
				//Console.WriteLine($"Url: {_settings.Url}, User: {username}, Pass: {password}");
				string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));

				webRequest.Headers["Authorization"] = "Basic " + svcCredentials;

				webRequest.ContentType = "application/json-rpc";
				webRequest.Method = "POST";

				JObject joe = new JObject();
				joe["jsonrpc"] = "1.0";
				joe["id"] = "1";
				joe["method"] = a_sMethod;

				if (a_params != null)
				{
					if (a_params.Length > 0)
					{
						JArray props = new JArray();
						foreach (var p in a_params)
						{
							props.Add(p);
						}
						joe.Add(new JProperty("params", props));
					}
				}

				string s = JsonConvert.SerializeObject(joe);
				byte[] byteArray = Encoding.UTF8.GetBytes(s);

				using (Stream dataStream = await webRequest.GetRequestStreamAsync())
				{
					dataStream.Write(byteArray, 0, byteArray.Length);
				}

				using (WebResponse webResponse = await webRequest.GetResponseAsync())
				{
					using (Stream str = webResponse.GetResponseStream())
					{
						using (StreamReader sr = new StreamReader(str))
						{
							return await sr.ReadToEndAsync();
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				throw;
			}
			
		}

		public async Task<string> GetBestBlockHash()
		{
			var json = await InvokeMethod("getbestblockhash");
			var result = JsonConvert.DeserializeObject<TransportRpcModel<string>>(json);
			return result.Result;
		}

		public async Task<GetRawTransactionRpcModel> GetRawTransactionAsync(string txid, int jsonResult = 1)
		{
			try
			{
				var json = await InvokeMethod("getrawtransaction", txid, jsonResult);
				var result = JsonConvert.DeserializeObject<TransportRpcModel<GetRawTransactionRpcModel>>(json);
				if (result?.Result == null)
					return null;
				result.Result.OriginalJson = json;
				return result.Result;
			}
			catch { }
			return null;
			
		}

		public async Task<GetBlockRpcModel> GetBlockAsync(string hash)
		{
			var json = await InvokeMethod("getblock", hash);
			var result = JsonConvert.DeserializeObject<TransportRpcModel<GetBlockRpcModel>>(json);
			if (result?.Result == null)
				return null;
			result.Result.OriginalJson = json;
			return result.Result;
		}

		public async Task<string> GetBlockHashAsync(int blockNumber)
		{
			var json = await InvokeMethod("getblockhash", blockNumber);
			var result = JsonConvert.DeserializeObject<TransportRpcModel<string>>(json);
			return result.Result;
		}

		public async Task<int> GetBlockCountAsync()
		{
			var json = await InvokeMethod("getblockcount");
			var result = JsonConvert.DeserializeObject<TransportRpcModel<int>>(json);
			return result.Result;
		}

		public async Task<RpcTransaction> GetTransactionByTxIdAsync(string txId)
		{
			var tx = await GetRawTransactionAsync(txId);

			if (tx == null)
			{
				return null;
			}

			var block = await GetBlockAsync(tx.Blockhash);

			return RpcTransaction.Create(tx, block.Height);
		}

		public async Task<GetInfoRpcModel> GetInfo()
		{
			var json = await InvokeMethod("getinfo");
			var result = JsonConvert.DeserializeObject<TransportRpcModel<GetInfoRpcModel>>(json);
			return result.Result;
		}

		public async Task<GetStakingInfoRpcModel> GetStakingInfo()
		{
			var json = await InvokeMethod("getstakinginfo");
			var result = JsonConvert.DeserializeObject<TransportRpcModel<GetStakingInfoRpcModel>>(json);
			return result.Result;
		}
	}

	public class RpcTransaction
	{
		public string Txid { get; set; }
		public string Blockhash { get; set; }
		public long Confirmations { get; set; }
		public DateTime Time { get; set; }
		public long Height { get; set; }

		public static DateTime GetTime(uint time)
		{
			return time.FromUnixDateTime();
		}

		public static RpcTransaction Create(GetRawTransactionRpcModel txData, long height)
		{
			return new RpcTransaction
			{
				Blockhash = txData.Blockhash,
				Txid = txData.Txid,
				Confirmations = txData.Confirmations,
				Time = GetTime(txData.Time),
				Height = height
			};
		}
	}
}
