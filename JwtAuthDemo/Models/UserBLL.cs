using JWTAuthDemo;
using Microsoft.Extensions.Options;

namespace JWTTest.Models
{
    public class UserBLL
    {
        public UserBLL()
        { 
        
        }
        public User GetUser(string username, string pwd)
        {
            return new User() { ID=1, username = username, pwd = pwd };
        }
    }

    public class User
    {
        public int ID { get; set; }
        public string username { get; set; }

        public string pwd { get; set; }

    }
}
