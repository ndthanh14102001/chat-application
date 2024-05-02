using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MySqlX.XDevAPI;
using server;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Server
{
    public partial class Form1 : Form
    {
        private delegate void CallDelegate(string text);
        Socket server;
        Dictionary<string, string> DSAccount = new Dictionary<string, string>(); // "username": "password"
        Dictionary<string, Socket> DSClient = new Dictionary<string, Socket>(); // "username": Socket
        Dictionary<string, List<string>> DSGroup = new Dictionary<string, List<string>>(); // "groupname": List["username"]
        private int size = 10485760 * 10 ;
        private byte[] data = new byte[10485760 * 10];

        private Security securityAlgorithm = new Security();
        public Form1()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine(securityAlgorithm.Encrypt("abcdef"));
            System.Diagnostics.Debug.WriteLine(securityAlgorithm.Decrypt("mnacgg"));
        }

        private void Start_Click(object sender, EventArgs e)
        {
            IPEndPoint iep = new IPEndPoint(IPAddress.Parse(IP.Text), int.Parse(PORT.Text));
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            server.Bind(iep);
            server.Listen(10);
            Start.Enabled = false;
            SetText("Server has Started!");
            server.BeginAccept(new AsyncCallback(AcceptClientThread), null);
        }

        private void AcceptClientThread(IAsyncResult iar)
        {
            try
            {
                Socket client = server.EndAccept(iar);

                server.BeginAccept(new AsyncCallback(AcceptClientThread), null);

               // SetText("Connected from: " + client.RemoteEndPoint.ToString());

                Response res = new Response("Welcome to server");
                Common common = new Common("OK", ParseToJson(res));
                //SendData(client, common);
                ReceiveData(client);
            }
            catch (Exception)
            {
            }
        }

        private void SendData(Socket client, object obj)
        {
            string json = ParseToJson(obj);
            string encryptedData = securityAlgorithm.Encrypt(json);
            byte[] message = Encoding.ASCII.GetBytes(encryptedData);
            client.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(SendDataCallback), client);
        }
        private void SendDataCallback(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            int sent = client.EndSend(iar);
        }

        private void ReceiveData(Socket client)
        {
            client.BeginReceive(data, 0, size, SocketFlags.None, new AsyncCallback(handleClient), client);
        }

        private void handleClient(IAsyncResult iar)
        {
            try
            {
                Socket client = (Socket)iar.AsyncState;
                int recv = client.EndReceive(iar);

                string encryptedData = Encoding.ASCII.GetString(data, 0, recv);
                string jsonString = securityAlgorithm.Decrypt(encryptedData);

                Common conn = JsonSerializer.Deserialize<Common>(jsonString);

                if (conn != null && conn.Content != null)
                {
                    switch (conn.Kind)
                    {
                        case "LOGIN":
                            {
                                Login userInfo = JsonSerializer.Deserialize<Login>(conn.Content);
                                string username = userInfo.Username;
                                string password = userInfo.Password;
                                User user = new User();
                                user = user.login(username, password);
                                if(user != null)
                                {

                                    DSClient.Remove(username);
                                    DSClient.Add(username, client);

                                    Channel_Friend channel_Friend = new Channel_Friend();
                                    List<Channel_Friend> channel_Friends = new List<Channel_Friend>();

                                    channel_Friends = channel_Friend.getChannelFriend(username);

                                    RESPONSELOGIN response = new RESPONSELOGIN();
                                    response.Kind = "OK";
                                    response.Content = channel_Friends;
                                    ResponseToClient(client, "LOGIN", response);
                                    ReceiveData(client);
                                    SetText($"{username} logged in!");
                                }
                                else
                                {
                                    RESPONSELOGIN response = new RESPONSELOGIN();
                                    response.Kind = "CANCEL";
                                    response.Content = "Login failed!";
                                    ResponseToClient(client, "LOGIN", response);
                                    ReceiveData(client);
                                }
                            }
                            break;
                        case "REGISTER":
                            {
                                Login userInfo = JsonSerializer.Deserialize<Login>(conn.Content);
                                string username = userInfo.Username;
                                string password = userInfo.Password;
                                User user = new User();
                                if (user.Register(username, password))
                                {
                                    ResponseToClient(client, "REGISTER", "OK");
                                }
                                else
                                {
                                    ResponseToClient(client, "REGISTER", "CANCEL");
                                }
                            }
                            break;
                        case "UPDATESOCKET":
                            {
                                string username = conn.Content;
                                if (username != null)
                                {
                                    if (DSClient.ContainsKey(username))
                                    {
                                        DSClient.Remove(username);
                                        DSClient.Add(username, client);
                                    }
                                }
                            }
                            break;
                        case "LOGOUT":
                            {
                                Logout logout = JsonSerializer.Deserialize<Logout>(conn.Content);
                                string username = logout.Username;
                                if (DSClient.ContainsKey(username))
                                {
                                    ResponseToClient(client, "LOGOUT", "Successfully logged out!");
                                    DSClient.Remove(username);
                                    SetText($"{username} logged out!");
                                    Socket clientLogout = (Socket)iar.AsyncState;
                                    clientLogout.EndConnect(iar);
                                }
                            }
                            break;
                        case "CHAT":
                            {
                                Chat msg = JsonSerializer.Deserialize<Chat>(conn.Content);
                                msg.Kind = 0;

                                string sender = msg.Sender;
                                int channel_id = msg.Channel_id;
                                string content = msg.Content;

                                if (sender != null && channel_id != null && content != null)
                                {
                                    MessageChat messageChatMethod = new MessageChat();
                                    messageChatMethod.AddMessageToChannelId(channel_id, sender, content);

                                    Channel channelMethod = new Channel();
                                    List<string> usernames = channelMethod.getUsernameListByChannelId(channel_id, sender);
                                    foreach (string username in usernames)
                                    {
                                        bool equal = DSClient.ContainsKey(username);
                                        if (DSClient.ContainsKey(username))
                                        {
                                            SetText($"{sender} gui {username}: {content + Environment.NewLine}");
                                            Socket friend = DSClient[username];

                                            ResponseToClient(friend, "CHAT", msg);
                                        }
                                        /*if (DSGroup.ContainsKey(username))
                                        {
                                            foreach (string user in DSGroup[username])
                                            {
                                                if (DSClient.ContainsKey(user))
                                                {
                                                    Socket friend = DSClient[user];
                                                    SendData(friend, conn);
                                                }
                                            }
                                            SetText($"{sender} send to {username}: {content + Environment.NewLine}");
                                            break;
                                        }*/
                                    }
                                    //ResponseToClient(DSClient[sender], "CHAT", msg);
                                }
                            }
                            break;
                        case "SENDIMAGE":
                            {
                                SENDFILE msgImage = JsonSerializer.Deserialize<SENDFILE>(conn.Content);
                                if (conn.Content != null)
                                {

                                    MessageChat messageChat = new MessageChat();
                                    messageChat.AddMessageFileToChannelId(msgImage.Channel_id, msgImage.Sender, msgImage.File, msgImage.FileName,1);

                                    Chat chat = new Chat();
                                    chat.Channel_id = msgImage.Channel_id;
                                    chat.Sender = msgImage.Sender;
                                    chat.File = msgImage.File;
                                    chat.FileName = msgImage.FileName;
                                    chat.Kind = 1;

                                    Channel channelMethod = new Channel();
                                    List<string> usernames = channelMethod.getUsernameListByChannelId(msgImage.Channel_id, msgImage.Sender);
                                    foreach (string username in usernames)
                                    {
                                        if (DSClient.ContainsKey(username))
                                        {
                                            SetText($"{msgImage.Sender} gui {username}: {msgImage.FileName + Environment.NewLine}");
                                            Socket friend = DSClient[username];

                                            ResponseToClient(friend, "CHAT", chat);
                                        }
                                    }
                                }
                            }
                            break;
                        case "SENDFILE":
                            {
                                SENDFILE msgFile = JsonSerializer.Deserialize<SENDFILE>(conn.Content);
                                if (conn.Content != null)
                                {

                                    MessageChat messageChat = new MessageChat();
                                    messageChat.AddMessageFileToChannelId(msgFile.Channel_id, msgFile.Sender, msgFile.File, msgFile.FileName, 2);

                                    Chat chat = new Chat();
                                    chat.Channel_id = msgFile.Channel_id;
                                    chat.Sender = msgFile.Sender;
                                    chat.File = msgFile.File;
                                    chat.FileName = msgFile.FileName;
                                    chat.Kind = 2;

                                    Channel channelMethod = new Channel();
                                    List<string> usernames = channelMethod.getUsernameListByChannelId(msgFile.Channel_id, msgFile.Sender);
                                    foreach (string username in usernames)
                                    {
                                        if (DSClient.ContainsKey(username))
                                        {
                                            SetText($"{msgFile.Sender} gui {username}: {msgFile.FileName + Environment.NewLine}");
                                            Socket friend = DSClient[username];

                                            ResponseToClient(friend, "CHAT", chat);
                                        }
                                    }
                                }
                            }
                            break;
                        case "ADDGROUP":
                            Group group = JsonSerializer.Deserialize<Group>(conn.Content);
                            string creater = group.Creater;
                            string groupName = group.GroupName;
                            List<string> listUsername = JsonSerializer.Deserialize<List<string>>(group.ListUsername.ToString());
                            listUsername.Add(creater);
                            Channel_Group channel_GroupMethod = new Channel_Group();
                           if (channel_GroupMethod.addGroup(listUsername, groupName))
                            {
                                ResponseToClient(client, "ADDGROUPRESULT", "OK");
                                SetText($"{creater} created group {groupName}!");
                            }
                            else
                            {
                                ResponseToClient(client, "ADDGROUPRESULT", "CANCEL");
                            }
                            /*if (DSGroup.ContainsKey(groupName))
                            {
                                if (DSGroup[groupName].Contains(memberName))
                                {
                                    ResponseToClient(client, "CANCEL", "Member has already existed in group!");
                                }
                                else
                                {
                                    DSGroup[groupName].Add(memberName);
                                    ResponseToClient(client, "OK", "Successfully added!");
                                    SetText($"{memberName} was added to {groupName}!");
                                }
                            }
                            else
                            {
                                DSGroup.Add(groupName, new List<string>() { memberName });
                                ResponseToClient(client, "OK", "Group was successfully added");
                                SetText($"{groupName} was added!");
                            }*/
                            break;
                        case "ADDFRIEND":
                            {
                                ADDFRIEND addFriend = JsonSerializer.Deserialize<ADDFRIEND>(conn.Content.ToString());
                                Channel_Friend channel_Friend_MeThod = new Channel_Friend();

                                int result = channel_Friend_MeThod.addFriend(addFriend.Username, addFriend.Friend);
                                if (result == 1)
                                {
                                    ResponseClient responseClient = new ResponseClient();
                                    responseClient.Kind = "OK";
                                    responseClient.Content = "Add Friend Success!";
                                    SetText(addFriend.Username + " added " + addFriend.Friend + " to list friend!");
                                    ResponseToClient(client, "ADDFRIENDRESULT", responseClient);
                                }
                                else if(result == -1 )
                                {
                                    ResponseClient responseClient = new ResponseClient();
                                    responseClient.Kind = "CANCEL";
                                    responseClient.Content = "Username not exists!";
                                    ResponseToClient(client, "ADDFRIENDRESULT", responseClient);
                                }
                                else if(result == 0)
                                {
                                    ResponseClient responseClient = new ResponseClient();
                                    responseClient.Kind = "CANCEL";
                                    responseClient.Content = addFriend.Username + " already exists your friends list!";
                                    ResponseToClient(client, "ADDFRIENDRESULT", responseClient);
                                }
                            }
                            break;
                        case "DELETECHANNEL":
                            {
                                int channel_id = int.Parse(conn.Content);
                                Channel channelMethod = new Channel();

                                channelMethod.deleteChannel(channel_id);
                                ResponseToClient(client, "DELETERESULT", "OK");
                            }
                            break;
                        case "GETMESSAGE":
                            {
                                MESSAGELIST clientRequest = JsonSerializer.Deserialize<MESSAGELIST>(conn.Content);

                                MessageChat messageChatMedthod = new MessageChat();
                                List<MessageChat> messageChats = new List<MessageChat>();
                                messageChats = messageChatMedthod.getMessageChatsByChannelId(clientRequest.Channel_id, clientRequest.Reader);
                                ResponseToClient(client, "MESSAGELIST", messageChats);
                            }
                            break;
                        case "SEENMESSAGELIST":
                            {
                                MESSAGELIST? clientRequest = JsonSerializer.Deserialize<MESSAGELIST>(conn.Content);

                                MessageChat messageChatMedthod = new MessageChat();
                                messageChatMedthod.updateMessageType(clientRequest.Channel_id, 1, clientRequest.Reader);

                            }
                            break;
                        case "GETGROUPS":
                            {
                                string username = conn.Content;
                                Channel_Group channel_Group_Method = new Channel_Group();
                                List<Channel_Group> channel_Groups = channel_Group_Method.getChannelGroups(username);
                                ResponseToClient(client, "GROUPLIST", channel_Groups);
                            }
                            break;
                        case "GETFRIENDS":
                            {
                                string username = conn.Content;
                                Channel_Friend channel_Friend = new Channel_Friend();
                                List<Channel_Friend> channel_Friends = new List<Channel_Friend>();

                                channel_Friends = channel_Friend.getChannelFriend(username);
                                ResponseToClient(client, "FRIENDLIST", channel_Friends);
                            
                            }
                            break;
                        case "GETFILEDOWNLOAD":
                            {
                                FILE? fileMessage = JsonSerializer.Deserialize<FILE>(conn.Content);
                                if(fileMessage != null)
                                {
                                    MessageChat messageChatMethod = new MessageChat();
                                    fileMessage.File = messageChatMethod.getMessageFileByMessageId(fileMessage.MessageId);
                                    string json = JsonSerializer.Serialize(fileMessage);
                                    ResponseToClient(client, "FILEDOWNLOAD", fileMessage);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                ReceiveData(client);
            }
            catch (Exception e)
            {
                
            }
        }

        private void AddUser()
        {
            for (int i = 1; i < 11; i++)
            {
                char letter = (char)('A' + (char)((i - 1) % 26));
                DSAccount.Add($"{letter}", "123");
            }
        }

        private void AddGroup()
        {
            int length = DSAccount.Count();
            int groupIndex = 0;
            string groupName = $"Group{groupIndex}";
            DSGroup.Add(groupName, new List<string>());

            for (int i = 0; i < length; i++)
            {
                if (i != 0 && i % 3 == 0)
                {
                    groupIndex += 1;
                    groupName = $"Group{groupIndex}";
                    DSGroup.Add(groupName, new List<string>());
                }
                DSGroup[groupName].Add(DSAccount.ElementAt(i).Key);
            }
        }
      /*  private void ResponseToClient(Socket client, string statusCode, string message)
        {
            Response res = new Response(message);
            Common common = new Common(statusCode, ParseToJson(res));
            SendData(client, common);
        }*/
        private void ResponseToClient(Socket client, string statusCode, object message)
        {
            ResponseClient responseClient = new ResponseClient();
            responseClient.Content = message;
            responseClient.Kind = statusCode;
            SendData(client, responseClient);
        }
        private void SetText(string text)
        {
            if (KQ.InvokeRequired)
            {
                var dlg = new CallDelegate(SetText);
                KQ.Invoke(dlg, new object[] { text });
            }
            else
            {
                KQ.Text += text + Environment.NewLine;
                KQ.SelectionStart = KQ.TextLength;
                KQ.ScrollToCaret();
            }
        }

        private string ParseToJson(object obj)
        {
            return JsonSerializer.Serialize(obj);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            string hostName = Dns.GetHostName();
            string myIP = "";
            IPAddress[] IPArray = Dns.GetHostByName(hostName).AddressList;
            foreach (IPAddress ip in IPArray)
            {
                if (ip.ToString().Contains('.'))
                {
                    myIP = ip.ToString();
                    break;
                }
            }
            IP.Text = myIP;

            AddUser();
            AddGroup();
        }

    }
}
