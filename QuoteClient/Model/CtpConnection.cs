using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuoteServer.Model {
    public class CtpConnection {
        /// <summary>
        /// 连接地址
        /// </summary>
        public string FrontMkAddr { get; set; } = "180.168.146.187:10110";
        public string FrontTdAddr { get; set; } = "180.168.146.187:10100";

        /// <summary>
        /// 经纪商代码
        /// </summary>
        public string BrokerID { get; set; } = "9999";

        /// <summary>
        /// 投资者账号
        /// </summary>
        public string Investor { get; set; } = "097217";

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = "123456";

        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnected { get; set; } = false;

        /// <summary>
        /// 是否登录
        /// </summary>
        public bool IsLogin { get; set; } = false;
    }
}
