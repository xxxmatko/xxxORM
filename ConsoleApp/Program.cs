using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using xDev.Data;

namespace ConsoleApp
{
    // TODO : Define connection strin as in entity framewrok, where provider name means queryprovider, and in connection string there will be real connection string
    // TODO : Finish ExecuteStoreCommand - test it against database, implement other commands and test them, then provider it self
    // TODO : Check ObjectContext - continue work, test creating connection and command execution, transaction maybe
    // TODO : Traversing new Expression and get column names for it
    // TODO : Add to MetaInfo delegate which creates and binds entity properties
    class Program
    {
        static void Main(string[] args)
        {
            TestListOfInts();
            TestGetterForUser();
            TestValidateUser();
            TestMetaInfoForUser();
        }


        public static void TestListOfInts()
        {
            var listOfInts = new List<int>
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9
            };
            Console.WriteLine(listOfInts.Any(x => x > 1));
        }


        public static void TestGetterForUser()
        {
            var user = new User(123, "usr", "usrpwd", 0);
            var userMeta = user.GetMetaInfo();
            
            Console.WriteLine("User Id:");
            // Unable to do this, it is internal property now
            //Console.WriteLine(userMeta.Getters["Id"].DynamicInvoke(user));
            
            Console.WriteLine("User Login:");
            Console.WriteLine(user.GetValue("Login"));
        }


        private static void TestValidateUser()
        {
            var user = new User();
            var userMeta = user.GetMetaInfo();

            bool isUserValid = user.IsValid();
            Console.WriteLine("Is user valid: {0}", isUserValid);
            if (!isUserValid)
            {
                foreach (var validation in user.Validate())
                {
                    Console.WriteLine("{0} : {1}", string.Join(",", validation.MemberNames), validation.ErrorMessage);
                }
            }


            // Valid user
            var validUser = new User(1, "usrlogin", "usrpsw", 123);
            Console.WriteLine("Valid user: {0}", validUser.IsValid());
            foreach (var validation in validUser.Validate())
            {
                Console.WriteLine("{0} : {1}", string.Join(",", validation.MemberNames), validation.ErrorMessage);
            }
        }


        public static void TestMetaInfoForUser()
        {
            var user = new User();
            var userMetaInfo0 = user.GetMetaInfo();
            
            Console.WriteLine(user.Id);
            user.SetValue("Id", 987);
            Console.WriteLine(user.Id);

            Console.WriteLine(user.Login);
            user.SetValue("Login", "usr1!");
            Console.WriteLine(user.Login);

            var userMetaInfo1 = user.GetMetaInfo();

            var dep = new Department();
            var depMetaInfo0 = dep.GetMetaInfo();

            var role = new Role();
            var roleMetaInfo0 = role.GetMetaInfo();
        }
    }
}
