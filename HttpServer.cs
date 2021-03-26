using shortlink;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;



public class WebServer
{
    static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    public static void Start()
    {
        int port = Convert.ToInt32(new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build().GetSection("Port").Value);

        socket.Bind(new IPEndPoint(IPAddress.Any, port));

        socket.Listen(100);

        socket.BeginAccept(OnAccept, socket);

        Console.WriteLine("監聽的端口為：" + port);
    }


    public static void OnAccept(IAsyncResult async)
    {
        var serverSocket = async.AsyncState as Socket;

        var clientSocket = serverSocket.EndAccept(async);

        serverSocket.BeginAccept(OnAccept, serverSocket);

        var bytes = new byte[10000];
        var len = clientSocket.Receive(bytes);

        var request = Encoding.UTF8.GetString(bytes, 0, len);

        #region �������
        //http����Ӧ����
        string response = string.Empty;

        //����http����Ӧͷ
        var responseHeader = string.Empty;

        //��������
        var addr = string.Empty;

        
        //�������ͣ�post\get...��
        var requsetType = request.Split("\r\n")[0].Split(" ")[0];

        //��������
        addr = request.Split("\r\n")[1].Split(" ")[1];

        //����URL����·��
        var filePath = string.Empty;

        //����
        Dictionary<string, string> UrlKey = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(request) && !request.Contains("favicon.ico"))
        {
            filePath = request.Split("\r\n")[0].Split(" ")[1].TrimStart('/');

            if (requsetType.ToLower() == "post")
            {
                string body = request.Split("\r\n\r\n")[1];
                UrlKey = Helper.DeserializeStringToDictionary<string, string>(body);
            }

            //�жϲ����Ƿ�Ϊ��
            if (!string.IsNullOrEmpty(filePath) || UrlKey != null)
            {
                if (requsetType.ToLower() == "get")
                {
                    UrlKey = Helper.ParseQueryString(request.Split("\r\n")[1].Split(" ")[1] + filePath, requsetType);
                }
            }
        } 
        #endregion

        #region ������

        //����Request
        var result = GetResult(UrlKey,filePath,addr);

        //��ȡResponse
        GetResponse(result, UrlKey, addr, out responseHeader, out response);

        #endregion

