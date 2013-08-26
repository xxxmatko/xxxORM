using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using xDev.Data;

namespace ConsoleApp
{
    // TODO : READ ENTITIES FORM DB FOR CUSTOM SELECT AND TYPE AND RETURN IENUMERABLE
    // TODO : Check ObjectContext - continue work, test creating connection and command execution, transaction maybe
    // TODO : Traversing new Expression and get column names for it
    class Program
    {
        static void Main(string[] args)
        {
            //TestUserManagementContext();
            //TestListOfInts();
            //TestGetterForUser();
            //TestValidateUser();
            //TestMetaInfoForUser();
            //TestConstructor();
            TestReadingData();
        }


        private static void TestReadingData()
        {
            Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "Id", "Login", "Password", "DepartmentId", "IsLocked");
            using (var db = new UserManagementContext())
            {
                var users = db.ExecuteStoreQuery<User>("SELECT * FROM USERS");
                //var users = db.ExecuteStoreQuery<User>("SELECT LOGIN FROM USERS");
                foreach(var user in users)
                {
                    Console.WriteLine("{0}\t{1}\t{2}\t\t{3}\t\t{4}", user.Id, user.Login, user.Password, user.DepartmentId, user.IsLocked);
                }
            }
        }


        private static void TestConstructor()
        {
            /*
            MethodInfo ChangeTypeMethod = typeof(Convert).GetMethod("ChangeType", new[] { typeof(object), typeof(Type) });
            Expression convertedObject = Expression.Call(ChangeTypeMethod, Expression.Constant("123"), Expression.Constant(typeof(int)));
            Expression converted = Expression.Convert(convertedObject, typeof(int));
            */

            //var userMeta = new User().GetMetaInfo();
            var userMeta = MetaInfo<User>.GetMetaInfo();
            var dynamicUser = userMeta.Create(1, "dynamicLogin", "dynamicPassword", 123);

            Console.WriteLine("User Id:");
            Console.WriteLine(dynamicUser.Id);

            Console.WriteLine("User Login:");
            Console.WriteLine(dynamicUser.GetValue("Login"));

            var dynamicUser2 = userMeta.Create(
                new Tuple<string, object>("Id", 9877)
            );
            Console.WriteLine("User Id:");
            Console.WriteLine(dynamicUser2.Id);
        }


        private static void TestUserManagementContext()
        {
            using (var db = new UserManagementContext())
            {
//                db.ExecuteStoreCommand(@"CREATE TABLE USERS
//                (
//                    ID int,
//                    LOGIN nvarchar(100),
//                    PASSWORD nvarchar(100),
//                    DEPARTMENT_ID int
//                );");
                //db.ExecuteStoreCommand(@"INSERT INTO USERS (ID,LOGIN,PASSWORD,DEPARTMENT_ID) VALUES ({0},{1},{2},{3});", 1, "user1", "user1psw", 123);
                //db.ExecuteStoreCommand(@"INSERT INTO USERS (ID,LOGIN,PASSWORD,DEPARTMENT_ID) VALUES ({0},{1},{2},{3});", 2, "user2", "user2psw", 234);
            }
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
