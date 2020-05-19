using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CTPMarketApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuoteServer.Model;

namespace QuoteServer {
    public class Program {
        /// <summary>
        /// 行情接口实例
        /// </summary>
        private static MarketApi _api;
        private static CtpConnection cnn;

        public static void Main(string[] args) {
            CreateHostBuilder(args).Build().Run();

            //++++++++++++++++++++++++++++++++++++++++++++++++
            cnn = new CtpConnection();
            Initialize(cnn);

            SubscribeMarketData();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });

        public static void Initialize(CtpConnection cnn) {
            _api = new MarketApi(cnn.BrokerID, cnn.FrontMkAddr);
            _api.OnRspError += new MarketApi.RspError((ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast) => {
                Console.WriteLine("ErrorID: {0}, ErrorMsg: {1}", pRspInfo.ErrorID, pRspInfo.ErrorMsg);
            });
            _api.OnFrontConnected += new MarketApi.FrontConnected(() => {
                cnn.IsConnected = true;
                _api.UserLogin(-3, cnn.Investor, cnn.Password);
            });
            _api.OnRspUserLogin += new MarketApi.RspUserLogin((ref CThostFtdcRspUserLoginField pRspUserLogin,
                ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast) => {
                    cnn.IsLogin = true;
                });
            _api.OnRspUserLogout += new MarketApi.RspUserLogout((ref CThostFtdcUserLogoutField pRspUserLogout,
                ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast) => {
                    cnn.IsLogin = false;
                    _api.Disconnect();
                });
            _api.OnFrontDisconnected += new MarketApi.FrontDisconnected((int nReasion) => {
                cnn.IsConnected = false;
            });

            _api.Connect();
            Thread.Sleep(500);
        }

        public static void SubscribeMarketData() {
            string instrumentID = "IF2006";
            _api.OnRspSubMarketData += new MarketApi.RspSubMarketData((ref CThostFtdcSpecificInstrumentField pSpecificInstrument,
            ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast) => {
                Console.WriteLine("订阅{0}成功", instrumentID);

                ////退订行情
                //_api.UnsubscribeMarketData(instrumentID);
                //Thread.Sleep(50);
            });
            _api.OnRspUnSubMarketData += new MarketApi.RspUnSubMarketData((ref CThostFtdcSpecificInstrumentField pSpecificInstrument,
            ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast) => {
                Console.WriteLine("退订{0}成功", instrumentID);
            });
            _api.OnRtnDepthMarketData += new MarketApi.RtnDepthMarketData((ref CThostFtdcDepthMarketDataField pDepthMarketData) => {
                Console.WriteLine("昨收价：{0}，现价：{1}", pDepthMarketData.PreClosePrice, pDepthMarketData.LastPrice);
            });

            //订阅行情
            _api.SubscribeMarketData(instrumentID);
            Thread.Sleep(50);
        }
    }
}