        clientSocket.Send(Encoding.UTF8.GetBytes(responseHeader));
        if (!string.IsNullOrEmpty(response))
        {
            clientSocket.Send(Encoding.UTF8.GetBytes(response));
        }
        clientSocket.Close();
    }

    #region ������-����1

    /*
     * 
     * [Get]
     * ��ת��http://localhost:3331/��
     * ����[�ɴ�&]��http://localhost:3331/?url=http://xxx.xx.xx?fdsaf&fdsafdsf��
     * [url�в�������OperationType�ظ�������url����󡢿ɴ�&]��http://localhost:3331/?name=1&url=http://xxx.xx.xx?fdsaf��
     * ������http://localhost:3331/?type=addOne&name=1&url=http://xxx.xx.xx?fdsaf��
     * ɾ����http://localhost:3331/?type=delOne&name=h1wymOVO9��
     * ��ѯ��http://localhost:3331/?type=getOne&name=h1wymOVO9��
     * ��ѯ�����http://localhost:3331/?type=getAll&name=h1wym��
     * ���¡�http://localhost:3331/?type=getAll&name=1&url=http://xxx.xx.xx?fdsaf��
     * 
     * [Post]
     * http://localhost:3331
     * ����{"url":""}
     * ����{"url":"","name":"xxx"}
     * ��ѯȫ��{"rquesttype":"json","type":"getAll","name":""}
     * ��ѯ����{"rquesttype":"json","type":"getOne","name":""}
     * ��ѯ����{"rquesttype":"json","type":"delOne","name":""}
     * ����{"rquesttype":"json","type":"addOne","name":"","url":"http://localhost:3331/?type=getAll&name=h1wym"}
     * ����{"rquesttype":"json","type":"setOne","name":"","url":"http://localhost:3331/?type=getAll&name=h1wym"}
     */
    public static Result GetResult(Dictionary<string, string> UrlKey, string filePath, string addr)
    {


        //��������
        Result result = new Result() { IsSucceed = false, Type = OperationType.@goto, Content = "", Message = "����ʧ�ܣ�" };

        #region ������Ϣ����
        using (var db = new SqlContext())
        {
            //[����·��ֱ����ת]http://xxxxxxxxx/41wqmMQJ8 or http://xxxxxxxxx/?name=41wqmMQJ8
            if (UrlKey.Count == 0 || (UrlKey.Count == 1 && UrlKey.ContainsKey("name")))
            {
                result.Type = OperationType.@goto;

                var sl = db.ShortLink.FirstOrDefault(m => m.Name == filePath);
                if (sl == null && UrlKey.ContainsKey("name"))
                {
                    sl = db.ShortLink.FirstOrDefault(m => m.Name == UrlKey["name"]);
                }

                if (sl != null && !string.IsNullOrEmpty(sl.OrgLink))
                {
                    result.IsSucceed = true;
                    result.Content = sl.OrgLink;
                }
                else
                {
                    result.IsSucceed = false;
                    result.Message = "��תʧ�ܣ�";
                }
            }

            //[����]http://xxxxxxxxx?url=http://localhost:3331 or http://xxxxxxxxx?url=http://localhost:3331&name=123
            else if ((UrlKey.Count == 1 && UrlKey.ContainsKey("url")) || (UrlKey.Count == 2 && UrlKey.ContainsKey("url") && UrlKey.ContainsKey("name")))
            {
                result.Type = OperationType.addOne;

                ShortLink shortLink = new ShortLink();
                shortLink.OrgLink = UrlKey["url"];
                shortLink.CreateTime = DateTime.Now;
                shortLink.UpdateTime = DateTime.Now;
                if (UrlKey.ContainsKey("name"))
                {
                    shortLink.Name = UrlKey["name"];
                }
                else
                {
                    shortLink.Name = Helper.GetRandomStr();
                }

                int dbReults = 0;
                if (db.ShortLink.Find(shortLink.Name) == null)
                {
                    var dbResult = db.ShortLink.Add(shortLink);
                    dbReults = db.SaveChanges();
                }

                if (dbReults > 0)
                {
                    result.IsSucceed = true;
                    result.Message = "�����ɹ���";
                    result.Content = shortLink.Name;
                }
                else
                {
                    result.IsSucceed = false;
                    result.Content = shortLink.Name;
                    result.Message = "����ʧ�ܣ� ��Name�Ѵ��ڣ�";
                }
            }

            //[ɾ������ѯһ�������¡���ѯ���-��ҳ]http://xxxxxxxxx?type=yyy
            else if (UrlKey.ContainsKey("type"))
            {
                result.Message = "����ʧ�ܣ�";

                bool isOk = false;
                try
                {
                    result.Type = (OperationType)Enum.Parse(typeof(OperationType), UrlKey["type"]);
                    isOk = true;
                }
                catch (Exception)
                {
                    isOk = false;
                }

                //�ж��ַ�תö���Ƿ�ɹ�
                if (isOk)
                {
                    switch (result.Type)
                    {
                        case OperationType.addOne:
                            #region ����
                            result.Message = "����ʧ�ܣ�";
                            if (UrlKey.ContainsKey("url"))
                            {
                                ShortLink shortLink = new ShortLink();
                                shortLink.OrgLink = UrlKey["url"];
                                if (UrlKey.ContainsKey("url"))
                                {
                                    shortLink.Name = UrlKey["name"];
                                }
                                else
                                {
                                    shortLink.Name = Helper.GetRandomStr();
                                }

                                int dbReults = 0;
                                if (db.ShortLink.Find(shortLink.Name) == null)
                                {
                                    var dbResult = db.ShortLink.Add(shortLink);
                                    dbReults = db.SaveChanges();
                                }
                                else
                                {
                                    result.Message += " ��Name�Ѵ��ڣ�";
                                    result.Content = shortLink.Name;
                                }

                                if (dbReults > 0)
                                {
                                    result.IsSucceed = true;
                                    result.Message = "���ӳɹ���";
                                    result.Content = shortLink.Name;
                                }
                            }
                            #endregion
                            break;
                        case OperationType.delOne:
                            #region ɾ��
                            result.Message = "ɾ��ʧ�ܣ�";
                            if (UrlKey.ContainsKey("name"))
                            {
                                ShortLink shortLink = new ShortLink();
                                shortLink.Name = UrlKey["name"];

                                int dbReults = 0;
                                if (db.ShortLink.Find(shortLink.Name) != null)
                                {
                                    var dbResult = db.ShortLink.Remove(shortLink);
                                    dbReults = db.SaveChanges();
                                }
                                else
                                {
                                    result.Message += " ��Name�����ڣ�";
                                    result.Content = shortLink.Name;
                                }

                                if (dbReults > 0)
                                {
                                    result.IsSucceed = true;
                                    result.Message = "ɾ���ɹ���";
                                    result.Content = shortLink.Name;
                                }
                            }
                            #endregion
                            break;
                        case OperationType.getOne:
                            result.Message = "��ѯʧ�ܣ�";
                            if (UrlKey.ContainsKey("name"))
                            {
                                ShortLink shortLink = new ShortLink();
                                shortLink.Name = UrlKey["name"];

                                shortLink = db.ShortLink.FirstOrDefault(m => m.Name == UrlKey["name"]);

                                if (shortLink != null && !string.IsNullOrEmpty(shortLink.OrgLink))
                                {
                                    result.IsSucceed = true;
                                    result.Content = shortLink;
                                    result.Message = "��ѯ�ɹ���";
                                }
                            }
                            break;
                        case OperationType.setOne:
                            result.Message = "����ʧ�ܣ�";
                            if (UrlKey.ContainsKey("name") && UrlKey.ContainsKey("url"))
                            {
                                ShortLink shortLink = new ShortLink();
                                shortLink.Name = UrlKey["name"];
                                shortLink.OrgLink = UrlKey["url"];

                                int dbReults = 0;
                                if (db.ShortLink.Find(shortLink.Name) != null)
                                {
                                    db.ShortLink.UpdateRange(shortLink);
                                    dbReults = db.SaveChanges();
                                }
                                else
                                {
                                    result.Message += " ��Name�����ڣ�";
                                    result.Content = shortLink.Name;
                                }

                                if (dbReults > 0)
                                {
                                    result.IsSucceed = true;
                                    result.Message = "���³ɹ���";
                                    result.Content = shortLink.Name;
                                }
                            }
                            break;
                        case OperationType.getAll:
                            result.Message = "ģ����ѯʧ�ܣ�";
                            if (UrlKey.ContainsKey("name"))
                            {
                                var list = db.ShortLink.Where(m => m.Name.Contains(UrlKey["name"])).ToList();

                                if (list.Count > 0)
                                {
                                    result.IsSucceed = true;
                                    result.Content = list;
                                    result.Message = "ģ����ѯ�ɹ���";
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        #endregion
        return result;
    }

    public static void GetResponse(Result result, Dictionary<string, string> UrlKey, string addr, out string responseHeader, out string response)
    {

        #region �������ݴ���

        //��ת
        if (result.Type == OperationType.@goto)
        {
            response = string.Empty;
            responseHeader = string.Format(@"HTTP/1.1 302 Found
Location: {0}

                ", result.Content);
        }

        //��������
        else
        {
            //��ȡJSON
            if (UrlKey.ContainsKey("rquesttype") && UrlKey["rquesttype"] == Enum.GetName(typeof(RquestType), RquestType.json))
            {
                response = JsonConvert.SerializeObject(result);
                responseHeader = string.Format(@"HTTP/1.1 200 OK
Date: " + DateTime.UtcNow + @"
Server: nginx
Content-Type: text/html;charset=utf-8
Cache-Control: no-cache
Pragma: no-cache
Via: hngd_ax63.139
X-Via: 1.1 tjhtapp63.147:3800, 1.1 cbsshdf-A4-2-D-14.32:8101
Content-Length: {0}

", Encoding.UTF8.GetByteCount(response));
            }

            //��ȡҳ������
            else
            {
                //������
                if (!result.IsSucceed)
                {
                    response = string.Format(@"<html>
                    <head>
                    <title></title>
                    </head>
                    <body>
<h1>{0}</h1>
<div>404-{1}</div>
                    </body>
                    </html>", result.Message, Enum.GetName(typeof(OperationType), result.Type));
                    responseHeader = string.Format(@"HTTP/1.1 200 OK
Date: " + DateTime.UtcNow + @"
Server: nginx
Content-Type: text/html;charset=utf-8
Cache-Control: no-cache
Pragma: no-cache
Via: hngd_ax63.139
X-Via: 1.1 tjhtapp63.147:3800, 1.1 cbsshdf-A4-2-D-14.32:8101
Content-Length: {0}

", Encoding.UTF8.GetByteCount(response));
                }
                else
                {
                    switch (result.Type)
                    {
                        //��������
                        case OperationType.addOne:
                            var url = "http://" + addr + "/" + result.Content;
                            response = @"<html>
                    <head>
                    <title></title>
                    </head>
                    <body>
                    <a href='" + url + "' target='_blank'>" + url + @"</a>
                    </body>
                    </html>";

                            break;
                        case OperationType.delOne:
                            response = string.Format(@"<html>
                    <head>
                    <title></title>
                    </head>
                    <body>
<h1>ɾ��-{0}</h1>
<div>{1}</div>
                    </body>
                    </html>", result.Message, result.Content);
                            break;
                        case OperationType.getOne:
                            response = string.Format(@"<html>
                    <head>
                    <title></title>
                    </head>
                    <body>
<h1>������ѯ-{0}</h1>
<div>{1}</div>
                    </body>
                    </html>", result.Message, result.Content);
                            break;
                        case OperationType.getAll:
                            response = string.Format(@"<html>
                    <head>
                    <title></title>
                    </head>
                    <body>
<h1>�б���ѯ-{0}</h1>
<div>{1}</div>
                    </body>
                    </html>", result.Message, result.Content);
                            break;
                        case OperationType.setOne:
                            response = string.Format(@"<html>
                    <head>
                    <title></title>
                    </head>
                    <body>
<h1>����-{0}</h1>
<div>{1}</div>
                    </body>
                    </html>", result.Message, result.Content);
                            break;
                        default:
                            response = "";
                            break;
                    }
                    responseHeader = string.Format(@"HTTP/1.1 200 OK
Date: " + DateTime.UtcNow + @"
Server: nginx
Content-Type: text/html;charset=utf-8
Cache-Control: no-cache
Pragma: no-cache
Via: hngd_ax63.139
X-Via: 1.1 tjhtapp63.147:3800, 1.1 cbsshdf-A4-2-D-14.32:8101
Content-Length: {0}

", Encoding.UTF8.GetByteCount(response));
                }
            }
        }
        #endregion

    }

    #endregion

}
