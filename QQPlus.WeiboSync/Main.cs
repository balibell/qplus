using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QQPlus.Framework;
using QQPlus.Framework.SDK;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Net;
using HttpTool;
using System.IO;
using System.Text.RegularExpressions;
using QQPlus.Framework.Entities;
using System.Collections.Generic;

namespace QQPlus.WeiboSync
{
    public class Main : Plugin
    {
        private Thread ThreadMain;

        private ConsoleForm form;

        // QQ group id
        private static readonly string GROUP_DUITANG = "68897644";
        private static readonly string GROUP_ZHOUZI = "130630048";
        private static readonly string GROUP_TEST = "113166072";

        // Weibo user id
        private static readonly string WB_DUITANG = "1746316033";
        private static readonly string WB_ZHOUZI = "1774653601";
        private static readonly string WB_TEST = "1142766351";

        private static Dictionary<string, string> LastBlog = new Dictionary<string, string>();
        private static Dictionary<string, string> MapGroupWB = new Dictionary<string, string>();
        private static Dictionary<string, bool> MapGroupWBOpen = new Dictionary<string, bool>();


        public Main()
        {
            this.PluginName = "fansfriend";
            this.Id = "QQPlus.WeiboSync";
            this.Description = "this a QQPlus Plugin for sending cluster message";
            this.Author = "balibell";
        }
        

        public override bool Install()
        {
            return base.Install();
        }

        public override bool UnInstall()
        {
            return base.UnInstall();
        }

        public override bool Stop()
        {
            if (ThreadMain != null)
            {
                ThreadMain.Abort();
            }
            return base.Stop();
        }

        private void TraceLog(string message)
        {
            if (form != null)
            {
                form.TraceLog(message);
            }
        }

        public override bool ShowForm()
        {
            form = new ConsoleForm(this);
            form.ShowDialog();
            // 初始化 form
            //form.InitPanel();
            return base.ShowForm();
        }

        public override bool Start()
        {

            MapGroupWBOpen.Add(GROUP_DUITANG, false);
            MapGroupWBOpen.Add(GROUP_ZHOUZI, true);
            MapGroupWBOpen.Add(GROUP_TEST, true);

            MapGroupWB.Add(GROUP_DUITANG, WB_DUITANG);
            MapGroupWB.Add(GROUP_ZHOUZI, WB_ZHOUZI);
            MapGroupWB.Add(GROUP_TEST, WB_TEST);




            sdk = new QQClientSDK();

            // 订阅事件，并生成相应的事件处理方法
            sdk.ReceiveNormalIM += Sdk_ReceiveNormalIM;
            sdk.ReceiveClusterIM += Sdk_ReceiveClusterIM;

            ThreadMain = new Thread(new ThreadStart(StartFetchWork)); //注意ThreadStart委托的定义形式
            ThreadMain.Start(); //线程开始，控制权返回Main线程
            return base.Start();
        }

        private void ResendWeiboInner(string key)
        {
            if (LastBlog.ContainsKey(key))
            {

                //实例化ThreadWithState类，为线程提供参数
                ThreadWithState tws = new ThreadWithState(this,
                    LastBlog[key], key);

                // 创建执行任务的线程，并执行
                Thread t = new Thread(new ThreadStart(tws.ThreadProc));
                t.Start();
            }
        }


        public void ResendWeibo()
        {
            ResendWeiboInner(WB_ZHOUZI);
        }

        public void ResendWeiboDuitang()
        {
            ResendWeiboInner(WB_DUITANG);
        }

        public void ResendWeiboTest()
        {
            ResendWeiboInner(WB_TEST);
        }

        // run in resendThread
        // ThreadWithState 类里包含了将要执行的任务以及执行任务的方法
        public class ThreadWithState
        {
            //要用到的属性，也就是我们要传递的参数
            private Main main;
            private string blogid;
            private string wbuid;

