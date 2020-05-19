﻿using CTPCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;


namespace CTPMarketAdapter.Adapter.Tests
{
    /// <summary>
    /// CTP行情适配器测试用例
    /// </summary>
    [TestClass()]
    public class CTPMarketAdapterTest
    {
        /// <summary>
        /// 行情适配器接口实例
        /// </summary>
        private MarketAdapter _adapter;

        /// <summary>
        /// 连接地址
        /// </summary>
        private string _frontAddr = "tcp://180.168.146.187:10110";

        /// <summary>
        /// 经纪商代码
        /// </summary>
        private string _brokerID = "9999";

        /// <summary>
        /// 投资者账号
        /// </summary>
        private string _investor = "097217";

        /// <summary>
        /// 密码
        /// </summary>
        private string _password = "123456";

        /// <summary>
        /// 是否连接
        /// </summary>
        private bool _isConnected;

        /// <summary>
        /// 是否登录
        /// </summary>
        private bool _isLogin;

        /// <summary>
        /// 初始化测试用例
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            _adapter = new MarketAdapter();
            var connectCallback = new DataCallback((DataResult result) =>
            {
                if (result.IsSuccess)
                {
                    _isConnected = true;
                    var loginCallback = new DataCallback((DataResult loginResult) =>
                    {
                        if (loginResult.IsSuccess)
                        {
                            _isLogin = true;
                        }
                        else
                        {
                            Console.WriteLine("登录失败：{0}", loginResult.Error);
                        }
                    });
                    _adapter.UserLogin(loginCallback, _investor, _password);
                    Thread.Sleep(100);
                }
                else
                {
                    Console.WriteLine("连接失败：{0}", result.Error);
                }
            });
            _adapter.Connect(connectCallback, _brokerID, _frontAddr);
            Thread.Sleep(200);
        }

        /// <summary>
        /// 清理测试用例
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            if (_isLogin)
            {
                var logoutCallback = new DataCallback((DataResult logoutResult) =>
                 {
                     if (logoutResult.IsSuccess)
                     {
                         _isLogin = false;
                     }
                     else
                     {
                         Console.WriteLine("登出失败：{0}", logoutResult.Error);
                     }
                 });
                _adapter.UserLogout(logoutCallback);
                Thread.Sleep(100);
            }
            else if (_isConnected)
            {
                var disconnectCallback = new DataCallback((DataResult disconnectResult) =>
                {
                    if (disconnectResult.IsSuccess)
                    {
                        _isConnected = false;
                    }
                    else
                    {
                        Console.WriteLine("登出失败：{0}", disconnectResult.Error);
                    }
                });
                _adapter.Disconnect(disconnectCallback);
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 测试获取交易日
        /// </summary>
        [TestMethod()]
        public void TestGetTradingDay()
        {
            string result = _adapter.GetTradingDay();
            Assert.AreEqual(8, result.Length);
        }

        /// <summary>
        /// 测试订阅行情
        /// </summary>
        [TestMethod()]
        public void TestSubscribeMarket()
        {
            string instrmentID = "IF2007";
            //订阅行情
            _adapter.OnMarketDataChanged += new MarketDataChangedHandler((market) =>
            {
                Assert.AreEqual(instrmentID, market.InstrmentID);
            });

            _adapter.SubscribeMarket(instrmentID);
            Thread.Sleep(100);
            _adapter.UnsubscribeMarket(instrmentID);
        }
    }
}