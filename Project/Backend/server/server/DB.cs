using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Xml.Linq;
using Microsoft.VisualBasic.ApplicationServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Text.Json;
using System.Reflection;

namespace server
{
    internal class DB
    {
        public MySqlConnection conn;

        public DB()
        {
            string host = "localhost";
            string db = "chat";
            string port = "3306";
            string user = "root";
            string pass = "";
            string constring = "Server=" + host + ";Database=" + db
                + ";port=" + port + ";User Id=" + user + ";password=" + pass;
            try
            {
                conn = new MySqlConnection(constring);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
    class Channel_Group : DB
    {
        int channel_id;
        string name;
        int unredMessage;

        public int Channel_id { get => channel_id; set => channel_id = value; }
        public string Name { get => name; set => name = value; }
        public int UnredMessage { get => unredMessage; set => unredMessage = value; }
        public List<Channel_Group> getChannelGroups(string username)
        {

            List<Channel_Group> channel_Groups = new List<Channel_Group>();
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "select channel.channel_id,channel.name " +
                "from channel,user " +
                "where channel_id in (select channel_id from channel,user " +
                "where user = user.username " +
                "and user = @username " +
                "GROUP BY channel_id) " +
                "and user != @username " +
                "and channel.user = user.username " +
                "and channel.name IS NOT NULL " +
                "GROUP BY channel.channel_id,channel.name";
            cmd.CommandType = CommandType.Text;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.Parameters.AddWithValue("@username", username);
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {

                    while (reader.Read())
                    {
                        Channel_Group channel_Group = new Channel_Group();
                        channel_Group.Channel_id = reader.GetInt32(0);
                        channel_Group.Name = reader.GetString(1);

                        channel_Groups.Add(channel_Group);
                    }

                }
            }
            foreach (Channel_Group item in channel_Groups)
            {
                Channel_Friend channelMethod = new Channel_Friend();
                item.UnredMessage = channelMethod.coutUnredMessageByChannelId(item.Channel_id, username);
            }
            conn.Close();
            return channel_Groups;
        }
        private void insertGroup(int channel_id, string user, string name)
        {
            using (MySqlCommand cmdInsert = new MySqlCommand())
            {

                cmdInsert.CommandText = "INSERT INTO channel(channel_id,user,name) " +
               " VALUES(@channel_id,@user,@name) ";
                cmdInsert.CommandType = CommandType.Text;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmdInsert.Connection = conn;
                cmdInsert.Parameters.AddWithValue("@channel_id", channel_id);
                cmdInsert.Parameters.AddWithValue("@user", user);
                cmdInsert.Parameters.AddWithValue("@name", name);
                cmdInsert.ExecuteReader();
                conn.Close();
            }
        }
        public bool addGroup(List<string> members, string groupName)
        {

            int channel_idMax = 0;
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "SELECT MAX(channel_id) FROM `channel`;";
            cmd.CommandType = CommandType.Text;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;

            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {

                    while (reader.Read())
                    {
                        channel_idMax = reader.GetInt32(0);
                    }

                }
            }
            cmd.ExecuteReader().Close();
            foreach (string member in members)
            {
                insertGroup(channel_idMax + 1, member, groupName);
            }
            conn.Close();
            return true;
        }
    }
    class Channel_Friend : DB
    {
        int channel_id;
        User user;
        int unredMessage;
        public int Channel_id
        {
            get { return channel_id; }
            set { channel_id = value; }
        }
        public User User
        {
            get { return user; }
            set { user = value; }
        }

        public int UnredMessage { get => unredMessage; set => unredMessage = value; }

