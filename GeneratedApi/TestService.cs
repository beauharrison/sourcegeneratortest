using System;
using GeneratedApi.Models;

namespace GeneratedApi
{
    public class TestService
    {

        public void Add(UserModel user)
        {
            Console.WriteLine(user.Name);
        }
    }
}
