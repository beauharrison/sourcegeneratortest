using System;
using GeneratedModels;

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
