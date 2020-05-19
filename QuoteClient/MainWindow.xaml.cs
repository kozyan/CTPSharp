using CTPCore;
using CTPMarketAdapter;
using CTPMarketAdapter.Adapter;
using CTPTradeAdapter.Adapter;
using CTPTradeAdapter.Model;
using QuoteClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuoteClient {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        /// <summary>
        /// 行情适配器接口实例
        /// </summary>
        private MarketAdapter _mkApi;

        /// <summary>
        /// 交易接口实例
        /// </summary>
        private TradeAdapter _tdApi;


        protected CtpInfo ctpInfo = new CtpInfo();

        /// <summary>
        /// 连接地址
        /// </summary>
        private string _frontMkAddr = "tcp://180.168.146.187:10110";
        private string _frontTdAddr = "tcp://180.168.146.187:10100";

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

        protected string logInfo { get; set; } = string.Empty;

        public MainWindow() {
            InitializeComponent();

            //DataContext = ctpInfo;
            logger.DataContext = ctpInfo;
        }

        protected void Log(string format, string message) {
            Action act = delegate () { 
                logger.Text += $"{string.Format(format, message)}\r\n";
            };

            Dispatcher.Invoke(act);
        }
        protected void Log(string message) {
            Action act = delegate () {
                logger.Text += $"{message}\r\n";
            };

            Dispatcher.Invoke(act);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            InitMK();
            InitTD();
        }

        public void InitMK() {
            _mkApi = new MarketAdapter();
            var connectCallback = new DataCallback((DataResult result) => {
                if(result.IsSuccess) {
                    _isConnected = true;
                    var loginCallback = new DataCallback((DataResult loginResult) => {
                        if(loginResult.IsSuccess) {
                            _isLogin = true;
                            Log("MK登录成功：{0}", loginResult.ReturnCode.ToString());
                        } else {
                            Log("MK登录失败：{0}", loginResult.Error);
                        }
                    });
                    _mkApi.UserLogin(loginCallback, _investor, _password);
                } else {
                    Log("MK连接失败：{0}", result.Error);
                }
            });
            _mkApi.Connect(connectCallback, _brokerID, _frontMkAddr);
        }

        public void InitTD() {
            _tdApi = new TradeAdapter();
            var connectCallback = new DataCallback((DataResult result) => {
                if(result.IsSuccess) {
                    _isConnected = true;
                    var loginCallback = new DataCallback((DataResult loginResult) => {
                        if(loginResult.IsSuccess) {
                            _isLogin = true;
                            _tdApi.SettlementInfoConfirm(null);
                            Log("TD登录成功：{0}", loginResult.ReturnCode.ToString());
                        } else {
                            Console.WriteLine("TD登录失败：{0}", loginResult.Error);
                            Log("TD登录失败：{0}", loginResult.Error);
                        }
                    });
                    _tdApi.UserLogin(loginCallback, _investor, _password);
                } else {
                    Log("TD连接失败：{0}", result.Error);
                }
            });
            _tdApi.Connect(connectCallback, _brokerID, _frontTdAddr);
        }

        public void mkDisconnect() {
            if(_isLogin) {
                var logoutCallback = new DataCallback((DataResult logoutResult) =>
                {
                    if(logoutResult.IsSuccess) {
                        _isLogin = false;
                        Log("登出成功：{0}", logoutResult.ReturnCode.ToString());
                    } else {
                        Log("登出失败：{0}", logoutResult.Error);
                    }
                });
                _mkApi.UserLogout(logoutCallback);
            } else if(_isConnected) {
                var disconnectCallback = new DataCallback((DataResult disconnectResult) => {
                    if(disconnectResult.IsSuccess) {
                        _isConnected = false;
                        Log("登出成功：{0}", disconnectResult.ReturnCode.ToString());
                    } else {
                        Log("登出失败：{0}", disconnectResult.Error);
                    }
                });
                _mkApi.Disconnect(disconnectCallback);
            }
        }

        public void tdDisconnect() {
            if(_isLogin) {
                var logoutCallback = new DataCallback((DataResult logoutResult) =>
                {
                    if(logoutResult.IsSuccess) {
                        _isLogin = false;
                    } else {
                        Log("TD登出失败：{0}", logoutResult.Error);
                    }
                });
                _tdApi.UserLogout(logoutCallback);
            } else if(_isConnected) {
                var disconnectCallback = new DataCallback((DataResult disconnectResult) =>
                {
                    if(disconnectResult.IsSuccess) {
                        _isConnected = false;
                    } else {
                        Log("TD登出失败：{0}", disconnectResult.Error);
                    }
                });
                _tdApi.Disconnect(disconnectCallback);
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e) {
            mkDisconnect();
            tdDisconnect();
        }

        private void btnLoadInstrments_Click(object sender, RoutedEventArgs e) {
            var callback = new DataListCallback<InstrumentInfo>((DataListResult<InstrumentInfo> result) => {
                if(result.IsSuccess) {
                    Log($"合约条数：{result.Result.Count}");

                    foreach(var item in result.Result) {
                        Log($"合约：{item.InstrumentName}({item.InstrumentID})");
                    }
                }
            });
            _tdApi.QueryInstrument(callback, null);
        }

        private void btnUnsubscribe_Click(object sender, RoutedEventArgs e) {
            _mkApi.UnsubscribeMarket();
        }

        private void btnSubscribe_Click(object sender, RoutedEventArgs e) {

            string instrmentID = "IF2007";
            //订阅行情
            _mkApi.OnMarketDataChanged += new MarketDataChangedHandler((market) => {
                Log("订阅：{0}", $"{market.InstrmentID},ask1:{market.AskPrice1},bid1:{market.BidPrice1}");
            });

            _mkApi.SubscribeMarket(instrmentID);
        }
    }
}