            //包含参数的构造函数
            public ThreadWithState(Main main, string blogid, string wbuid)
            {
                this.main = main;
                this.blogid = blogid;
                this.wbuid = wbuid;
            }

            //要丢给线程执行的方法，本处无返回类型就是为了能让ThreadStart来调用
            public void ThreadProc()
            {
                //这里就是要执行的任务,本处只显示一下传入的参数
                this.main.TraceLog("上一次微博id：" + blogid);
                if (blogid != null && blogid != "")
                {
                    string tagUrl = "http://dtxn.sinaapp.com/wormhole/timeline?blogid=" + blogid;
                    CookieCollection cookies = new CookieCollection();
                    HttpWebResponse response = HttpWebResponseUtility.CreateGetHttpResponse(tagUrl, null, null, cookies);

                    System.Threading.Thread.Sleep(1000);
                    this.main.WakeUpMainThread();
                }
            }
        }


        // run in main thread
        private void StartFetchWork()
        {


            while (true)
            {

                // 7点 —— 凌晨1点 同步qq群工作时间
                DateTime now = DateTime.Now;
                now = now.AddHours(-1);
                int hour = now.Hour;
                if ( hour > 6 && hour < 24 && (MapGroupWBOpen[GROUP_DUITANG] || MapGroupWBOpen[GROUP_ZHOUZI] || MapGroupWBOpen[GROUP_TEST]))
                {
                    string html = FetchWeibo();

                    // 是否开启堆糖群同步开关
                    if (MapGroupWBOpen[GROUP_DUITANG])
                    {
                        SendToQQ(html, GROUP_DUITANG/*qq群*/, MapGroupWB[GROUP_DUITANG]/*微博uid*/, "");
                    }


                    // 是否开启语丝群同步开关
                    if (MapGroupWBOpen[GROUP_ZHOUZI])
                    {
                        SendToQQ(html, GROUP_ZHOUZI/*qq群*/, MapGroupWB[GROUP_ZHOUZI]/*微博uid*/, "");
                    }


                    // 是否开启贝尔群同步开关
                    if (MapGroupWBOpen[GROUP_TEST])
                    {
                        SendToQQ(html, GROUP_TEST/*qq群*/, MapGroupWB[GROUP_TEST]/*微博uid*/, "");
                    }
                }

                try
                {
                    System.Threading.Thread.Sleep(30000);

                }
                catch (ThreadInterruptedException e)
                {
                    // nothing
                    this.TraceLog("线程强制中断 ThreadInterruptedException");
                }

            }
        }

