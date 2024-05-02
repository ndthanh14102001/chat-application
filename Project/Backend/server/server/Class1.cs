using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace server
{
    public class FILE
    {
        int messageId;
        string path;
        byte[] file;

        public int MessageId { get => messageId; set => messageId = value; }
        public string Path { get => path; set => path = value; }
        public byte[] File { get => file; set => file = value; }
    }
    public class SENDFILE
    {
        private string sender;
        private int channel_id;
        private string fileName;
        private byte[] file;

        public SENDFILE(byte[] file, string fileName, int channel_id, string sender)
        {
            this.file = file;
            this.fileName = fileName;
            this.channel_id = channel_id;
            this.sender = sender;
        }

        public byte[] File { get => file; set => file = value; }
        public string FileName { get => fileName; set => fileName = value; }
        public int Channel_id { get => channel_id; set => channel_id = value; }
        public string Sender { get => sender; set => sender = value; }
    }
    public class ADDFRIEND
    {
        private string username;
        private string friend;
        public ADDFRIEND(string username, string friend)
        {
            this.Username = username;
            this.Friend = friend;
        }

        public string Username { get => username; set => username = value; }
        public string Friend { get => friend; set => friend = value; }
    }
    public class RESPONSELOGIN
    {
        string kind;
        object content;

        public string Kind { get => kind; set => kind = value; }
        public object Content { get => content; set => content = value; }
    }
    public class MESSAGELIST
    {
        private string reader;
        private int channel_id;

        public string Reader { get => reader; set => reader = value; }
        public int Channel_id { get => channel_id; set => channel_id = value; }
    }
    public class ResponseClient
    {
        private string kind;
        private object _object;
        
        public string Kind { get => kind; set => kind = value; }
        public object Content { get => _object; set => _object = value; }
    }
    public class Common
    {
        private string kind;
        private string content;

        public Common(string kind, string content)
        {
            this.kind = kind;
            this.content = content;
        }

        public string Kind { get => kind; set => kind = value; }
        public string Content { get => content; set => content = value; }
    }
   
    public class Login
    {
        private string username;
        private string password;

        public Login(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
    }

    public class Logout
    {
        private string username;

        public Logout(string username)
        {
            this.username = username;
        }

        public string Username { get => username; set => username = value; }
    }
    
    public class Chat
    {
        private string sender;
        private string content;
        private int channel_id;
        private string fileName;
        private byte[] file;
        private int kind;
        public string Sender { get => sender; set => sender = value; }
        public string Content { get => content; set => content = value; }
        public int Channel_id { get => channel_id; set => channel_id = value; }
        public string FileName { get => fileName; set => fileName = value; }
        public byte[] File { get => file; set => file = value; }
        public int Kind { get => kind; set => kind = value; }
    }

    public class Response
    {
        private string content;

        public Response(string content)
        {
            this.content = content;
        }

        public string Content { get => content; set => content = value; }
    }

    public class Group
    {
        private string creater;
        private string groupName;
        private object listUsername;
        public Group(string creater, string groupName, object listUsername)
        {
            this.creater = creater;
            this.groupName = groupName;
            this.listUsername = listUsername;
        }

        public string GroupName { get => groupName; set => groupName = value; }
        public object ListUsername { get => listUsername; set => listUsername = value; }
        public string Creater { get => creater; set => creater = value; }
    }
}