        public int coutUnredMessageByChannelId(int channelId, string userseen)
        {

            int count = 0;
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "SELECT COUNT(message_id) " +
                "FROM message " +
                "WHERE channel_id = @channel_id " +
                "AND sender != @sender " +
                "AND message_type = 0 ";
            cmd.CommandType = CommandType.Text;

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.Parameters.AddWithValue("@channel_id", channelId);
            cmd.Parameters.AddWithValue("@sender", userseen);
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {

                    while (reader.Read())
                    {
                        count = reader.GetInt32(0);
                    }

                }
            }
            conn.Close();
            return count;
        }
        public List<Channel_Friend> getChannelFriend(string username)
        {

            List<Channel_Friend> channel_Friends = new List<Channel_Friend>();
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "select channel.channel_id,user.username,user.password,user.name " +
                "from channel,user " +
                "where channel_id in " +
                "(select channel_id from channel,user " +
                "where user = user.username " +
                "and user = @username " +
                "GROUP BY channel_id) " +
                "and user != @username " +
                "and channel.user = user.username " +
                "and channel.name IS NULL";
            cmd.CommandType = CommandType.Text;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.Parameters.AddWithValue("@username", username);
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {

                    while (reader.Read())
                    {
                        Channel_Friend channel_Friend = new Channel_Friend();
                        channel_Friend.Channel_id = reader.GetInt32(0);
                        User user = new User();
                        user.Username = reader.GetString(1);
                        user.Password = reader.GetString(2);
                        user.Name = reader.GetString(3);
                        channel_Friend.User = user;

                        channel_Friends.Add(channel_Friend);
                    }

                }
            }
            foreach (Channel_Friend item in channel_Friends)
            {
                item.UnredMessage = coutUnredMessageByChannelId(item.Channel_id, username);
            }
            conn.Close();
            return channel_Friends;
        }
        private void insertChannel(int channel_id, string username)
        {
            using (MySqlCommand cmdInsert = new MySqlCommand())
            {

                cmdInsert.CommandText = "INSERT INTO channel(channel_id,user) " +
               " VALUES(@channel_id,@user) ";
                cmdInsert.CommandType = CommandType.Text;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmdInsert.Connection = conn;
                cmdInsert.Parameters.AddWithValue("@channel_id", channel_id);
                cmdInsert.Parameters.AddWithValue("@user", username);

                cmdInsert.ExecuteReader();
                conn.Close();
            }
        }
        private bool checkUsername(string username)
        {
            using (MySqlCommand cmd = new MySqlCommand())
            {

                cmd.CommandText = "SELECT * FROM user WHERE username=@username ";
                cmd.CommandType = CommandType.Text;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@username", username);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        conn.Close();
                        return true;

                    }
                }
                conn.Close();
            }
            return false;
        }
        public int addFriend(string username, string friend)
        {

            if (!checkUsername(friend))
            {
                return -1;
            }

            List<Channel_Friend> friendList = getChannelFriend(username);
            foreach (Channel_Friend channel_Friend in friendList)
            {
                if (channel_Friend.User.Username == friend)
                {
                    conn.Close();
                    return 0;
                }
            }

            int channel_idMax = 0;
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "SELECT MAX(channel_id) FROM `channel`;";
            cmd.CommandType = CommandType.Text;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            try
            {
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {

                        while (reader.Read())
                        {
                            channel_idMax = reader.GetInt32(0);

                        }

                    }
                }
            }
            catch (Exception ex)
            {

            }

            cmd.ExecuteReader().Close();
            insertChannel(channel_idMax + 1, username);
            insertChannel(channel_idMax + 1, friend);
            conn.Close();
            return 1;
        }
    }
    class Channel : DB
    {
        private int channel_id;
        private string username;
        private string name;

        public int Channel_id { get => channel_id; set => channel_id = value; }
        public string Username { get => username; set => username = value; }
        public string Name { get => name; set => name = value; }

        public void deleteChannel(int channel_id)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "DELETE FROM message WHERE channel_id = @channel_id ";
            cmd.CommandType = CommandType.Text;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.Parameters.AddWithValue("@channel_id", channel_id);
            cmd.ExecuteReader().Close();

            cmd.CommandText = "DELETE FROM channel WHERE channel_id = @channel_id ";
            cmd.CommandType = CommandType.Text;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.ExecuteReader();
            conn.Close();
        }
        public List<string> getUsernameListByChannelId(int channel_id, string sender)
        {

            List<string> usernames = new List<string>();

            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "SELECT user FROM channel WHERE channel_id = @channel_id and user != @sender";
            cmd.CommandType = CommandType.Text;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.Parameters.AddWithValue("@channel_id", channel_id);
            cmd.Parameters.AddWithValue("@sender", sender);
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {

                    while (reader.Read())
                    {
                        string username = reader.GetString(0);
                        usernames.Add(username);
                    }

                }
            }
            conn.Close();
            return usernames;
        }

    }
    class User : DB
    {
        string username;
        string password;
        string name;
        public string Username
        {
            get { return username; }
            set { username = value; }
        }
        public string Password
        {
            get { return password; }
            set { password = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public User login(string username, string password)
        {
            string hashedPassword = new Utils().CalculateMD5Hash(password);
            User user = null;
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "SELECT * FROM user where username = @username and password = @password";
            cmd.CommandType = CommandType.Text;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", hashedPassword);
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {

                    while (reader.Read())
                    {
                        user = new User();
                        user.Username = reader.GetString(0);
                        user.Password = reader.GetString(1);
                        //user.Name = reader.GetString(2);
                    }

                }
            }
            conn.Close();
            return user;
        }
        public bool Register(string username, string password)
        {
            string hashedPassword = new Utils().CalculateMD5Hash(password);
            User user = null;
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "SELECT * FROM user where username = @username";
            cmd.CommandType = CommandType.Text;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.Parameters.AddWithValue("@username", username);
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {

                    while (reader.Read())
                    {
                        user = new User();
                        user.Username = reader.GetString(0);
                        user.Password = reader.GetString(1);
                        user.Name = reader.GetString(2);
                    }

                }
            }
            if (user != null)
            {
                conn.Close();
                return false;
            }
            else
            {
                cmd.CommandText = "INSERT INTO user(username,password) " +
                    "VALUES(@username,@password)";
                cmd.CommandType = CommandType.Text;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Connection = conn;
                //cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", hashedPassword);
                cmd.ExecuteReader();
                conn.Close();
                return true;
            }
        }
    }
    class MessageChat : DB
    {
        int message_id;
        string sender;
        string username;
        int channel_id;
        string message;
        int message_type;
        string date;
        string file_name;
        byte[] file;
        int kind = 0;
        int maxSize = 1000000;
        public string Sender { get => sender; set => sender = value; }
        public int Message_id { get => message_id; set => message_id = value; }
        public string Message { get => message; set => message = value; }
        public int Message_type { get => message_type; set => message_type = value; }
        public int Channel_id { get => channel_id; set => channel_id = value; }
        public string Date { get => date; set => date = value; }
        public string Username { get => username; set => username = value; }
        public string File_name { get => file_name; set => file_name = value; }
        public byte[] File { get => file; set => file = value; }
        public int Kind { get => kind; set => kind = value; }

        public void AddMessageFileToChannelId(int channel_id, string sender, byte[] file, string fileName, int kind)
        {

            MySqlCommand cmd = new MySqlCommand();
            string sqlUpdateMessage = "insert into message(sender,channel_id,file,file_name,kind) " +
                "values(@sender,@channel_id,@file,@file_name,@kind)";
            cmd.CommandText = sqlUpdateMessage;
            cmd.CommandType = CommandType.Text;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.Parameters.AddWithValue("@sender", sender);
            cmd.Parameters.AddWithValue("@channel_id", channel_id);
            cmd.Parameters.AddWithValue("@file", file);
            cmd.Parameters.AddWithValue("@file_name", fileName);
            cmd.Parameters.AddWithValue("@kind", kind);
            cmd.ExecuteReader();

            conn.Close();
        }
        public void AddMessageToChannelId(int channel_id, string sender, string content)
        {

            MySqlCommand cmd = new MySqlCommand();
            string sqlUpdateMessage = "insert into message(sender,channel_id,message,kind) " +
                "values(@sender,@channel_id,@content,0)";
            cmd.CommandText = sqlUpdateMessage;
            cmd.CommandType = CommandType.Text;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.Parameters.AddWithValue("@sender", sender);
            cmd.Parameters.AddWithValue("@channel_id", channel_id);
            cmd.Parameters.AddWithValue("@content", content);
            cmd.ExecuteReader();


            conn.Close();
        }
        public static byte[] ReadBlob(DbDataReader reader, int ordinal)
        {
            byte[] blobContent;
            int bufferSize = 1024;
            byte[] outByte = new byte[bufferSize];
            long retval;
            long startIndex = 0;

            MemoryStream memStream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(memStream))
            {

                retval = reader.GetBytes(ordinal, startIndex, outByte, 0, bufferSize);

                while (retval == bufferSize)
                {
                    writer.Write(outByte);
                    writer.Flush();

                    startIndex += bufferSize;
                    retval = reader.GetBytes(ordinal, startIndex, outByte, 0, bufferSize);
                }


                writer.Write(outByte, 0, (int)retval);
                writer.Flush();

                memStream.Position = 0;
                blobContent = memStream.ToArray();
            }
            memStream.Close();
            return blobContent;
        }

        public byte[] getMessageFileByMessageId(int messageId)
        {

            byte[]? file = null;
            List<MessageChat> messageChats = new List<MessageChat>();
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "SELECT file " +
                " FROM message " +
                " WHERE message_id = @message_id ";
            cmd.CommandType = CommandType.Text;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.Parameters.AddWithValue("@message_id", messageId);
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        file = ReadBlob(reader, 0);
                    }
                }
            }
            conn.Close();
            return file;
        }
        public void updateMessageType(int channel_id, int message_type, string username)
        {

            List<MessageChat> messageChats = new List<MessageChat>();
            MySqlCommand cmd = new MySqlCommand();
            string sqlUpdateMessage = "update message " +
                "set message_type = @message_type " +
                "where channel_id = @channelId " +
                "and sender != @sender";
            cmd.CommandText = sqlUpdateMessage;
            cmd.CommandType = CommandType.Text;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.Parameters.AddWithValue("@channelId", channel_id);
            cmd.Parameters.AddWithValue("@sender", username);
            cmd.Parameters.AddWithValue("@message_type", message_type);
            cmd.ExecuteReader();
            conn.Close();
        }
        public List<MessageChat> getMessageChatsByChannelId(int channel_id, string userRead)
        {

            List<MessageChat> messageChats = new List<MessageChat>();
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "SELECT message_id,sender,message,message_type,date,kind,file_name,file,user.name " +
                " FROM message,user " +
                " WHERE channel_id = @channelId " +
                " AND user.username = message.sender" +
                " GROUP BY message_id,sender,message,message_type,date,file_name,file,kind,user.name " +
                " ORDER BY date ASC";
            cmd.CommandType = CommandType.Text;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.Parameters.AddWithValue("@channelId", channel_id);
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {

                    while (reader.Read())
                    {
                        MessageChat messageChat = new MessageChat();
                        messageChat.Message_id = reader.GetInt32(0);
                        messageChat.sender = reader.GetString(1);

                        messageChat.message_type = reader.GetInt32(3);
                        DateTime date = reader.GetDateTime(4);
                        messageChat.Date = date.ToString();
                        messageChat.Kind = reader.GetInt32(5);

                        /* if(messageChat.Kind > 0)
                         {
                             messageChat.File_name = reader.GetString(6);

                             messageChat.File = ReadBlob(reader, 7);
                             //reader.GetBytes(7, 0, messageChat.File, 0, messageChat.maxSize);
                             //messageChat.JsonFile = JsonSerializer.Serialize(messageChat.File);
                         }
                         else
                         {

                         }*/
                        if (messageChat.Kind > 0)
                        {
                            messageChat.File_name = reader.GetString(6);
                            if (messageChat.Kind == 1)
                            {
                                messageChat.File = ReadBlob(reader, 7);
                            }
                        }
                        if (messageChat.Kind == 0)
                        {
                            messageChat.message = reader.GetString(2);
                        }
                        messageChat.Username = reader.GetString(8);

                        messageChats.Add(messageChat);

                    }

                }
            }
            string sqlUpdateMessage = "update message " +
                "set message_type = 1 " +
                "where channel_id = @channelId " +
                "and sender != @sender";
            cmd.CommandText = sqlUpdateMessage;
            cmd.CommandType = CommandType.Text;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.Parameters.AddWithValue("@sender", userRead);
            cmd.ExecuteReader();
            conn.Close();
            return messageChats;
        }
    }
}