        public void WakeUpMainThread()
        {
            if (ThreadMain.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
            {
                ThreadMain.Interrupt();
            }
        }




        private string FetchWeibo()
        {
            string totalUids = WB_DUITANG+","+WB_ZHOUZI+","+WB_TEST;
            string tagUrl = "http://dtxn.sinaapp.com/convenience/timeline?uids="+ totalUids;
            CookieCollection cookies = new CookieCollection();
            HttpWebResponse response = HttpWebResponseUtility.CreateGetHttpResponse(tagUrl, null, null, cookies);

            Stream stream = response.GetResponseStream();   //获取响应的字符串流
            StreamReader sr = new StreamReader(stream); //创建一个stream读取流
            string html = sr.ReadToEnd();   //从头读到尾，放到字符串html李米

            //this.TraceLog("request over machine time: " + DateTime.Now.ToString());
            //DateTime dt = DateTime.ParseExact(response.Headers["Date"], "r", System.Globalization.CultureInfo.InvariantCulture);
            //this.TraceLog("response header date" + (response.Headers["Date"]));
            //this.TraceLog("parseExact "+dt.ToString());


            this.TraceLog("FetchWeibo request "+ tagUrl);
            return html;
        }


        // 113166072 测试群
        // 130630048 理性与人文群
        private void SendToQQ(string html, string groupid, string wbuid, string nickname)
        {
            JToken bloginfo = null;
            string blogid = "";
            try
            {
                JObject json = JObject.Parse(html);

                bloginfo = json.GetValue(wbuid);
                if(bloginfo != null)
                {
                    blogid = bloginfo["idstr"].ToString();
                }
            }
            catch (Exception e)
            {
                // parse failed
                this.TraceLog(e.ToString());
                return;
            }


            this.TraceLog("start sending blog【"+blogid+"】 to qq group【"+groupid+"】");
            

            if (bloginfo != null && blogid != "")
            {
                string text = bloginfo["text"].ToString();
                string pics = "";

                string rtext = bloginfo["r_text"].ToString();
                if (rtext != "")
                {
                    text += "\n//" + bloginfo["r_user"] + ":\n" + rtext;
                    pics = bloginfo["r_pics"].ToString().Trim();
                }else
                {
                    pics = bloginfo["pics"].ToString().Trim();
                }
                string textpic = "";
                if ( pics.Length > 0 )
                {
                    string[] picsArray = pics.Split(',');
                    int len = picsArray.Length;

                    // 如果是九宫格组图，取中间一张图显示
                    if(len == 9)
                    {
                        textpic += "\n[image=" + picsArray[4] + "]";
                    }
                    else // 不是九宫格，则从头显示
                    {
                        for (int i = 0; i < len && i < 1; i++)
                        {
                            textpic += "\n[image=" + picsArray[i] + "]";
                        }
                    }
                }

                this.TraceLog("qq群" + groupid + " 有微博更新了~" + text);
                this.SendNewWeiboToCluster(groupid, nickname + text + textpic);
                this.TraceLog("图片地址: " + textpic);


                // notify server that text of this blog has been sent
                string clearUrl = "http://dtxn.sinaapp.com/convenience/clearblog/" + wbuid + "?blogid=" + blogid;
                HttpWebResponse response = HttpWebResponseUtility.CreateGetHttpResponse(clearUrl, null, null, null);

                LastBlog[wbuid] = blogid;
            }
        }

        private void SendNewWeiboToCluster(string groupid, string message)
        {
            if (message != "")
            {
                Cluster group = client.GetCluster(uint.Parse(groupid));
                client.SendClusterMessage(group, null, message);
            }
        }

        
        

        private void Sdk_ReceiveNormalIM(object sender, ReceiveNormalIMQQEventArgs e)
        {
            // 如果匹配到是图片地址
            string pattern = @"http(s)?:\/\/.+\.(jpg|png|gif|jpeg)";
            MatchCollection ms = Regex.Matches(e.Message.Trim(), pattern);


            if (ms.Count > 0)
            {
                string sendmessage = "[image=http://qr.liantu.com/api.php?text=hehe]";
                client.SendMessage(e.QQ, sendmessage);
            }

            e.Cancel = true;
        }

        private void Sdk_ReceiveClusterIM(object sender, ReceiveClusterIMQQEventArgs e)
        {
            string strGroupID = e.Cluster.ExternalId.ToString();

            this.TraceLog(e.Message);

            if (e.Message.IndexOf("http://fir.im") > -1 || e.Message.IndexOf("https://fir.im") > -1)
            {
                // 如果匹配到是 fir 地址
                string pattern = @"http(s)?:\/\/fir.im\/[^\s]+";
                MatchCollection ms = Regex.Matches(e.Message.Trim(), pattern);
                if( ms.Count > 0)
                {
                    string sendmessage = GetQRInfo(ms[0].Value);
                    SendNewWeiboToCluster(strGroupID, sendmessage);
                }
            }
            else
            {
                // 如果匹配到是图片地址
                string pattern = @"http(s)?:\/\/.+\.(jpg|png|gif|jpeg)(\?\d*)?";
                MatchCollection ms = Regex.Matches(e.Message.Trim(), pattern);

                if (ms.Count > 0 && (strGroupID == GROUP_TEST || strGroupID == GROUP_DUITANG))
                {
                    this.TraceLog(e.Message);
                    string sendmessage = "[image=" + ms[0].Value + "]";
                    SendNewWeiboToCluster(strGroupID, sendmessage);
                    
                }
                else if (e.Message.IndexOf("莫小贝") > -1)
                {
                    string sendms = "";
                    if (e.Message == "莫小贝重发")
                    {
                        if (MapGroupWB.ContainsKey(strGroupID))
                        {
                            ResendWeiboInner(MapGroupWB[strGroupID]);
                        }
                    }else if (e.Message == "莫小贝够了")
                    {
                        MapGroupWBOpen[strGroupID] = false;
                        sendms = "没劲透了你们";
                    }
                    else if (e.Message == "莫小贝发吧")
                    {
                        MapGroupWBOpen[strGroupID] = true;
                        sendms = "把本座的宝贝儿拿上来";
                    }
                    else
                    {
                        string[] robotms = new string[] {
                            "喊我干嘛，嫂子都还没睡醒呢...",
                            "那年夏天，膝盖中了一箭",
                            "我身在江湖，江湖却没有关于我的传说。",
                            "嫂子——",
                            "我是五岳盟主，我说话你们都得听的！",
                            "你死也是衡山派的死人！",
                            "感觉再也会不爱了~[face15.gif]"
                        };

                        int count = robotms.Length;
                        Random ra = new Random();
                        int next = ra.Next(0, count);
                        sendms = robotms[next];
                    }

                    if (sendms != "")
                    {
                        SendNewWeiboToCluster(strGroupID, sendms);
                    }
                }
            }

            e.Cancel = true;
        }

        private string GetQRInfo(string firUrl)
        {
            //firUrl = Regex.Replace(firUrl, "https://", "http;//");

            HttpWebResponse response = HttpWebResponseUtility.CreateGetHttpResponse(firUrl, null, HttpWebResponseUtility.UserAgentMob, null);

            Stream stream = response.GetResponseStream();   //获取响应的字符串流
            StreamReader sr = new StreamReader(stream); //创建一个stream读取流
            string html = sr.ReadToEnd();   //从头读到尾，放到字符串html李米

            MatchCollection ms;
            string pattern;

            //// appName
            //string appName = ""; 
            //pattern = @"<h1 class='value name'>([^<]+)";
            //ms = Regex.Matches(html, pattern);
            //if( ms.Count > 0)
            //{
            //    appName = ms[0].Groups[1].Value.Trim();
            //}


            //// appVersion
            //string appPlatform = "";
            //pattern = @"<span class=""label platform"">([^<]+)";
            //ms = Regex.Matches(html, pattern);
            //if (ms.Count > 0)
            //{
            //    appPlatform = ms[0].Groups[1].Value.Trim();
            //}

            //// appVersion
            //string appVersion = "";
            //pattern = @"<span class=""version"">([^<]+)";
            //ms = Regex.Matches(html, pattern);
            //if (ms.Count > 0)
            //{
            //    appVersion = ms[0].Groups[1].Value.Trim();
            //}


            // appDescMob
            string appDescMob = "";
            pattern = @"<p class=""update-info"">([^z]+?)<\/p>";
            ms = Regex.Matches(html, pattern, RegexOptions.Multiline);

            if (ms.Count > 0)
            {
                appDescMob = ms[0].Groups[1].Value.Trim();

                appDescMob = Regex.Replace(appDescMob, @"(<\/?[^>]+>|&nbsp;|\n|\t)", "");

                appDescMob = Regex.Replace(appDescMob, @" {3,}", "  ");

                appDescMob = Regex.Replace(appDescMob, @"Adhoc", "iOS");
            }
            this.TraceLog(appDescMob);

            string returnValue = appDescMob
                                 + "\n[image=http://qr.liantu.com/api.php?text=" + Uri.EscapeDataString(firUrl) + "]";
            return returnValue;
        }
    }

    
}
